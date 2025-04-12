using System.IO;
using System.Text.Json;
using CodeHealth.UI.Services;
using CodeHealth.UI.Services.DataLoaders;
using Microsoft.AspNetCore.Components;

namespace CodeHealth.UI.Components.Pages.Project;

public class ProjectSummaryPage : ComponentBase
{
    [Parameter]
    public string ProjectId { get; set; }

    protected bool isAllDataLoaded = false;
    protected string lastScannedTime;
    protected string scanResultsMessage;
    protected Dictionary<string, double> languageBreakdown;

    // Scanner-specific totals/summaries
    protected int totalComplexity;
    protected double averageComplexity;
    protected int totalTodos;

    protected override async Task OnInitializedAsync()
    {
        var projectMetadata = await SharedProjectService.GetProjectInfo(ProjectId);
        var folderName = projectMetadata?.FolderName;
        var lastRunTime = projectMetadata?.Timestamp;

        // Load run data based on the folder name
        if (folderName != null)
        {
            var complexityData = await CyclomaticComplexityDataLoder.LoadCyclomaticComplexityData(folderName);            
            if (complexityData.Any())
            {
                totalComplexity = complexityData.Sum(x => x.Complexity);
                averageComplexity = complexityData.Average(x => x.Complexity);
            }

            var todoData = await TodoCommentDataLoader.LoadTodoCommentsAsync(folderName);
            if (todoData.Any())
            {
                totalTodos = todoData.Count;
            }

            // Load language distribution from JSON file
            var languageDataPath = Path.Combine(folderName, "language_distribution.json");
            if (File.Exists(languageDataPath))
            {
                var languageData = await File.ReadAllTextAsync(languageDataPath);
                languageBreakdown = JsonSerializer.Deserialize<Dictionary<string, double>>(languageData);
            }
            else
            {
                languageBreakdown = new Dictionary<string, double>();
            }
        }

        lastScannedTime = lastRunTime?.ToString("yyyy-MM-dd HH:mm:ss") ?? "Never Scanned";

        isAllDataLoaded = true;
    }

    protected async void ScanProject()
    {
        scanResultsMessage = "";
        var folderPath = await SharedProjectService.GetProjectSourcePath(ProjectId);
        
        try
        {
            var results = ProjectScanner.Scan(folderPath);
            scanResultsMessage = $"Scan of {folderPath} done in {results}";
        }
        catch (Exception ex)
        {
            scanResultsMessage = $"Scan failed: {ex.Message}!";
        }

        // Refresh UI
        await OnInitializedAsync();
        StateHasChanged();
    }
}
