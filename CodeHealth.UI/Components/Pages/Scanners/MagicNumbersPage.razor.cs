using CodeHealth.Core.Dtos;
using CodeHealth.Core.IO;
using CodeHealth.UI.Services;
using CodeHealth.UI.Services.DataLoaders;
using Microsoft.AspNetCore.Components;

namespace CodeHealth.UI.Components.Pages.Scanners
{
    public partial class MagicNumbersPage : ComponentBase
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
            ScannerData = await ScannerResultsDataLoader.LoadScannerResultsAsync(ProjectId, runDirectoryPath, Constants.FileNames.MagicNumbersFile);
        }

        protected string GetMagicNumberBadgeClass(IssueResult r) => "magic-number";
        protected string GetMagicNumberBadgeText(IssueResult r) => $"on line {r.Line}";

    }
}
