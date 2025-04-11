namespace CodeHealth.Core.IO;

public static class Constants
{
    public static class DirectoryNames
    {
        public const string DataDirectory = "data"; // Root directory for ALL data
        public static readonly string RunsDirectory = Path.Combine(DataDirectory, "runs"); // Root directory for runs
    }

    public static class FileNames
    {
        public static readonly string ProjectsMetadataFile = Path.Combine(DirectoryNames.DataDirectory, "projects.json");
        // Scanner outputs
        public const string CyclomatiComplexityFile = "cyclomatic_complexity.json";
        public const string TodoCommentsFile = "todos.json";
    }
}