using System.IO;
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
        private const int LinesToShowBeforeAndAfterTodo = 4;

        [Parameter]
        public string ProjectId { get; set; }
        protected string ProjectRootDirectory { get; set; }

        protected List<IssueResult> todoData;

        protected override async Task OnInitializedAsync()
        {
            // Retrieve the project path and run directory path
            var runDirectoryPath = await SharedProjectService.GetRunDirectoryPath(ProjectId);
            ProjectRootDirectory = await SharedProjectService.GetProjectSourcePath(ProjectId);

            // Load the TODO data
            todoData = await ScannerResultsDataLoader.LoadScannerResultsAsync(ProjectId, runDirectoryPath, Constants.FileNames.TodoCommentsFile);

            // Process each TODO entry
            foreach (var todo in todoData)
            {
                // Resolve the full path of the source file
                var sourceFilePath = Path.Combine(ProjectRootDirectory, todo.File.Replace('/', Path.DirectorySeparatorChar));

                if (File.Exists(sourceFilePath))
                {
                    // Read the file and retrieve lines around the TODO comment
                    var lines = await File.ReadAllLinesAsync(sourceFilePath);
                    var start = Math.Max(0, todo.Line - LinesToShowBeforeAndAfterTodo - 1); // zero-based index
                    var end = Math.Min(lines.Length, todo.Line + LinesToShowBeforeAndAfterTodo);

                    // Optionally store the context (surrounding lines) for future use
                    // todo.Context = lines.Skip(start).Take(end - start).ToList();
                }
                else
                {
                    // Handle cases where the file might have been deleted or is missing
                    // todo.Context = new List<string> { "// Source file not found: " + todo.File };
                }
            }
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
