using System.Text.Json;
using CodeHealth.Core.Dtos;

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
            return Path.Combine(FileAndFolderConstants.RunsDirectory, runTime.ToString("yyyyMMdd_HHmmss"));
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
            var projectsMetadata = JsonSerializer.Deserialize<Dictionary<string, ProjectInfo>>(projectsMetadataJson) ?? new Dictionary<string, ProjectInfo>();

            // Update the metadata with the new folder's timestamp and folder name
            projectsMetadata[folderPath] = new ProjectInfo
            {
                Timestamp = runTime,
                FolderName = GetDirectoryName(runTime),
            };

            // Save the updated latest-runs.json file
            var updatedJson = JsonSerializer.Serialize(projectsMetadata, new JsonSerializerOptions { WriteIndented = true });
            File.WriteAllText(FileAndFolderConstants.ProjectsMetadataFile, updatedJson);
        }
    }
}
