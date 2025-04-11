using CodeHealth.Core.Dtos;
using CodeHealth.UI.Services;
using Microsoft.AspNetCore.Components;

namespace CodeHealth.UI.Components.Pages.Project;

public class CyclomaticComplexityPage : ComponentBase
{
    [Parameter]
    public string ProjectId { get; set; }
    protected List<CyclomaticComplexityData> complexityData;

    protected override async Task OnInitializedAsync()
    {
        var runDirectoryPath = await SharedProjectService.GetRunDirectoryPath(ProjectId);
        complexityData = await CyclomaticComplexityDataLoder.LoadCyclomaticComplexityData(runDirectoryPath);
    }

    protected string GetComplexityClass(int cc)
    {
        if (cc > 20) return "table-danger";
        if (cc > 10) return "table-warning";
        return "";
    }
}