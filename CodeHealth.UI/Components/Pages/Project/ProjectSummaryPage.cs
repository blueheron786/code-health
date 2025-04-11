using System.IO;
using System.Text.Json;
using CodeHealth.Core.IO;
using CodeHealth.Scanners.CSharp.Formatters;
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
        var projectMetadata = await SharedProjectService.LoadProjectInfo(ProjectId);
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

        lastScannedTime = lastRunTime?.ToString("yyyy-MM-dd HH:mm:ss");

        isAllDataLoaded = true;
    }
}
