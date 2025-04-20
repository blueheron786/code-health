using CodeHealth.Core.Dtos;
using CodeHealth.Core.IO;
using System.Text.Json;

namespace CodeHealth.Scanners.Common.Scanners;

public class HeuristicLongMethodScanner
{
    private readonly int _threshold;

    public HeuristicLongMethodScanner(int threshold = 40)
    {
        if (threshold < 0)
        {
            throw new ArgumentOutOfRangeException(nameof(threshold), "Threshold cannot be negative");
        }
           
        _threshold = threshold;
    }

    public void AnalyzeFiles(Dictionary<string, string> sourceFiles, string rootPath, string resultsDirectory)
    {
        // Ensure directory exists
        Directory.CreateDirectory(resultsDirectory);

        var issues = new List<IssueResult>();

        foreach (var (fullFileName, content) in sourceFiles)
        {
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
        MethodInfo? currentMethod = null;
        int braceDepth = 0;
        int methodStartLine = 0;

        for (int i = 0; i < lines.Length; i++)
        {
            var line = lines[i].Trim();

            // Check for method declaration
            var methodInfo = MethodNameExtractor.DetectMethodAtLine(line, i);
            if (methodInfo != null)
            {
                currentMethod = methodInfo;
                methodStartLine = i;
                braceDepth = line.Contains("{") ? 1 : 0;
                continue;
            }

            if (currentMethod != null)
            {
                braceDepth += line.Count(c => c == '{');
                braceDepth -= line.Count(c => c == '}');

                if (braceDepth <= 0)
                {
                    var methodEnd = i;
                    var length = methodEnd - methodStartLine + 1;
                    if (length > _threshold)
                    {
                        issues.Add(new IssueResult
                        {
                            Scanner = "LongMethods",
                            Type = "Method",
                            File = fileName,
                            Line = currentMethod.StartLine + 1,
                            EndLine = methodEnd + 1,
                            Name = currentMethod.Name,
                            Metric = new Metric
                            {
                                Name = "LineCount",
                                Value = length,
                                Threshold = _threshold
                            },
                            Message = $"Method '{currentMethod.Name}' is {length} lines long.",
                            Severity = "Medium",
                            Suggestion = "Consider breaking this method into smaller units.",
                            Tags = new List<string> { "long-method", "heuristic" },
                            Fixable = false
                        });
                    }
                    currentMethod = null;
                }
            }
        }

        return issues;
    }
}