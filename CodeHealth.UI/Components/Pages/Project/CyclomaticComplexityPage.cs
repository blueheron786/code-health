using System.IO;
using System.Text.Json;
using CodeHealth.Scanners.CSharp.Formatters;
using Microsoft.AspNetCore.Components;
using static CodeHealth.UI.Components.Pages.Project.ProjectSummaryPage;

namespace CodeHealth.UI.Components.Pages.Project;

public class CyclomaticComplexityPage : ComponentBase
{
    [Parameter]
    public string ProjectId { get; set; }
    protected List<CyclomaticComplexityData> complexityData;

    protected override async Task OnInitializedAsync()
    {
        complexityData = await LoadCyclomaticComplexityData(ProjectId);
    }

    private async Task<List<CyclomaticComplexityData>> LoadCyclomaticComplexityData(string projectId)
    {
        var filePath = Path.Combine("projects", projectId, "cyclomatic_complexity.json");

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
        if (cc > 20)
        {
            return "table-danger"; // Red for CC > 20
        }
        else if (cc > 10)
        {
            return "table-warning"; // Yellow for CC > 10
        }
        else
        {
            return ""; // Default for normal CC
        }
    }
}
