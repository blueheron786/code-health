namespace CodeHealth.Core.Dtos.TodoComments;

// TODO (har har, ironic I know): delete. We use generic IssueResult now.
public class TodoCommentData
{
    public string File { get; set; } = string.Empty;
    public int Line { get; set; }
    
    // These are derived from the current code, not stored in JSON
    public string Text { get; set; } = string.Empty;
    // The few lines before/after our TODO line
    public List<string> Context { get; set; } = new();
}
