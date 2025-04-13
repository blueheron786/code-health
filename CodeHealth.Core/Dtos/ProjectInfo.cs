namespace CodeHealth.Core.Dtos;

public class ProjectInfo
{
    /// <summary>
    /// When did the scan run?
    /// </summary>
    public DateTime Timestamp { get; set; }

    /// <summary>
    /// What's the (relative) folder name with the latest run data?
    /// </summary>
    public string FolderName { get; set; }
}