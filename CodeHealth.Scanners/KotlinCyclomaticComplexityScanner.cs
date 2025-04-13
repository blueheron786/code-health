namespace CodeHealth.Scanners;

using CodeHealth.Core.Dtos.CyclomaticComplexity;
using System.Text.RegularExpressions;
using CodeHealth.Scanners.Common;

public class KotlinCyclomaticComplexityScanner : IStaticCodeScanner
{
    public const string FileExtension = ".kt";

    public void AnalyzeFiles(Dictionary<string, string> sourceFiles, string rootPath, string outputDir)
    {
        var report = new Report();

        foreach (var kvp in sourceFiles)
        {
            string fileName = kvp.Key;
            string code = kvp.Value;

            if (!fileName.EndsWith(FileExtension, StringComparison.OrdinalIgnoreCase))
            {
                continue;
            }

            // Preprocess to strip comments and strings (will work similarly as with Java)
            code = StripCommentsAndStrings(code);

            var fileResult = new FileResult
            {
                File = Path.GetRelativePath(rootPath, fileName).Replace("\\", "/")
            };

            // Find all method-like declarations using regex
            var methodMatches = Regex.Matches(code, @"(?:fun)\s+(\w+)\s*\([^)]*\)\s*(\{?)");

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
                report.Files.Add(fileResult);
        }

        CyclomaticComplexityReporter.FinalizeReport(report, outputDir, "cyclomatic_complexity.kotlin.json");
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
        // Simple decision points to match
        var patterns = new[]
        {
            "if\\s*\\(", "for\\s*\\(", "while\\s*\\(", "when\\s*\\(", // Kotlin’s "when"
            "&&", "\\|\\|", "\\?\\s*" // Ternary operator-like expressions
        };

        return patterns.Sum(p => Regex.Matches(code, p).Count);
    }

    private static string ExtractMethodBody(string code, int startIndex)
    {
        // Similar to Java/C# body extraction
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

