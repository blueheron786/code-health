using System.IO;
using System.Text.Json;
using CodeHealth.Scanners.CSharp.Formatters;
using CodeHealth.UI.Services;
using Microsoft.AspNetCore.Components;

namespace CodeHealth.UI.Components.Pages.Project;

public class CyclomaticComplexityPage : ComponentBase
{
    [Parameter]
    public string ProjectId { get; set; }
    protected List<CyclomaticComplexityData> complexityData;

    protected override async Task OnInitializedAsync()
    {
        var runDirectoryPath = await SharedProjectService.GetRunDirectoryPath(ProjectId);
        complexityData = await LoadCyclomaticComplexityData(runDirectoryPath);
    }

    private async Task<List<CyclomaticComplexityData>> LoadCyclomaticComplexityData(string runDirectoryPath)
    {
        var filePath = Path.Combine(runDirectoryPath, "cyclomatic_complexity.json");

        if (!File.Exists(filePath))
        {
            return new List<CyclomaticComplexityData>();
        }

        var jsonData = await File.ReadAllTextAsync(filePath);
        var report = JsonSerializer.Deserialize<CyclomaticComplexityJsonFormatter.Report>(jsonData);

        var methods = report.Files
            .SelectMany(file => file.Methods.Select(method => new CyclomaticComplexityData
            {
                File = file.File,
                Method = method.Method,
                Complexity = method.Complexity
            }))
            .OrderByDescending(m => m.Complexity)
            .ToList();

        return methods;
    }

    protected string GetComplexityClass(int cc)
    {
        if (cc > 20) return "table-danger";
        if (cc > 10) return "table-warning";
        return "";
    }
}

public class ProjectInfo
{
    public DateTime Timestamp { get; set; }
    public string FolderName { get; set; }
}

public class CyclomaticComplexityData
{
    public string File { get; set; }
    public string Method { get; set; }
    public int Complexity { get; set; }
}
