using System.IO;
using System.Text.Json;
using CodeHealth.Core.Dtos;

namespace CodeHealth.UI.Services.DataLoaders;

public static class ScannerResultsDataLoader
{
    public static async Task<List<IssueResult>> LoadScannerResultsAsync(string projectId, string runDirectoryPath, string reportFileName)
    {
        var projectSourcePath = await SharedProjectService.GetProjectSourcePath(projectId);
        var resultsReportFilePath = Path.Combine(runDirectoryPath, reportFileName);

        if (!File.Exists(resultsReportFilePath))
        {
            return new List<IssueResult>();
        }

        var jsonData = await File.ReadAllTextAsync(resultsReportFilePath);
        var report = JsonSerializer.Deserialize<Report>(jsonData);

        if (report?.Issues == null)
        {
            return new List<IssueResult>();
        }

        foreach (var issue in report.Issues)
        {
            var absolutePath = Path.Combine(projectSourcePath, issue.File);

            if (!File.Exists(absolutePath))
            {
                continue;
            }

            var sourceLines = await File.ReadAllLinesAsync(absolutePath);

            issue.File = absolutePath; // Convert to full path

            if (issue.Line > 0 && issue.Line <= sourceLines.Length)
            {
                issue.Message += $" → \"{sourceLines[issue.Line - 1].Trim()}\"";
            }
            else
            {
                issue.Message += " → [Unable to read source line]";
            }
        }

        return report.Issues
                     .OrderBy(i => i.File)
                     .ThenBy(i => i.Line)
                     .ToList();
    }

    private static List<string> GetContextLines(string[] sourceLines, int lineNumber, int contextSize = 3)
    {
        var context = new List<string>();
        int start = Math.Max(1, lineNumber - contextSize);
        int end = Math.Min(sourceLines.Length, lineNumber + contextSize);

        for (int i = start; i <= end; i++)
        {
            context.Add(sourceLines[i - 1]);
        }

        return context;
    }
}
