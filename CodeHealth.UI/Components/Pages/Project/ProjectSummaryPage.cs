using CodeHealth.UI.Services;
using Microsoft.AspNetCore.Components;

namespace CodeHealth.UI.Components.Pages.Project;

public class ProjectSummaryPage : ComponentBase
{
    [Parameter]
    public string ProjectId { get; set; }

    protected bool isAllDataLoaded = false;
    protected int totalComplexity;
    protected double averageComplexity;
    protected string lastScannedTime;

    protected override async Task OnInitializedAsync()
    {
        var projectMetadata = await SharedProjectService.GetProjectInfo(ProjectId);
        var folderName = projectMetadata?.FolderName;
        var lastRunTime = projectMetadata?.Timestamp;

        // Load cyclomatic complexity data based on the folder name
        if (folderName != null)
        {
            var complexityData = await CyclomaticComplexityDataLoder.LoadCyclomaticComplexityData(folderName);
            
            if (complexityData.Any())
            {
                totalComplexity = complexityData.Sum(x => x.Complexity);
                averageComplexity = complexityData.Average(x => x.Complexity);
            }
        }

        lastScannedTime = lastRunTime?.ToString("yyyy-MM-dd HH:mm:ss") ?? "Never Scanned";

        isAllDataLoaded = true;
    }

    protected async void ScanProject()
    {
        var folderPath = await SharedProjectService.GetProjectSourcePath(ProjectId);
        var results = ProjectScanner.Scan(folderPath);
        // Update UI
        await OnInitializedAsync();
        StateHasChanged();
    }
}
