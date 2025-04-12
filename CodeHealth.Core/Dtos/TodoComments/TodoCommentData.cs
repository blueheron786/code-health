namespace CodeHealth.Core.Dtos.TodoComments;

public class TodoCommentData
{
    public string File { get; set; } = string.Empty;
    public int Line { get; set; }
    public string Text { get; set; } = string.Empty;
}