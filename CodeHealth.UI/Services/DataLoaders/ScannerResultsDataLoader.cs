using System.IO;
using System.Text.Json;
using CodeHealth.Core.Dtos;

namespace CodeHealth.UI.Services.DataLoaders;

public static class ScannerResultsDataLoader
{
    public static async Task<List<IssueResult>> LoadScannerResultsAsync(string projectId, string runDirectoryPath, string reportFileName)
    {
        var projectSourcePath = await SharedProjectService.GetProjectSourcePath(projectId);

        var matchingFiles = reportFileName.Contains("*")
            ? Directory.GetFiles(runDirectoryPath, reportFileName)
            : new[] { Path.Combine(runDirectoryPath, reportFileName) };

        var allIssues = new List<IssueResult>();

        foreach (var resultsReportFilePath in matchingFiles)
        {
            if (!File.Exists(resultsReportFilePath))
            {
                continue;
            }

            var jsonData = await File.ReadAllTextAsync(resultsReportFilePath);
            var report = JsonSerializer.Deserialize<Report>(jsonData);

            if (report?.Issues == null)
            {
                continue;
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

                if (!(issue.Line > 0 && issue.Line <= sourceLines.Length))
                {
                    issue.Message += " → [Unable to read source line]";
                }
            }

            allIssues.AddRange(report.Issues);
        }

        return allIssues
                .OrderBy(i => i.File)
                .ThenBy(i => i.Line)
                .ToList();
    }
}
