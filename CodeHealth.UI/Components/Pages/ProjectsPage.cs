using System.IO;
using System.Text.Json;
using CodeHealth.Core.Dtos;
using CodeHealth.Core.IO;
using Microsoft.AspNetCore.Components;

namespace CodeHealth.UI.Components.Pages;

public partial class ProjectsPage : ComponentBase
{
    protected List<Project> projects;

    protected override async Task OnInitializedAsync()
    {
        // Load projects and their last run times from the projects metadata file
        projects = await LoadProjectsAsync();
    }

    private async Task<List<Project>> LoadProjectsAsync()
    {
        var projectsMetadataFile = FileAndFolderConstants.ProjectsMetadataFile;
        
        if (!File.Exists(projectsMetadataFile))
        {
            return new List<Project>(); // Return an empty list if no data
        }

        // Read the JSON data
        var json = await File.ReadAllTextAsync(projectsMetadataFile);
        
        // Deserialize into a dictionary of folder paths to ProjectInfo objects
        var projectData = JsonSerializer.Deserialize<Dictionary<string, ProjectInfo>>(json);

        // Convert the dictionary to a list of projects
        var projectList = projectData?.Select(kvp => new Project
        {
            Id = Path.GetFileName(kvp.Key), // Use the folder name as the ID
            Name = Path.GetFileName(kvp.Key), // Get the last part of the folder path
            FolderName = kvp.Value.FolderName,
            Timestamp = kvp.Value.Timestamp,
            LastRunTime = TimeAgo(kvp.Value.Timestamp) // Get the time ago format
        }).ToList();

        return projectList ?? new List<Project>();
    }


    private string TimeAgo(DateTime runTime)
    {
        var timeSpan = DateTime.Now - runTime;

        if (timeSpan.TotalSeconds < 60)
        {
            return $"{timeSpan.Seconds} seconds ago";
        }
        else if (timeSpan.TotalMinutes < 60)
        {
            return $"{timeSpan.Minutes} minutes ago";
        }
        else if (timeSpan.TotalHours < 24)
        {
            return $"{timeSpan.Hours} hours ago";
        }
        else if (timeSpan.TotalDays < 30)
        {
            return $"{timeSpan.Days} days ago";
        }
        else
        {
            return runTime.ToString("yyyy-MM-dd"); // If it's more than a month, show the date
        }
    }

    protected string GetProjectLink(string projectId)
    {
        return $"/project/{projectId}"; // Create the link for each project
    }

    public class Project
    {
        public string Id { get; set; }
        public string Name { get; set; }
        public string LastRunTime { get; set; }
        public string FolderName { get; set; }
        public DateTime Timestamp { get; set; }
    }

}
