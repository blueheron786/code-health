using System.Text.Json;
using System.Text.RegularExpressions;
using CodeHealth.Core.Dtos;
using CodeHealth.Core.IO;

namespace CodeHealth.Scanners.Common;

public class TodoCommentScanner : IStaticCodeScanner
{
    private static readonly Regex TodoRegex = new(@"//\s*TODO\b.*", RegexOptions.Compiled | RegexOptions.IgnoreCase);

    public void AnalyzeFiles(Dictionary<string, string> sourceFiles, string rootPath, string outputDir)
    {
        var report = new Report();

        foreach (var kvp in sourceFiles)
        {
            var filePath = kvp.Key;
            var code = kvp.Value;
            var relativePath = Path.GetRelativePath(rootPath, filePath).Replace("\\", "/");

            var lines = code.Split('\n');

            for (int i = 0; i < lines.Length; i++)
            {
                var line = lines[i];
                var match = TodoRegex.Match(line);

                if (match.Success)
                {
                    report.Issues.Add(new IssueResult
                    {
                        Scanner = "TodoComments",
                        Type = "Comment",
                        File = relativePath,
                        Line = i + 1,
                        EndLine = i + 1,
                        Column = match.Index + 1,
                        EndColumn = match.Index + match.Length + 1,
                        Message = line,
                        Severity = "Low",
                        Suggestion = "Review TODO and consider resolving or removing.",
                        Tags = new List<string> { "todo", "comment", "technical-debt" },
                        Fixable = false,
                        Metric = new Metric
                        {
                            Name = "TodoComment",
                            Value = 1
                        }
                    });
                }
            }
        }

        var outputFile = Path.Combine(outputDir, Constants.FileNames.TodoCommentsFile);
        var options = new JsonSerializerOptions { WriteIndented = true };
        File.WriteAllText(outputFile, JsonSerializer.Serialize(report, options));
    }
}
