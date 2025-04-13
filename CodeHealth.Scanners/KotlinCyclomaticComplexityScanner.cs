namespace CodeHealth.Scanners
{
    using CodeHealth.Core.Dtos.CyclomaticComplexity;
    using System.Text.RegularExpressions;
    using CodeHealth.Scanners.Common;

    public class KotlinCyclomaticComplexityScanner : IStaticCodeScanner
    {
        public const string FileExtension = ".kt";
        
        // Regex for method detection (Kotlin's 'fun' keyword)
        private const string MethodPattern = @"(?:fun)\s+(\w+)\s*\([^)]*\)\s*(\{?)";
        
        // Regex patterns to strip out string literals and comments
        private const string StringLiteralPattern = "\"(?:\\\\.|[^\"\\\\])*\"";
        private const string SingleLineCommentPattern = @"//.*";
        private const string MultiLineCommentPattern = @"/\*.*?\*/";
        
        // Patterns for decision points that contribute to cyclomatic complexity
        private static readonly string[] DecisionPointPatterns = new[] 
        {
            @"if\s*\(",    // if statements
            @"for\s*\(",   // for loops
            @"while\s*\(", // while loops
            @"when\s*\(",  // Kotlin's 'when' expressions (similar to switch)
            @"&&",         // Logical AND
            @"\|\|",       // Logical OR
            @"\?\s*"       // Ternary operator
        };

        public void AnalyzeFiles(Dictionary<string, string> sourceFiles, string rootPath, string outputDir)
        {
            var report = new Report();

            // Iterate through the source files
            foreach (var kvp in sourceFiles)
            {
                string fileName = kvp.Key;
                string code = kvp.Value;

                // Only process Kotlin files (.kt)
                if (!fileName.EndsWith(FileExtension, StringComparison.OrdinalIgnoreCase))
                    continue;

                // Preprocess the code to remove comments and strings
                code = StripCommentsAndStrings(code);
                string[] lines = code.Split('\n');

                // Use regex to find all Kotlin method declarations
                var methodMatches = Regex.Matches(code, MethodPattern);
                foreach (Match match in methodMatches)
                {
                    string methodName = match.Groups[1].Value;
                    int methodStartPos = match.Index;
                    
                    // Calculate the start and end lines of the method
                    int startLine = GetLineNumber(code, methodStartPos, lines);
                    string methodBlock = ExtractMethodBody(code, methodStartPos);
                    int endLine = GetLineNumber(code, methodStartPos + methodBlock.Length, lines);

                    // Calculate cyclomatic complexity (decision points + 1)
                    int complexity = CountDecisionPoints(methodBlock) + 1;

                    // Add the method result to the report
                    var issueResult = new IssueResult
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

                    // Add the issue result to the report
                    report.Issues.Add(issueResult);
                    report.TotalComplexity += complexity;
                }
            }

            // Finalize the report and save it as a JSON file
            CyclomaticComplexityReporter.FinalizeReport(report, outputDir, "cyclomatic_complexity.kotlin.json");
        }

        private static string StripCommentsAndStrings(string code)
        {
            // Strip out string literals and single-line comments
            code = Regex.Replace(code, StringLiteralPattern, "\"\"");
            code = Regex.Replace(code, SingleLineCommentPattern, "");
            // Strip multi-line comments (non-greedy)
            return Regex.Replace(code, MultiLineCommentPattern, "", RegexOptions.Singleline);
        }

        private static int CountDecisionPoints(string code)
        {
            // Count decision points (if, for, while, etc.)
            return DecisionPointPatterns.Sum(pattern => Regex.Matches(code, pattern).Count);
        }

        private static int GetLineNumber(string code, int position, string[] lines)
        {
            int currentPos = 0;
            for (int i = 0; i < lines.Length; i++)
            {
                currentPos += lines[i].Length + 1;
                if (currentPos > position)
                    return i + 1; // Line numbers are 1-based
            }
            return 1;
        }

        private static string ExtractMethodBody(string code, int startIndex)
        {
            int braceCount = 0;
            int i = code.IndexOf('{', startIndex);
            if (i == -1) {
                return ""; // Return empty if no method body found
            }

            int start = i;
            for (; i < code.Length; i++)
            {
                if (code[i] == '{') {
                    braceCount++;
                } else if (code[i] == '}') {
                    braceCount--;
                }
                
                if (braceCount == 0) {
                    break; // End of method body
                }
            }
            return i < code.Length ? code.Substring(start, i - start + 1) : "";
        }
    }
}
