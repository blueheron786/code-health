using CodeHealth.Core.Dtos.CyclomaticComplexity;
using System.Text.RegularExpressions;
using CodeHealth.Scanners.Common;

namespace CodeHealth.Scanners;

// NOTE: This is a lightweight, token-based Java cyclomatic complexity analyzer.
// ✔ Works well with traditional Java (6–11) codebases using explicit method bodies and common control flow.
// ⚠ Not guaranteed to handle modern Java (14+) features like lambdas, pattern matching, records, etc.
// ❌ Does NOT use a real Java parser — it's optimized for speed, not accuracy.
public class JavaCyclomaticComplexityScanner : IStaticCodeScanner
{
    public readonly string FileExtension = ".java";

    private const string MethodPattern = @"(?:public|private|protected)?\s*(?:static\s+)?[\w<>\[\]]+\s+(\w+)\s*\([^)]*\)\s*(\{?)";
    private static readonly string[] DecisionPatterns = new[]
    {
        @"if\s*\(",
        @"for\s*\(",
        @"while\s*\(",
        @"case\s+[^:]+:",
        @"catch\s*\(",
        @"&&",
        @"\|\|",
        @"\?\s*"
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

                fileResult.Methods.Add(new MethodResult
                {
                    Method = methodName,
                    Complexity = complexity,
                    StartLine = startLine,
                    EndLine = endLine
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
