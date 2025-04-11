using System.Text.Json;

namespace CodeHealth.Core.IO
{
    public static class RunInfo
    {
        public static string CreateRun(string folderPath, DateTime runTime)
        {
            // Create a directory for the current run based on the timestamp
            var directory = GetDirectoryName(runTime);
            Directory.CreateDirectory(directory);

            // Update the latest-runs.json file
            UpdateLatestRuns(folderPath, runTime);

            return directory;
        }

        private static string GetDirectoryName(DateTime runTime)
        {
            // Generate a folder name based on the run time (e.g., "runs/2025-04-10_10-30-45")
            return Path.Combine(FileAndFolderConstants.RunsDirectory, runTime.ToString("yyyy-MM-dd_HH-mm-ss"));
        }

        private static void UpdateLatestRuns(string folderPath, DateTime runTime)
        {
            // Ensure the latest-runs.json file exists
            if (!File.Exists(FileAndFolderConstants.ProjectsMetadataFile))
            {
                File.WriteAllText(FileAndFolderConstants.ProjectsMetadataFile, "{}"); // Create an empty JSON file if it doesn't exist
            }

            // Read the existing latest-runs.json file
            var projectsMetadataJson = File.ReadAllText(FileAndFolderConstants.ProjectsMetadataFile);
            var projectsMetadata = JsonSerializer.Deserialize<Dictionary<string, ProjectMetadata>>(projectsMetadataJson) ?? new Dictionary<string, ProjectMetadata>();

            // Create the new folderName
            var folderName = Path.GetFileName(folderPath);

            // Update the metadata with the new folder's timestamp and folder name
            projectsMetadata[folderPath] = new ProjectMetadata
            {
                Timestamp = runTime,
                FolderName = folderName
            };

            // Save the updated latest-runs.json file
            var updatedJson = JsonSerializer.Serialize(projectsMetadata, new JsonSerializerOptions { WriteIndented = true });
            File.WriteAllText(FileAndFolderConstants.ProjectsMetadataFile, updatedJson);
        }
    }

    // Create a class for project metadata to hold the timestamp and folder name
    public class ProjectMetadata
    {
        public DateTime Timestamp { get; set; }
        public string FolderName { get; set; }
    }
}
