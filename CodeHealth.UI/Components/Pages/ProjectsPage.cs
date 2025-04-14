using System.IO;
using System.Text.Json;
using System.Linq;
using CodeHealth.Core.Dtos;
using CodeHealth.Core.IO;
using Microsoft.AspNetCore.Components;

namespace CodeHealth.UI.Components.Pages;

public partial class ProjectsPage : ComponentBase
{
    protected List<Project> projects;
    protected List<Project> filteredProjects;
    protected string searchQuery = string.Empty;
    private string sortColumn = "Name";
    private bool sortAscending = true;

    protected override async Task OnInitializedAsync()
    {
        projects = await LoadProjectsAsync();
        ApplyFilterAndSort();
    }

    protected async void RefreshProjectsList()
    {
        projects = await LoadProjectsAsync();
        ApplyFilterAndSort();
        StateHasChanged();
    }

    private void ApplyFilterAndSort()
    {
        // Apply search filter
        var filtered = string.IsNullOrWhiteSpace(searchQuery)
            ? projects
            : projects.Where(p => 
                p.Name.Contains(searchQuery, StringComparison.OrdinalIgnoreCase) ||
                p.PrimaryLanguage.Contains(searchQuery, StringComparison.OrdinalIgnoreCase));

        // Apply sorting
        filteredProjects = sortColumn switch
        {
            "Name" => sortAscending 
                ? filtered.OrderBy(p => p.Name).ToList()
                : filtered.OrderByDescending(p => p.Name).ToList(),
            "PrimaryLanguage" => sortAscending
                ? filtered.OrderBy(p => p.PrimaryLanguage).ToList()
                : filtered.OrderByDescending(p => p.PrimaryLanguage).ToList(),
            "LastRunTime" => sortAscending
                ? filtered.OrderBy(p => p.Timestamp).ToList()
                : filtered.OrderByDescending(p => p.Timestamp).ToList(),
            _ => filtered.ToList()
        };
    }

    protected void HandleSearch(ChangeEventArgs e)
    {
        searchQuery = e.Value?.ToString() ?? string.Empty;
        ApplyFilterAndSort();
    }

    protected void SortBy(string column)
    {
        if (sortColumn == column)
        {
            sortAscending = !sortAscending;
        }
        else
        {
            sortColumn = column;
            sortAscending = true;
        }
        ApplyFilterAndSort();
    }

    protected string GetSortIcon(string column)
    {
        if (sortColumn != column) return string.Empty;
        return sortAscending ? "↑" : "↓";
    }

    private async Task<List<Project>> LoadProjectsAsync()
    {
        var projectsMetadataFile = Constants.FileNames.ProjectsMetadataFile;
        
        if (!File.Exists(projectsMetadataFile))
        {
            return new List<Project>();
        }

        var json = await File.ReadAllTextAsync(projectsMetadataFile);
        var projectData = JsonSerializer.Deserialize<Dictionary<string, ProjectInfo>>(json);

        var projectList = projectData?.Select(kvp => new Project
        {
            Id = Path.GetFileName(kvp.Key),
            Name = Path.GetFileName(kvp.Key),
            FolderName = kvp.Value.FolderName,
            Timestamp = kvp.Value.Timestamp,
            LastRunTime = TimeAgo(kvp.Value.Timestamp),
            //PrimaryLanguage = GetPrimaryLanguage(kvp.Value.Languages) // Add primary language
        }).ToList();

        return projectList ?? new List<Project>();
    }

    private string GetPrimaryLanguage(Dictionary<string, decimal> languages)
    {
        if (languages == null || languages.Count == 0)
            return "Unknown";
            
        return languages.OrderByDescending(x => x.Value)
                      .FirstOrDefault().Key;
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
            return runTime.ToString("yyyy-MM-dd");
        }
    }

    protected string GetProjectLink(string projectId)
    {
        return $"/project/{projectId}";
    }

    public class Project
    {
        public string Id { get; set; }
        public string Name { get; set; }
        public string LastRunTime { get; set; }
        public string FolderName { get; set; }
        public DateTime Timestamp { get; set; }
        public string PrimaryLanguage { get; set; }
    }
}