using CodeHealth.Core.Dtos;
using CodeHealth.Core.IO;
using CodeHealth.UI.Services;
using CodeHealth.UI.Services.DataLoaders;
using Microsoft.AspNetCore.Components;

namespace CodeHealth.UI.Components.Pages.Scanners;

public partial class CyclomaticComplexityPage : ComponentBase
{
    [Parameter]
    public string ProjectId { get; set; }
    
    [Inject]
    protected NavigationManager NavigationManager { get; set; }
    
    protected List<IssueResult> ScannerData { get; set; }
    protected string ProjectRootDirectory { get; set; }
    
    protected Func<IssueResult, string> ValueFormatter => r => r.Metric.Value.ToString();
    protected Func<IssueResult, string> BadgeClassDelegate => GetComplexityClass;
    protected Func<IssueResult, string> BadgeTextDelegate => GetComplexityText;

    protected override async Task OnInitializedAsync()
    {
        ProjectRootDirectory = await SharedProjectService.GetProjectSourcePath(ProjectId);
        var runDirectoryPath = await SharedProjectService.GetRunDirectoryPath(ProjectId);
        
        ScannerData = await ScannerResultsDataLoader.LoadScannerResultsAsync(
            ProjectId, 
            runDirectoryPath, 
            Constants.FileNames.CyclomatiComplexityFiles);
            
        if (ScannerData != null)
        {
            ScannerData = ScannerData
                .Where(c => c.Metric.Value > 1) // ignore trivial cases
                .OrderByDescending(c => c.Metric.Value)
                .ToList();
        }
    }

    protected string GetComplexityClass(IssueResult result) => 
        result.Metric.Value > 20 ? "high-complexity" : 
        result.Metric.Value > 10 ? "medium-complexity" :
        "low-complexity";

    protected string GetComplexityText(IssueResult result) =>
        result.Metric.Value > 20 ? "High" :
        result.Metric.Value > 10 ? "Medium" :
        "Low";
}