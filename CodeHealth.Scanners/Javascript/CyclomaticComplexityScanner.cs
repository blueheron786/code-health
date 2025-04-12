namespace CodeHealth.Scanners.JavaScript;

using CodeHealth.Core.Dtos.CyclomaticComplexity;
using CodeHealth.Core.IO;
using CodeHealth.Scanners.Common;
using System.Text.Json;
using System.Text.RegularExpressions;

public class CyclomaticComplexityScanner
{
    private static readonly Regex MethodRegex = new(@"function\s+(\w+)\s*\(", RegexOptions.Compiled);
    private static readonly Regex ArrowFunctionRegex = new(@"(\w+)\s*=\s*\((.*?)\)\s*=>", RegexOptions.Compiled);

    private static readonly Regex ComplexityRegex = new(@"(\bif\b|\bfor\b|\bwhile\b|\bcase\b|\bcatch\b|\?\s|&&|\|\|)", RegexOptions.Compiled);

    public void AnalyzeFiles(Dictionary<string, string> sourceFiles, string rootPath, string outputDir)
    {
        var report = new Report();

        foreach (var kvp in sourceFiles)
        {
            string filePath = kvp.Key;
            string content = kvp.Value;

            var fileResult = new FileResult
            {
                File = Path.GetRelativePath(rootPath, filePath).Replace("\\", "/")
            };

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

                report.TotalComplexity += complexity;
            }

            if (fileResult.Methods.Any())
                report.Files.Add(fileResult);
        }

        CyclomaticComplexityReporter.FinalizeReport(report, outputDir, "cyclomatic_complexity.javascript.json");
    }
}

