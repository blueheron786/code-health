using CodeHealth.Core.Dtos.CyclomaticComplexity;
using CodeHealth.Core.IO;
using System.Text.RegularExpressions;
using System.Text.Json;
using CodeHealth.Scanners.Common;

namespace CodeHealth.Scanners.Java;

// NOTE: This is a lightweight, token-based Java cyclomatic complexity analyzer.
// ✔ Works well with traditional Java (6–11) codebases using explicit method bodies and common control flow.
// ⚠ Not guaranteed to handle modern Java (14+) features like lambdas, pattern matching, records, etc.
// ❌ Does NOT use a real Java parser — it's optimized for speed, not accuracy.
public class CyclomaticComplexityScanner : IStaticCodeScanner
{
    public void AnalyzeFiles(Dictionary<string, string> sourceFiles, string rootPath, string outputDir)
    {
        var report = new Report();

        foreach (var kvp in sourceFiles)
        {
            string fileName = kvp.Key;
            string code = kvp.Value;

            // Preprocess to strip comments & strings
            code = StripCommentsAndStrings(code);

            var fileResult = new FileResult
            {
                File = Path.GetRelativePath(rootPath, fileName).Replace("\\", "/")
            };

            // Use regex to find method declarations
            var methodMatches = Regex.Matches(code, @"(?:public|private|protected)?\s*(?:static\s+)?[\w<>\[\]]+\s+(\w+)\s*\([^)]*\)\s*(\{?)");

            foreach (Match match in methodMatches)
            {
                string methodName = match.Groups[1].Value;
                int methodStart = match.Index;
                string methodBlock = ExtractMethodBody(code, methodStart);

                int complexity = CountDecisionPoints(methodBlock) + 1;

                fileResult.Methods.Add(new MethodResult
                {
                    Method = methodName,
                    Complexity = complexity
                });

                report.TotalComplexity += complexity;
            }

            if (fileResult.Methods.Any())
            {
                report.Files.Add(fileResult);
            }
        }

        CyclomaticComplexityReporter.FinalizeReport(report, outputDir, "cyclomatic_complexity.java.json");
    }

    private static string StripCommentsAndStrings(string code)
    {
        // Remove strings
        code = Regex.Replace(code, "\"(?:\\\\.|[^\"\\\\])*\"", "\"\"");
        // Remove single-line comments
        code = Regex.Replace(code, @"//.*", "");
        // Remove multi-line comments
        code = Regex.Replace(code, @"/\*.*?\*/", "", RegexOptions.Singleline);
        return code;
    }

    private static int CountDecisionPoints(string code)
    {
        // Simple token-based matchers
        var patterns = new[]
        {
            "if\\s*\\(", "for\\s*\\(", "while\\s*\\(", "case\\s+[^:]+:", "catch\\s*\\(",
            "&&", "\\|\\|", "\\?\\s*" // ternary operator
        };

        return patterns.Sum(p => Regex.Matches(code, p).Count);
    }

    private static string ExtractMethodBody(string code, int startIndex)
    {
        int braceCount = 0;
        int i = code.IndexOf('{', startIndex);
        if (i == -1) return ""; // no body found

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
