using System.IO;
using System.Text.Json;
using CodeHealth.Core.Dtos.TodoComments;
using CodeHealth.Core.IO;

namespace CodeHealth.UI.Services.DataLoaders;


public static class TodoCommentDataLoader
{
    public static async Task<List<TodoCommentData>> LoadTodoCommentsAsync(string runDirectoryPath)
    {
        var filePath = Path.Combine(runDirectoryPath, Constants.FileNames.TodoCommentsFile);

        if (!File.Exists(filePath))
        {
            return new List<TodoCommentData>();
        }

        var jsonData = await File.ReadAllTextAsync(filePath);
        var report = JsonSerializer.Deserialize<TodoCommentsReport>(jsonData);

        var todos = report?.Files?
            .SelectMany(file => file.Comments.Select(comment => new TodoCommentData
            {
                File = file.File,
                Line = comment.Line,
                Text = comment.Text
            }))
            .OrderBy(c => c.File)
            .ThenBy(c => c.Line)
            .ToList() ?? new List<TodoCommentData>();

        return todos;
    }

}
