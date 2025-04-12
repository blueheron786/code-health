namespace CodeHealth.Scanners;

using CodeHealth.Core.Dtos.CyclomaticComplexity;
using CodeHealth.Scanners.Common;
using System.Text.RegularExpressions;

public class JavascriptTypescriptCyclomaticComplexityScanner
{
    public readonly string[] FileExtensions = [".js", ".jsx", ".ts", ".tsx"];

    private static readonly Regex MethodRegex = new(@"function\s+(\w+)\s*\(", RegexOptions.Compiled);
    private static readonly Regex ArrowFunctionRegex = new(@"(\w+)\s*=\s*\((.*?)\)\s*=>", RegexOptions.Compiled);

    private static readonly Regex ComplexityRegex = new(@"(\bif\b|\bfor\b|\bwhile\b|\bcase\b|\bcatch\b|\?\s|&&|\|\|)", RegexOptions.Compiled);

    public void AnalyzeFiles(Dictionary<string, string> sourceFiles, string rootPath, string outputDir)
    {
        var javascriptReport = new Report();
        var typescriptReport = new Report();

        foreach (var kvp in sourceFiles)
        {
            string filePath = kvp.Key;
            string content = kvp.Value;

           if (!FileExtensions.Any(ext => filePath.EndsWith(ext, StringComparison.OrdinalIgnoreCase)))
            {
                continue;
            }

            var fileResult = new FileResult
            {
                File = Path.GetRelativePath(rootPath, filePath).Replace("\\", "/")
            };

            var isTypescript = filePath.Contains(".ts", StringComparison.OrdinalIgnoreCase) ||
                filePath.Contains(".tsx", StringComparison.OrdinalIgnoreCase);

            // Combine function/arrow-func detection
            var matches = MethodRegex.Matches(content)
                .Cast<Match>()
                .Concat(ArrowFunctionRegex.Matches(content).Cast<Match>());

            foreach (var match in matches)
            {
                string methodName = match.Groups[1].Value;

                // This is a naive way — just count all complexity-increasing constructs in the file
                // In the future, you could scope this by function
                int complexity = 1 + ComplexityRegex.Matches(content).Count;

                fileResult.Methods.Add(new MethodResult
                {
                    Method = methodName,
                    Complexity = complexity
                });

                if (isTypescript) {
                    typescriptReport.TotalComplexity += complexity;
                } else {
                    javascriptReport.TotalComplexity += complexity;
                }
            }

            if (fileResult.Methods.Any())
            {
                if (isTypescript) {
                    typescriptReport.Files.Add(fileResult);
                } else {
                    javascriptReport.Files.Add(fileResult);
                }
            }
        }

        CyclomaticComplexityReporter.FinalizeReport(javascriptReport, outputDir, "cyclomatic_complexity.javascript.json");
        // Necessary for proper file-level tagging of language
        CyclomaticComplexityReporter.FinalizeReport(typescriptReport, outputDir, "cyclomatic_complexity.typescript.json");
    }
}

