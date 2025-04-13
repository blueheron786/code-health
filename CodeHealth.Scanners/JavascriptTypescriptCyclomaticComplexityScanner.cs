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

            // Get all lines for line number calculation
            var lines = content.Split(new[] { '\r', '\n' }, StringSplitOptions.RemoveEmptyEntries);
            
            // Combine function/arrow-func detection
            var matches = MethodRegex.Matches(content)
                .Cast<Match>()
                .Concat(ArrowFunctionRegex.Matches(content).Cast<Match>());

            foreach (var match in matches)
            {
                string methodName = match.Groups[1].Value;
                
                // Calculate start line
                int startLine = 1;
                int position = match.Index;
                for (int i = 0; i < lines.Length; i++)
                {
                    if (position <= 0)
                    {
                        startLine = i + 1;
                        break;
                    }
                    position -= lines[i].Length + Environment.NewLine.Length;
                }

                // Naive end line calculation (could be improved with proper parsing)
                int endLine = startLine;
                int complexity = 1; // Start with 1 for the method itself
                
                // Count complexity tokens within the method (naive implementation)
                // This is just a placeholder - you might want to implement proper scope detection
                complexity += ComplexityRegex.Matches(content).Count;

                fileResult.Methods.Add(new MethodResult
                {
                    Method = methodName,
                    Complexity = complexity,
                    StartLine = startLine,
                    EndLine = endLine
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
        CyclomaticComplexityReporter.FinalizeReport(typescriptReport, outputDir, "cyclomatic_complexity.typescript.json");
    }
}