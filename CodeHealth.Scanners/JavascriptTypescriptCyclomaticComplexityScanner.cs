namespace CodeHealth.Scanners
{
    using CodeHealth.Core.Dtos.CyclomaticComplexity;
    using CodeHealth.Scanners.Common;
    using System.Text.RegularExpressions;
    using System.Linq;

    public class JavascriptTypescriptCyclomaticComplexityScanner
    {
        public readonly string[] FileExtensions = { ".js", ".jsx", ".ts", ".tsx" };

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
                    complexity += ComplexityRegex.Matches(content).Count;

                    var issueResult = new IssueResult
                    {
                        File = Path.GetRelativePath(rootPath, filePath).Replace("\\", "/"),
                        Line = startLine,
                        EndLine = endLine,
                        Name = methodName,
                        Metric = new Metric
                        {
                            Name = "Cyclomatic Complexity",
                            Value = complexity,
                            Threshold = 10 // Example threshold for high complexity
                        },
                        Message = $"Method '{methodName}' has a cyclomatic complexity of {complexity}.",
                        CodeSnippet = new List<string>
                        {
                            content.Substring(match.Index, Math.Min(100, content.Length - match.Index)) // Capture first 100 characters or less
                        },
                        Severity = complexity > 10 ? "High" : "Medium", // Simple severity based on complexity value
                        Suggestion = "Consider refactoring the method to reduce complexity.",
                        Tags = new List<string> { "complexity", "refactor" },
                        Fixable = true // This can be updated later if refactoring suggestions are automated
                    };

                    // Add to the appropriate report
                    if (isTypescript)
                    {
                        typescriptReport.TotalComplexity += complexity;
                        typescriptReport.Issues.Add(issueResult);
                    }
                    else
                    {
                        javascriptReport.TotalComplexity += complexity;
                        javascriptReport.Issues.Add(issueResult);
                    }
                }
            }

            // Finalize the report output for both JavaScript and TypeScript
            CyclomaticComplexityReporter.FinalizeReport(javascriptReport, outputDir, "cyclomatic_complexity.javascript.json");
            CyclomaticComplexityReporter.FinalizeReport(typescriptReport, outputDir, "cyclomatic_complexity.typescript.json");
        }
    }
}
