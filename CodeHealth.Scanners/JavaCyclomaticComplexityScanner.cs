namespace CodeHealth.Scanners
{
    using CodeHealth.Core.Dtos;
    using CodeHealth.Core.Dtos.TodoComments;
    using CodeHealth.Scanners.Common;
    using System.Text.RegularExpressions;

    public class JavaCyclomaticComplexityScanner : IStaticCodeScanner
    {
        public readonly string FileExtension = ".java";

        private const string MethodPattern = @"(?:public|private|protected)?\s*(?:static\s+)?[\w<>\[\]]+\s+(\w+)\s*\([^)]*\)\s*(\{?)";
        private static readonly string[] DecisionPatterns = new[] 
        {
            @"if\s*\(", @"for\s*\(", @"while\s*\(", @"case\s+[^:]+:", @"catch\s*\(", @"&&", @"\|\|", @"\?\s*"
        };

        private const string StringPattern = "\"(?:\\\\.|[^\"\\\\])*\"";
        private const string SingleLineCommentPattern = @"//.*";
        private const string MultiLineCommentPattern = @"/\*.*?\*/";

        public void AnalyzeFiles(Dictionary<string, string> sourceFiles, string rootPath, string outputDir)
        {
            var report = new Report();

            foreach (var kvp in sourceFiles)
            {
                string fileName = kvp.Key;
                string originalCode = kvp.Value;

                if (!fileName.EndsWith(FileExtension, StringComparison.OrdinalIgnoreCase))
                {
                    continue;
                }

                // Preprocess to strip comments & strings
                string strippedCode = StripCommentsAndStrings(originalCode);

                var fileResult = new FileResult
                {
                    File = Path.GetRelativePath(rootPath, fileName).Replace("\\", "/")
                };

                // Use regex to find method declarations
                var methodMatches = Regex.Matches(strippedCode, MethodPattern);

                foreach (Match match in methodMatches)
                {
                    string methodName = match.Groups[1].Value;
                    int methodStart = match.Index;
                    string methodBlock = ExtractMethodBody(strippedCode, methodStart);

                    int complexity = CountDecisionPoints(methodBlock) + 1;

                    int charIndexInOriginal = match.Index;
                    int startLine = originalCode.Take(charIndexInOriginal).Count(c => c == '\n') + 1;
                    int endCharIndex = charIndexInOriginal + methodBlock.Length;
                    int endLine = originalCode.Take(endCharIndex).Count(c => c == '\n') + 1;

                    var issue = new IssueResult
                    {
                        File = Path.GetRelativePath(rootPath, fileName).Replace("\\", "/"),
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
                        Severity = complexity > 10 ? "High" : "Medium", // Simple severity based on complexity value
                        Suggestion = "Consider refactoring the method to reduce complexity.",
                        Tags = new List<string> { "complexity", "refactor" },
                        Fixable = true // This can be updated later if refactoring suggestions are automated
                    };

                    report.Issues.Add(issue);
                    report.TotalComplexity += complexity;
                }
            }

            // Calculate Average Complexity
            report.AverageComplexity = report.TotalComplexity / (double)report.Issues.Count;

            // Finalize the report output (e.g., saving it to a JSON file)
            CyclomaticComplexityReporter.FinalizeReport(report, outputDir, "cyclomatic_complexity.java.json");
        }

        private static string StripCommentsAndStrings(string code)
        {
            code = Regex.Replace(code, StringPattern, "\"\"");
            code = Regex.Replace(code, SingleLineCommentPattern, "");
            code = Regex.Replace(code, MultiLineCommentPattern, "", RegexOptions.Singleline);
            return code;
        }

        private static int CountDecisionPoints(string code)
        {
            return DecisionPatterns.Sum(p => Regex.Matches(code, p).Count);
        }

        private static string ExtractMethodBody(string code, int startIndex)
        {
            int braceCount = 0;
            int i = code.IndexOf('{', startIndex);
            if (i == -1) return "";

            int start = i;
            for (; i < code.Length; i++)
            {
                if (code[i] == '{') braceCount++;
                else if (code[i] == '}') braceCount--;

                if (braceCount == 0) break;
            }

            return code.Substring(start, i - start + 1);
        }
    }
}
