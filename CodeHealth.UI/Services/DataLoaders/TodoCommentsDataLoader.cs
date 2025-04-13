using System.IO;
using System.Text.Json;
using CodeHealth.Core.Dtos.TodoComments;
using CodeHealth.Core.IO;

namespace CodeHealth.UI.Services.DataLoaders;

public static class TodoCommentDataLoader
{
    public static async Task<List<TodoCommentData>> LoadTodoCommentsAsync(string projectId, string runDirectoryPath)
    {
        var projectSourcePath = await SharedProjectService.GetProjectSourcePath(projectId);
        var todoReportFilePath = Path.Combine(runDirectoryPath, Constants.FileNames.TodoCommentsFile);

        if (!File.Exists(todoReportFilePath))
        {
            return new List<TodoCommentData>();
        }

        var jsonData = await File.ReadAllTextAsync(todoReportFilePath);
        var report = JsonSerializer.Deserialize<TodoCommentsReport>(jsonData);

        var todos = new List<TodoCommentData>();

        if (report?.Files != null)
        {
            foreach (var file in report.Files)
            {
                var absolutePath = Path.Combine(projectSourcePath, file.File);
                if (!File.Exists(absolutePath))
                {
                    continue;
                }

                var sourceLines = await File.ReadAllLinesAsync(absolutePath);
                
                foreach (var comment in file.Comments)
                {
                    var lineText = comment.Line > 0 && comment.Line <= sourceLines.Length 
                        ? sourceLines[comment.Line - 1].Trim() 
                        : "// Unable to read source line";

                    todos.Add(new TodoCommentData
                    {
                        File = absolutePath,
                        Line = comment.Line,
                        Text = lineText,
                        Context = GetContextLines(sourceLines, comment.Line)
                    });
                }
            }
        }

        return todos.OrderBy(c => c.File)
                    .ThenBy(c => c.Line)
                    .ToList();
    }

    private static List<string> GetContextLines(string[] sourceLines, int lineNumber, int contextSize = 3)
    {
        var context = new List<string>();
        int start = Math.Max(1, lineNumber - contextSize);
        int end = Math.Min(sourceLines.Length, lineNumber + contextSize);

        for (int i = start; i <= end; i++)
        {
            context.Add(sourceLines[i - 1]);
        }

        return context;
    }
}