using System.IO;
using System.Text.Json;
using CodeHealth.Core.Dtos;
using CodeHealth.Core.IO;

namespace CodeHealth.UI.Services;

public static class SharedProjectService
{
    public static async Task<string> GetRunDirectoryPath(string projectId)
    {
        var projects = await GetProjectInfo();
        if (projects == null)
        {
            throw new InvalidOperationException("Project metadata files should exist, but doesn't.");
        }
        
        var data = projects.SingleOrDefault(p => p.Key.EndsWith(projectId));
        return data.Value.FolderName;

        throw new KeyNotFoundException($"Project {projectId} not found in projects.json");
    }

    public static async Task<ProjectInfo> LoadProjectInfo(string projectId)
    {
        var projectData = await GetProjectInfo();
        return projectData?.FirstOrDefault(kvp => kvp.Key.EndsWith(projectId)).Value;
    }

    private static async Task<Dictionary<string, ProjectInfo>> GetProjectInfo()
    {
        var projectsMetadataFile = FileAndFolderConstants.ProjectsMetadataFile;
        if (!File.Exists(projectsMetadataFile))
        {
            return null; // Return null if no metadata exists
        }

        var json = await File.ReadAllTextAsync(projectsMetadataFile);
        var projectData = JsonSerializer.Deserialize<Dictionary<string, ProjectInfo>>(json);
        return projectData;
    }
}