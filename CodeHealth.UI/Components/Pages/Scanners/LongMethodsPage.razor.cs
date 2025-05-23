using CodeHealth.Core.Dtos;
using CodeHealth.Core.IO;
using CodeHealth.UI.Services;
using CodeHealth.UI.Services.DataLoaders;
using Microsoft.AspNetCore.Components;

namespace CodeHealth.UI.Components.Pages.Scanners
{
    public partial class LongMethodsPage : ComponentBase
    {
        [Parameter]
        public string ProjectId { get; set; }
        protected string ProjectRootDirectory { get; set; }

        protected List<IssueResult> ScannerData;

        protected override async Task OnInitializedAsync()
        {
            // Retrieve the project path and run directory path
            var runDirectoryPath = await SharedProjectService.GetRunDirectoryPath(ProjectId);
            ProjectRootDirectory = await SharedProjectService.GetProjectSourcePath(ProjectId);

            // Load the TODO data
            ScannerData = await ScannerResultsDataLoader.LoadScannerResultsAsync(ProjectId, runDirectoryPath, Constants.FileNames.LongMethodsFile);
        }

        protected string GetLongMethodsBadgeClass(IssueResult r)
        {
            // For long methods, we'll use a single class since it's a binary condition
            return "badge-long-method";
        }

        protected string GetLongMethodsBadgeText(IssueResult r)
        {
            // Already showing value like 52, show "52 lines"
            return $"{r.Metric.Value} lines";
        }
    }
}
