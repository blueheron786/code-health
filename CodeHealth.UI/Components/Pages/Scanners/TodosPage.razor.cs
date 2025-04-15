using CodeHealth.Core.Dtos;
using CodeHealth.Core.IO;
using CodeHealth.UI.Services;
using CodeHealth.UI.Services.DataLoaders;
using Microsoft.AspNetCore.Components;

namespace CodeHealth.UI.Components.Pages.Scanners
{
    /// <summary>
    /// Shows the TODO comments and their surrounding code (based on current on-disk code).
    /// </summary>
    public partial class TodosPage : ComponentBase
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
            ScannerData = await ScannerResultsDataLoader.LoadScannerResultsAsync(ProjectId, runDirectoryPath, Constants.FileNames.TodoCommentsFile);
        }

        protected string GetTodoBadgeClass(IssueResult r)
        {
            var text = r.Message?.ToLowerInvariant() ?? "";
            if (text.Contains("fixme")) return "badge-fixme";
            if (text.Contains("hack")) return "badge-hack";
            return "badge-todo";
        }

        protected string GetTodoBadgeText(IssueResult r)
        {
            var text = r.Message?.ToLowerInvariant() ?? "";
            if (text.Contains("fixme")) return "FIXME";
            if (text.Contains("hack")) return "HACK";
            return "TODO";
        }
    }
}
