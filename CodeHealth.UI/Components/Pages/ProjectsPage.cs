using System.IO;
using System.Text.Json;
using CodeHealth.Core.IO;
using Microsoft.AspNetCore.Components;

namespace CodeHealth.UI.Components.Pages;

public partial class ProjectsPage : ComponentBase
{
    protected List<Project> projects;

    protected override async Task OnInitializedAsync()
    {
        // Load projects and their last run times from the latest-runs.json file
        projects = await LoadProjectsAsync();
    }

    private async Task<List<Project>> LoadProjectsAsync()
    {
        var latestRunsFile = FileAndFolderConstants.LatestRunsFile;
        
        if (!File.Exists(latestRunsFile))
        {
            return new List<Project>(); // Return an empty list if no data
        }

        // Read the JSON data
        var json = await File.ReadAllTextAsync(latestRunsFile);
        var latestRuns = JsonSerializer.Deserialize<Dictionary<string, DateTime>>(json);

        // Convert the dictionary to a list of projects
        var projectList = latestRuns?.Select(kvp => new Project
        {
            Name = Path.GetFileName(kvp.Key), // Get the last part of the folder path
            LastRunTime = TimeAgo(kvp.Value) // Get the time ago format
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

    public class Project
    {
        public string Name { get; set; }
        public string LastRunTime { get; set; }
    }
}