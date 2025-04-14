using CodeHealth.Core.Dtos;
using CodeHealth.Core.IO;
using CodeHealth.UI.Services;
using CodeHealth.UI.Services.DataLoaders;
using Microsoft.AspNetCore.Components;

namespace CodeHealth.UI.Components.Pages.Scanners;

public class CyclomaticComplexityPage : ComponentBase
{
    [Parameter]
    public string ProjectId { get; set; }
    
    [Inject]
    protected NavigationManager _navigationManager { get; set; }
    
    protected List<IssueResult> complexityData;

    protected string _projectRootDirectory;

    protected override async Task OnInitializedAsync()
    {
        _projectRootDirectory = await SharedProjectService.GetProjectSourcePath(ProjectId);
        var runDirectoryPath = await SharedProjectService.GetRunDirectoryPath(ProjectId);
        
        complexityData = await ScannerResultsDataLoader.LoadScannerResultsAsync(ProjectId, runDirectoryPath, Constants.FileNames.CyclomatiComplexityFiles);
            complexityData = complexityData
                .Where(c => c.Metric.Value > 1) // ignore trivial cases
                .OrderByDescending(c => c.Metric.Value)
                .ToList();
    }

    protected string GetComplexityClass(int cc)
    {
        if (cc > 20) return "high-complexity";
        if (cc > 10) return "medium-complexity";
        return "low-complexity";
    }
    
    protected string GetComplexityText(int cc)
    {
        if (cc > 20) return "High";
        if (cc > 10) return "Medium";
        return "Low";
    }
    
    protected void NavigateToFileView(string filePath)
    {
        var encodedFilePath = Uri.EscapeDataString(filePath);
        _navigationManager.NavigateTo($"/project/{ProjectId}/file-view?path={encodedFilePath}");
    }
}