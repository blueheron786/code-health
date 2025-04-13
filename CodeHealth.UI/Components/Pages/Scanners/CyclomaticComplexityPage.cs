using CodeHealth.Core.Dtos.CyclomaticComplexity;
using CodeHealth.UI.Services;
using CodeHealth.UI.Services.DataLoaders;
using Microsoft.AspNetCore.Components;

namespace CodeHealth.UI.Components.Pages.Scanners;

public class CyclomaticComplexityPage : ComponentBase
{
    [Parameter]
    public string ProjectId { get; set; }
    
    [Inject]
    protected NavigationManager NavigationManager { get; set; }
    
    protected List<CyclomaticComplexityData> complexityData;

    protected override async Task OnInitializedAsync()
    {
        var runDirectoryPath = await SharedProjectService.GetRunDirectoryPath(ProjectId);
        complexityData = await CyclomaticComplexityDataLoader.LoadCyclomaticComplexityData(runDirectoryPath);
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
        NavigationManager.NavigateTo($"/project/{ProjectId}/file-view?path={encodedFilePath}");
    }
}