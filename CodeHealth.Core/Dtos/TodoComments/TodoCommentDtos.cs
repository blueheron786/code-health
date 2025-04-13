namespace CodeHealth.Core.Dtos.TodoComments;

public class TodoCommentsReport
{
    public int TotalTodos { get; set; }
    public List<FileResult> Files { get; set; } = new();
}

public class FileResult
{
    public string File { get; set; } = string.Empty;
    public List<CommentResult> Comments { get; set; } = new();
}

public class CommentResult
{
    public int Line { get; set; }
}
