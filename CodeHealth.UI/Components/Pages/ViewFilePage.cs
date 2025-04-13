using CodeHealth.Core.Dtos.CyclomaticComplexity;
using CodeHealth.UI.Services;
using CodeHealth.UI.Services.DataLoaders;
using Microsoft.AspNetCore.Components;
using System.IO;
using System.Web;

namespace CodeHealth.UI.Components.Pages;

public partial class ViewFilePage : ComponentBase
{
    [Parameter]
    public string ProjectId { get; set; }
    
    [Parameter]
    [SupplyParameterFromQuery]
    public string Path { get; set; }
    
    [Inject]
    protected NavigationManager NavigationManager { get; set; }
    
    protected string FileContent { get; set; }
    protected List<CyclomaticComplexityData> FileComplexities { get; set; }
    protected string FileName { get; set; }

    protected override async Task OnInitializedAsync()
    {
        if (string.IsNullOrEmpty(Path))
        {
            NavigationManager.NavigateTo($"/project/{ProjectId}/cyclomatic-complexity");
            return;
        }

        var decodedPath = HttpUtility.UrlDecode(Path);
        FileName = System.IO.Path.GetFileName(decodedPath);
        
        var runDirectoryPath = await SharedProjectService.GetRunDirectoryPath(ProjectId);
        var allComplexities = await CyclomaticComplexityDataLoader.LoadCyclomaticComplexityData(runDirectoryPath);
        
        FileComplexities = allComplexities.Where(c => c.File.Equals(decodedPath, StringComparison.OrdinalIgnoreCase)).ToList();
        FileContent = await LoadFileContent(decodedPath);
    }

    private async Task<string> LoadFileContent(string filePath)
    {
        var projectSourcePath = await SharedProjectService.GetProjectSourcePath(ProjectId);
        var absolutePath = System.IO.Path.Combine(projectSourcePath, filePath);
        if (File.Exists(absolutePath))
        {
            var text = await File.ReadAllTextAsync(absolutePath);
            return text;
        }
        else
        {
            // not necessarily a coding mistake; maybe they deleted the file since the analysis.
            return $"// Source file not found: {absolutePath}";
        }
    }

    protected string GetComplexityClass(int cc)
    {
        if (cc > 20) return "high-complexity";
        if (cc > 10) return "medium-complexity";
        return "low-complexity";
    }

    protected void NavigateBack()
    {
        NavigationManager.NavigateTo($"/project/{ProjectId}/cyclomatic-complexity");
    }
}