namespace CodeHealth.Core.IO;

using System.Text.Json;

public static class RunInfo
{
    public static string CreateRun(string folderPath, DateTime runTime)
    {
        var directory = GetDirectoryName(runTime);
        Directory.CreateDirectory(directory);

        var metaData = new {
            Folder = folderPath,
        };
        string metaDataJson = JsonSerializer.Serialize(metaData);
        
        File.WriteAllText(Path.Join(directory, "metadata.json"), metaDataJson);

        return directory;
    }

    private static string GetDirectoryName(DateTime runTime)
    {
        return Path.Combine("runs", runTime.ToString("yyyy-MM-dd_HH-mm-ss"));
    }
}