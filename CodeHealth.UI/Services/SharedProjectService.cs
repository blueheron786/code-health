using System.IO;
using System.Text.Json;
using CodeHealth.Core.IO;

namespace CodeHealth.UI.Services;

public static class SharedProjectService
{
    public static async Task<string> GetRunDirectoryPath(string projectId)
    {
        if (!File.Exists(FileAndFolderConstants.ProjectsMetadataFile))
        {
            throw new FileNotFoundException("projects.json not found");
        }

        var projectsJson = await File.ReadAllTextAsync(FileAndFolderConstants.ProjectsMetadataFile);
        var projects = JsonSerializer.Deserialize<Dictionary<string, ProjectInfo>>(projectsJson);
        
        var data = projects.SingleOrDefault(p => p.Key.EndsWith(projectId));
        return data.Value.FolderName;

        throw new KeyNotFoundException($"Project {projectId} not found in projects.json");
    }
}

public class ProjectInfo
{
    public DateTime Timestamp { get; set; }
    public string FolderName { get; set; }
}