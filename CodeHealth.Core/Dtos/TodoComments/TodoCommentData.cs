namespace CodeHealth.Core.Dtos.TodoComments;

public class TodoCommentData
{
    public string File { get; set; } = string.Empty;
    public int Line { get; set; }
    public string Text { get; set; } = string.Empty;
    // The few lines before/after our TODO line
    public List<string> Context { get; set; } = new();
}
