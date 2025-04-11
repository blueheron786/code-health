using System.IO;
using System.Text.Json;
using CodeHealth.Scanners.CSharp.Formatters;
using Microsoft.AspNetCore.Components;

namespace CodeHealth.UI.Components.Pages.Project;

public class ProjectSummaryPage : ComponentBase
{
 [Parameter] public string ProjectId { get; set; }
    protected List<CyclomaticComplexityData> complexityData;
    protected int totalComplexity;
    protected double averageComplexity;

    protected override async Task OnInitializedAsync()
    {
        complexityData = await LoadCyclomaticComplexityData(ProjectId);

        if (complexityData.Any())
        {
            totalComplexity = complexityData.Sum(x => x.Complexity);
            averageComplexity = complexityData.Average(x => x.Complexity);
        }
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

        // Map MethodResult to CyclomaticComplexityData
        var methods = report.Files
            .SelectMany(file => file.Methods)
            .Select(method => new CyclomaticComplexityData
            {
                Method = method.Method,
                Complexity = method.Complexity
            })
            .OrderByDescending(m => m.Complexity)
            .ToList();

        return methods;
    }

    protected string GetComplexityClass(int complexity)
    {
        if (complexity > 20)
        {
            return "table-danger"; // Red
        }
        else if (complexity > 10)
        {
            return "table-warning"; // Yellow
        }
        else
        {
            return string.Empty; // No class for normal complexity
        }
    }


    public class CyclomaticComplexityData
    {
        public string File { get; set; }
        public string Method { get; set; }
        public int Complexity { get; set; }
    }
}
