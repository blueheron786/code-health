using System.Windows.Controls.Primitives;
using CodeHealth.Core.Dtos;
using CodeHealth.Core.IO;
using CodeHealth.UI.Services;
using CodeHealth.UI.Services.DataLoaders;
using Microsoft.AspNetCore.Components;

namespace CodeHealth.UI.Components.Pages.Scanners;

public partial class GenericScanner : ComponentBase
{
    [Parameter]
    public string ProjectId { get; set; }

    [Parameter]
    public string ScannerName { get; set; } = string.Empty;

    [Parameter]
    public List<IssueResult> ScannerData { get; set; }

    [Inject]
    protected NavigationManager NavigationManager { get; set; }
    
    [Parameter]
    public string ProjectRootDirectory { get; set; } = string.Empty;

    [Parameter]
    public Func<IssueResult, string> ValueFormatter { get; set; } = r => r.Metric.Value.ToString();

    [Parameter]
    public Func<IssueResult, string> BadgeClass { get; set; } = _ => string.Empty;

    [Parameter]
    public Func<IssueResult, string> BadgeText { get; set; } = _ => string.Empty;

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

    protected void NavigateToFileView(string filePath)
    {
        if (NavigationManager != null)
        {
            var encodedFilePath = Uri.EscapeDataString(filePath);
            NavigationManager.NavigateTo($"/project/{ProjectId}/file-view?path={encodedFilePath}");
        }
    }
}