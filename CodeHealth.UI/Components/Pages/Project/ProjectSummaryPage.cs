using System.IO;
using System.Text.Json;
using CodeHealth.Core.IO;
using CodeHealth.Scanners.CSharp.Formatters;
using Microsoft.AspNetCore.Components;

namespace CodeHealth.UI.Components.Pages.Project;

public class ProjectSummaryPage : ComponentBase
{
    [Parameter]
    public string ProjectId { get; set; }
 
    protected bool isAllDataLoaded = false;
    protected int totalComplexity;
    protected double averageComplexity;
    protected string lastScannedTime;

    protected override async Task OnInitializedAsync()
    {
        var projectMetadata = await LoadProjectMetadata(ProjectId);
        var folderName = projectMetadata?.FolderName;
        var lastRunTime = projectMetadata?.Timestamp;

        // Load cyclomatic complexity data based on the folder name
        if (folderName != null)
        {
            var complexityData = await LoadCyclomaticComplexityData(folderName);
            
            if (complexityData.Any())
            {
                totalComplexity = complexityData.Sum(x => x.Complexity);
                averageComplexity = complexityData.Average(x => x.Complexity);
            }
        }

        lastScannedTime = lastRunTime?.ToString("yyyy-MM-dd HH:mm:ss");

        isAllDataLoaded = true;
    }

    private async Task<ProjectMetadata> LoadProjectMetadata(string projectId)
    {
        var projectsMetadataFile = FileAndFolderConstants.ProjectsMetadataFile;
        if (!File.Exists(projectsMetadataFile))
        {
            return null; // Return null if no metadata exists
        }

        var json = await File.ReadAllTextAsync(projectsMetadataFile);
        var projectData = JsonSerializer.Deserialize<Dictionary<string, ProjectMetadata>>(json);

        return projectData?.FirstOrDefault(kvp => kvp.Key.EndsWith(projectId)).Value;
    }

    private async Task<List<CyclomaticComplexityData>> LoadCyclomaticComplexityData(string projectId)
    {
        var filePath = Path.Combine(FileAndFolderConstants.RunsDirectory, projectId, FileAndFolderConstants.CyclomatiComplexityFile);

        if (!File.Exists(filePath))
        {
            return new List<CyclomaticComplexityData>();
        }

        var jsonData = await File.ReadAllTextAsync(filePath);
        var report = JsonSerializer.Deserialize<CyclomaticComplexityJsonFormatter.Report>(jsonData);

        var methods = report.Files
            .SelectMany(file => file.Methods.Select(method => new CyclomaticComplexityData
            {
                File = file.File,
                Method = method.Method,
                Complexity = method.Complexity
            }))
            .OrderByDescending(m => m.Complexity)
            .ToList();

        return methods;
    }

    public class CyclomaticComplexityData
    {
        public string File { get; set; }
        public string Method { get; set; }
        public int Complexity { get; set; }
    }

    public class ProjectMetadata
    {
        public DateTime Timestamp { get; set; }
        public string FolderName { get; set; }
    }
}
