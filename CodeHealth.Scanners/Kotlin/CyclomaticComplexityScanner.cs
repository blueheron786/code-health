using System;

namespace CodeHealth.Scanners.Kotlin;

using CodeHealth.Core.Dtos.CyclomaticComplexity;
using CodeHealth.Core.IO;
using System.Text.RegularExpressions;
using System.Text.Json;
using CodeHealth.Scanners.Common;

public class CyclomaticComplexityScanner : IStaticCodeScanner
{
    public void AnalyzeFiles(Dictionary<string, string> sourceFiles, string rootPath, string outputDir)
    {
        var report = new Report();

        foreach (var kvp in sourceFiles)
        {
            string fileName = kvp.Key;
            string code = kvp.Value;

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

        int methodCount = report.Files.Sum(f => f.Methods.Count);
        report.AverageComplexity = methodCount > 0 ? (double)report.TotalComplexity / methodCount : 0;

        var outputFile = Path.Combine(outputDir, Constants.FileNames.CyclomatiComplexityFile);
        var jsonOptions = new JsonSerializerOptions { WriteIndented = true };
        var json = JsonSerializer.Serialize(report, jsonOptions);
        File.WriteAllText(outputFile, json);
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

