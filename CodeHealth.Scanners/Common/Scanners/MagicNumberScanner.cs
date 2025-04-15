using System.Text.Json;
using System.Text.RegularExpressions;
using CodeHealth.Core.Dtos;
using CodeHealth.Core.IO;

namespace CodeHealth.Scanners.Common.Scanners;

public class MagicNumberScanner : IStaticCodeScanner
{
    private static readonly Regex NumberRegex = new(@"\b\d+(\.\d+)?\b", RegexOptions.Compiled);
    private static readonly Regex ConstAssignmentRegex = new(@"const\s+\w+\s+\w+\s*=\s*\d+", RegexOptions.Compiled);
    private static readonly Regex VariableAssignmentRegex = new(@"\b(?:int|float|double|var)\s+\w+\s*=\s*\d+", RegexOptions.Compiled);

    private static readonly Regex CStyleMethodRegex = new(@"^\s*(public|private|protected|internal)?\s*(static\s+)?[\w<>\[\]]+\s+(\w+)\s*\([^)]*\)\s*{?\s*$", RegexOptions.Compiled);
    private static readonly Regex JsLikeFunctionRegex = new(@"\bfunction\b|\s*=>\s*{", RegexOptions.Compiled);

    public void AnalyzeFiles(Dictionary<string, string> sourceFiles, string rootPath, string outputDir)
    {
        var report = new Report();

        foreach (var kvp in sourceFiles)
        {
            var filePath = kvp.Key;
            var code = kvp.Value;
            var relativePath = Path.GetRelativePath(rootPath, filePath).Replace("\\", "/");

            var lines = code.Split('\n');

            int braceDepth = 0;
            string currentMethodName = "Global Scope";

            for (int i = 0; i < lines.Length; i++)
            {
                var line = lines[i];
                var trimmed = line.Trim();
                if (trimmed.StartsWith("//") || trimmed.StartsWith("/*") || trimmed.StartsWith("*") || trimmed.StartsWith("#"))
                {
                    continue; // Skip comments
                }
                
                // Method detection
                var methodMatch = CStyleMethodRegex.Match(trimmed);
                if (!methodMatch.Success)
                    methodMatch = JsLikeFunctionRegex.Match(trimmed);

                if (methodMatch.Success)
                {
                    currentMethodName = MethodNameExtractor.ExtractMethodName(trimmed, i, methodMatch);
                    braceDepth = line.Count(c => c == '{') - line.Count(c => c == '}');
                }
                else
                {
                    braceDepth += line.Count(c => c == '{') - line.Count(c => c == '}');
                    if (braceDepth <= 0)
                    {
                        currentMethodName = "Global Scope";
                    }
                }

                // Scan for magic numbers
                var matches = NumberRegex.Matches(line);
                foreach (Match match in matches)
                {
                    var value = match.Value;
                    if (value == "0" || value == "1") continue;
                    if (ConstAssignmentRegex.IsMatch(line) || VariableAssignmentRegex.IsMatch(line)) continue;

                    report.Issues.Add(new IssueResult
                    {
                        Scanner = "MagicNumber",
                        Type = "MagicNumber",
                        File = relativePath,
                        Line = i + 1,
                        EndLine = i + 1,
                        Column = match.Index + 1,
                        EndColumn = match.Index + match.Length + 1,
                        Name = currentMethodName,
                        Message = $"Magic number: {value}",
                        Severity = "Medium",
                        Suggestion = "Consider extracting this number into a named constant.",
                        Tags = new List<string> { "magic-number", "readability" },
                        Fixable = true,
                        Metric = new Metric
                        {
                            Name = "MagicNumber",
                            Value = int.TryParse(value, out var intVal) ? intVal : 0
                        }
                    });
                }
            }
        }

        var outputFile = Path.Combine(outputDir, Constants.FileNames.MagicNumbersFile);
        var options = new JsonSerializerOptions { WriteIndented = true };
        File.WriteAllText(outputFile, JsonSerializer.Serialize(report, options));
    }
}
