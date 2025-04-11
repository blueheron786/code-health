namespace CodeHealth.Core.IO;

public static class FileAndFolderConstants
{
    public static readonly string DataDirectory = "data"; // Root directory for ALL data
    public static readonly string RunsDirectory = Path.Combine(DataDirectory, "runs"); // Root directory for runs
    public static readonly string LatestRunsFile = Path.Combine(DataDirectory, "latest-runs.json");

}