using System.IO;
using System.Text.Json;
using CodeHealth.Core.Dtos.CyclomaticComplexity;

namespace CodeHealth.UI.Services.DataLoaders;

public static class IssueResultLoader
{
    public static async Task<List<IssueResult>> LoadIssues(string runDirectory)
    {
        var allIssues = new List<IssueResult>();
        var files = Directory.GetFiles(runDirectory, "*.json", SearchOption.AllDirectories);

        foreach (var file in files)
        {
            var json = await File.ReadAllTextAsync(file);
            var report = JsonSerializer.Deserialize<Report>(json);
            if (report?.Issues != null)
            {
                allIssues.AddRange(report.Issues);
            }
        }

        return allIssues;
    }
}
