using CodeHealth.Core.Dtos;
using CodeHealth.Core.IO;
using Microsoft.CodeAnalysis;
using System.Text.Json;
using System.Text.RegularExpressions;

namespace CodeHealth.Scanners.Common;

public class HeuristicLongMethodScanner
{
    private const string CStyleMethodRegex = @"\b(public|private|protected)?\s*(static\s+)?[\w<>\[\]]+\s+\w+\s*\(.*\)\s*{?\s*$";
    private const string JsLikeFunctionRegex = @"\bfunction\b|\s*=>\s*{";
    private readonly int _threshold;

    public HeuristicLongMethodScanner(int threshold = 40) // Modified constructor
    {
        _threshold = threshold;
    }

    public void AnalyzeFiles(Dictionary<string, string> sourceFiles, string rootPath, string resultsDirectory)
    {
        var issues = new List<IssueResult>();

        foreach (var (fullFileName, content) in sourceFiles)
        {
            // Create relative filename
            var relativeFileName = Path.GetRelativePath(rootPath, fullFileName).Replace("\\", "/");
            var fileIssues = AnalyzeText(relativeFileName, content);
            issues.AddRange(fileIssues);
        }

        var report = new Report
        {
            Issues = issues,
            TotalMetricValue = issues.Sum(i => i.Metric.Value),
            AverageMetricValue = issues.Count == 0 ? 0 : issues.Average(i => i.Metric.Value)
        };

        var outPath = Path.Combine(resultsDirectory, Constants.FileNames.LongMethodsFile);
        File.WriteAllText(outPath, JsonSerializer.Serialize(report, new JsonSerializerOptions { WriteIndented = true }));
    }

    private List<IssueResult> AnalyzeText(string fileName, string text)
    {
        var issues = new List<IssueResult>();
        var lines = text.Split('\n');
        int methodStart = -1;
        int methodLineStart = -1;
        int braceDepth = 0;
        string methodName = "Unknown";

        for (int i = 0; i < lines.Length; i++)
        {
            var line = lines[i].Trim();

            // Check for method declaration
            var methodMatch = Regex.Match(line, CStyleMethodRegex) ?? Regex.Match(line, JsLikeFunctionRegex);
            if (methodMatch.Success)
            {
                methodStart = i;
                methodLineStart = i;
                braceDepth = line.Contains("{") ? 1 : 0;

                // Extract method name
                methodName = ExtractMethodName(line, methodLineStart, methodMatch);
                continue;
            }

            if (methodStart >= 0)
            {
                braceDepth += line.Count(c => c == '{');
                braceDepth -= line.Count(c => c == '}');

                if (braceDepth <= 0)
                {
                    var methodEnd = i;
                    var length = methodEnd - methodStart + 1;
                    if (length > _threshold)
                    {
                        issues.Add(new IssueResult
                        {
                            Scanner = "LongMethods",
                            Type = "Method",
                            File = fileName,
                            Line = methodLineStart + 1,
                            EndLine = methodEnd + 1,
                            Name = methodName, // Use extracted name
                            Metric = new Metric
                            {
                                Name = "LineCount",
                                Value = length,
                                Threshold = _threshold
                            },
                            Message = $"Method '{methodName}' is {length} lines long.",
                            Severity = "Medium",
                            Suggestion = "Consider breaking this method into smaller units.",
                            Tags = new List<string> { "long-method", "heuristic" },
                            Fixable = false
                        });
                    }

                    methodStart = -1;
                    methodName = "Unknown";
                }
            }
        }

        return issues;
    }

    private static string ExtractMethodName(string line, int lineNumber, Match methodMatch)
    {
        try
        {
            // For C-style methods
            if (methodMatch.Value.Contains("("))
            {
                var beforeParen = line.Substring(0, line.IndexOf('(')).Trim();
                var lastPart = beforeParen.Split(new[] { ' ', '\t' }, StringSplitOptions.RemoveEmptyEntries).Last();
                return lastPart;
            }
            // For arrow functions
            else if (methodMatch.Value.Contains("=>"))
            {
                var beforeArrow = line.Substring(0, line.IndexOf("=>")).Trim();
                return beforeArrow.Contains("(")
                    ? $"anonymous (line {lineNumber})"
                    : beforeArrow; // For simple arrow functions like "x =>"
            }
            // For function declarations
            else if (methodMatch.Value.Contains("function"))
            {
                var afterFunction = line.Substring(line.IndexOf("function") + 8).Trim();
                return afterFunction.Split(new[] { '(', ' ' }, StringSplitOptions.RemoveEmptyEntries).First();
            }
        }
        catch
        {
            // Fall through to return "Unknown" below
        }

        return "Unknown";
    }
}