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

        var scanResults = new List<IssueResult>();

        if (report?.Issues != null)
        {
            foreach (var file in report.Issues)
            {
                var absolutePath = Path.Combine(projectSourcePath, file.File);
                if (!File.Exists(absolutePath))
                {
                    continue;
                }

                var sourceLines = await File.ReadAllLinesAsync(absolutePath);
                
                // foreach (var comment in file.Comments)
                // {
                //     var lineText = comment.Line > 0 && comment.Line <= sourceLines.Length 
                //         ? sourceLines[comment.Line - 1].Trim() 
                //         : "// Unable to read source line";

                //     scanResults.Add(new IssueResult
                //     {
                //         File = absolutePath,
                //         Line = comment.Line,
                //         Message = lineText,
                //     });
                // }
            }
        }

        return scanResults.OrderBy(c => c.File)
                    .ThenBy(c => c.Line)
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