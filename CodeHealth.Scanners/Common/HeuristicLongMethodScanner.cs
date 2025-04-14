using CodeHealth.Core.Dtos;
using Microsoft.CodeAnalysis;
using System.Text.Json;
using System.Text.RegularExpressions;

namespace CodeHealth.Scanners.Common;

public class HeuristicLongMethodScanner
{
    private const string CStyleMethodRegex = @"\b(public|private|protected)?\s*(static\s+)?[\w<>\[\]]+\s+\w+\s*\(.*\)\s*{?\s*$";
    private const string JsLikeFunctionRegex = @"\bfunction\b|\s*=>\s*{";
    private readonly int _threshold;

    public HeuristicLongMethodScanner(int threshold = 40)
    {
        _threshold = threshold;
    }

    public void AnalyzeFiles(Dictionary<string, string> sourceFiles, string resultsDirectory)
    {
        var issues = new List<IssueResult>();

        foreach (var (fileName, content) in sourceFiles)
        {
            var fileIssues = AnalyzeText(fileName, content);
            issues.AddRange(fileIssues);
        }

        var report = new Report
        {
            Issues = issues,
            TotalMetricValue = issues.Sum(i => i.Metric.Value),
            AverageMetricValue = issues.Count == 0 ? 0 : issues.Average(i => i.Metric.Value)
        };

        var outPath = Path.Combine(resultsDirectory, "long_methods.json");
        File.WriteAllText(outPath, JsonSerializer.Serialize(report, new JsonSerializerOptions { WriteIndented = true }));
    }

    private List<IssueResult> AnalyzeText(string fileName, string text)
    {
        var issues = new List<IssueResult>();
        var lines = text.Split('\n');
        int methodStart = -1;
        int methodLineStart = -1;
        int braceDepth = 0;

        for (int i = 0; i < lines.Length; i++)
        {
            var line = lines[i].Trim();

            if (Regex.IsMatch(line, CStyleMethodRegex) || Regex.IsMatch(line, JsLikeFunctionRegex))
            {
                methodStart = i;
                methodLineStart = i;
                braceDepth = line.Contains("{") ? 1 : 0;
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
                            Name = "Unknown",
                            Metric = new Metric
                            {
                                Name = "LineCount",
                                Value = length,
                                Threshold = _threshold
                            },
                            Message = $"Possible method block is {length} lines long.",
                            Severity = "Medium",
                            Suggestion = "Consider breaking this block into smaller units.",
                            Tags = new List<string> { "long-method", "heuristic" },
                            Fixable = false
                        });
                    }

                    methodStart = -1;
                }
            }
        }

        return issues;
    }
}
