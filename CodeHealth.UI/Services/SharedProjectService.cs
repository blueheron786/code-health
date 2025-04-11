using System.IO;
using System.Text.Json;
using CodeHealth.Core.Dtos;
using CodeHealth.Core.IO;

namespace CodeHealth.UI.Services;

public static class SharedProjectService
{
    public static async Task<string> GetRunDirectoryPath(string projectId)
    {
        var projects = await GetAllProjectsInfo();
        if (projects == null)
        {
            throw new InvalidOperationException("Project metadata files should exist, but doesn't.");
        }
        
        var data = projects.FirstOrDefault(p => Path.GetFileName(p.Key).Equals(projectId, StringComparison.OrdinalIgnoreCase));

        if (data.Equals(default(KeyValuePair<string, ProjectInfo>)))
        {
            throw new KeyNotFoundException($"Project {projectId} not found in projects.json");
        }
        return data.Value.FolderName;
    }

    public static async Task<ProjectInfo> GetProjectInfo(string projectId)
    {
        var projectData = await GetAllProjectsInfo();
        return projectData?.FirstOrDefault(kvp => kvp.Key.EndsWith(projectId)).Value;
    }

    public static async Task<string> GetProjectSourcePath(string projectId)
    {
        var projects = await GetAllProjectsInfo();
        var match = projects.FirstOrDefault(p => 
            Path.GetFileName(p.Key).Equals(projectId, StringComparison.OrdinalIgnoreCase));

        if (match.Equals(default(KeyValuePair<string, ProjectInfo>)))
        {
            throw new KeyNotFoundException($"Project {projectId} not found in projects.json");
        }

        return match.Key; // This is the full source path, like "D:\projects\code-health"
    }


    private static async Task<Dictionary<string, ProjectInfo>> GetAllProjectsInfo()
    {
        var projectsMetadataFile = Constants.FileNames.ProjectsMetadataFile;
        if (!File.Exists(projectsMetadataFile))
        {
            return null; // Return null if no metadata exists
        }

        var json = await File.ReadAllTextAsync(projectsMetadataFile);
        var projectData = JsonSerializer.Deserialize<Dictionary<string, ProjectInfo>>(json);
        return projectData;
    }
}