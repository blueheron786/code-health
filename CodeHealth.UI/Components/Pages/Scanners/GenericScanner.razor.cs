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

    protected void NavigateToFileView(string filePath)
    {
        if (NavigationManager != null)
        {
            var encodedFilePath = Uri.EscapeDataString(filePath);
            NavigationManager.NavigateTo($"/project/{ProjectId}/file-view?path={encodedFilePath}");
        }
    }
}