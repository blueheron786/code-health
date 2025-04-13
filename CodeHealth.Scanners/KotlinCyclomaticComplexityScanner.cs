namespace CodeHealth.Scanners;

using CodeHealth.Core.Dtos.CyclomaticComplexity;
using System.Text.RegularExpressions;
using CodeHealth.Scanners.Common;

public class KotlinCyclomaticComplexityScanner : IStaticCodeScanner
{
    public const string FileExtension = ".kt";
    
    // Method detection patterns
    private const string MethodPattern = @"(?:fun)\s+(\w+)\s*\([^)]*\)\s*(\{?)";
    
    // String/comment stripping patterns
    private const string StringLiteralPattern = "\"(?:\\\\.|[^\"\\\\])*\"";
    private const string SingleLineCommentPattern = @"//.*";
    private const string MultiLineCommentPattern = @"/\*.*?\*/";
    
    // Complexity decision point patterns
    private static readonly string[] DecisionPointPatterns = new[]
    {
        @"if\s*\(",    // if statements
        @"for\s*\(",   // for loops
        @"while\s*\(", // while loops
        @"when\s*\(",  // Kotlin when expressions
        @"&&",         // logical AND
        @"\|\|",       // logical OR
        @"\?\s*"       // ternary operator
    };

    public void AnalyzeFiles(Dictionary<string, string> sourceFiles, string rootPath, string outputDir)
    {
        var report = new Report();

        foreach (var kvp in sourceFiles)
        {
            string fileName = kvp.Key;
            string code = kvp.Value;

            if (!fileName.EndsWith(FileExtension, StringComparison.OrdinalIgnoreCase))
                continue;
            // Preprocess to strip comments and strings (will work similarly as with Java)
            code = StripCommentsAndStrings(code);
            string[] lines = code.Split('\n');

            var fileResult = new FileResult
            {
                File = Path.GetRelativePath(rootPath, fileName).Replace("\\", "/")
            };

            // Find all method-like declarations using regex
            var methodMatches = Regex.Matches(code, MethodPattern);
            foreach (Match match in methodMatches)
            {
                string methodName = match.Groups[1].Value;
                int methodStartPos = match.Index;
                
                int startLine = GetLineNumber(code, methodStartPos, lines);
                string methodBlock = ExtractMethodBody(code, methodStartPos);
                int endLine = GetLineNumber(code, methodStartPos + methodBlock.Length, lines);

                fileResult.Methods.Add(new MethodResult
                {
                    Method = methodName,
                    Complexity = CountDecisionPoints(methodBlock) + 1,
                    StartLine = startLine,
                    EndLine = endLine
                });
            }

            if (fileResult.Methods.Any())
                report.Files.Add(fileResult);
        }

        CyclomaticComplexityReporter.FinalizeReport(report, outputDir, "cyclomatic_complexity.kotlin.json");
    }

    private static string StripCommentsAndStrings(string code)
    {
        code = Regex.Replace(code, StringLiteralPattern, "\"\"");
        code = Regex.Replace(code, SingleLineCommentPattern, "");
        return Regex.Replace(code, MultiLineCommentPattern, "", RegexOptions.Singleline);
    }

    private static int CountDecisionPoints(string code)
    {
        return DecisionPointPatterns.Sum(pattern => 
            Regex.Matches(code, pattern).Count);
    }

    private static int GetLineNumber(string code, int position, string[] lines)
    {
        int currentPos = 0;
        for (int i = 0; i < lines.Length; i++)
        {
            currentPos += lines[i].Length + 1;
            if (currentPos > position)
                return i + 1;
        }
        return 1;
    }

    private static string ExtractMethodBody(string code, int startIndex)
    {
        int braceCount = 0;
        int i = code.IndexOf('{', startIndex);
        if (i == -1) {
            return "";
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
                break;
            }
        }
        return i < code.Length ? code.Substring(start, i - start + 1) : "";
    }
}