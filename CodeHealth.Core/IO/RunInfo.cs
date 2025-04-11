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
            if (!File.Exists(FileAndFolderConstants.LatestRunsFile))
            {
                File.WriteAllText(FileAndFolderConstants.LatestRunsFile, "{}"); // Create an empty JSON file if it doesn't exist
            }

            // Read the existing latest-runs.json file
            var latestRunsJson = File.ReadAllText(FileAndFolderConstants.LatestRunsFile);
            var latestRuns = JsonSerializer.Deserialize<Dictionary<string, DateTime>>(latestRunsJson) ?? new Dictionary<string, DateTime>();

            // Update the folder's latest run time
            latestRuns[folderPath] = runTime;

            // Save the updated latest-runs.json file
            var updatedJson = JsonSerializer.Serialize(latestRuns, new JsonSerializerOptions { WriteIndented = true });
            File.WriteAllText(FileAndFolderConstants.LatestRunsFile, updatedJson);
        }
    }
}
