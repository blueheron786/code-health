using System.IO;
using CodeHealth.Core.Dtos;
using CodeHealth.Core.IO;
using CodeHealth.UI.Services;
using CodeHealth.UI.Services.DataLoaders;
using Microsoft.AspNetCore.Components;

namespace CodeHealth.UI.Components.Pages.Scanners;

/// <summary>
/// Shows the actual TODOs and surrounding code. Note that this uses the current code
/// (on-disk), which might not be the same as the code at the time you scanned it.
/// </summary>
public class TodosPage : ComponentBase
{
    private const int LinesToShowBeforeAndAfterTodo = 4;

    [Parameter]
    public string ProjectId { get; set; }

    protected List<IssueResult> todoData;

    protected override async Task OnInitializedAsync()
    {
        var runDirectoryPath = await SharedProjectService.GetRunDirectoryPath(ProjectId);
        var projectSourcePath = await SharedProjectService.GetProjectSourcePath(ProjectId);
        todoData = await ScannerResultsDataLoader.LoadScannerResultsAsync(ProjectId, runDirectoryPath, Constants.FileNames.TodoCommentsFile);

        foreach (var todo in todoData)
        {
            // Resolve the full path using the project source path
            var sourceFilePath = Path.Combine(projectSourcePath, todo.File.Replace('/', Path.DirectorySeparatorChar));

            if (File.Exists(sourceFilePath))
            {
                var lines = await File.ReadAllLinesAsync(sourceFilePath);
                var start = Math.Max(0, todo.Line - LinesToShowBeforeAndAfterTodo - 1); // zero-based index
                var end = Math.Min(lines.Length, todo.Line + LinesToShowBeforeAndAfterTodo);

                // todo.Context = lines.Skip(start).Take(end - start).ToList();
            }
            else
            {
                // not necessarily a coding mistake; maybe they deleted the file since the analysis.
                // todo.Context = new List<string> { "// Source file not found: " + todo.File };
            }
        }
    }
}
