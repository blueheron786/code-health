using System.IO;
using System.Text.Json;
using CodeHealth.Core.IO;
using CodeHealth.UI.Services;
using CodeHealth.UI.Services.DataLoaders;
using Microsoft.AspNetCore.Components;

namespace CodeHealth.UI.Components.Pages.Project;

public partial class ProjectSummaryPage : ComponentBase
{
    [Parameter]
    public string ProjectId { get; set; }

    [Inject]
    protected NavigationManager NavigationManager { get; set; }

    protected bool isAllDataLoaded = false;
    protected string lastScannedTime;
    protected string scanResultsMessage;
    protected Dictionary<string, double> languageBreakdown;

    // Scanner-specific totals/summaries
    protected int totalComplexity;
    protected double averageComplexity;
    protected int totalTodos;
    protected int totalLongMethods;
    protected double averageLongMethodLength;
    protected int totalMagicNumbers;
    protected double averageMagicNumberCount;

    protected override async Task OnInitializedAsync()
    {
        var projectMetadata = await SharedProjectService.GetProjectInfo(ProjectId);
        var folderName = projectMetadata?.FolderName;
        var lastRunTime = projectMetadata?.Timestamp;

        // Load run data based on the folder name
        if (folderName != null)
        {
            // CCD (Cyclomatic Complexity Data)
            var complexityData = await ScannerResultsDataLoader.LoadScannerResultsAsync(ProjectId, folderName, Constants.FileNames.CyclomatiComplexityFiles);
            if (complexityData.Any())
            {
                complexityData = complexityData.Where(x => x.Metric.Value > 1).ToList(); // Ignore trivial cases
                totalComplexity = complexityData.Sum(x => x.Metric.Value);
                averageComplexity = complexityData.Average(x => x.Metric.Value);
            }

            // TODOs
            var todoData = await ScannerResultsDataLoader.LoadScannerResultsAsync(ProjectId, folderName, Constants.FileNames.TodoCommentsFile);
            if (todoData.Any())
            {
                totalTodos = todoData.Count;
            }

            // (Overly) long methods
            var longMethodsData = await ScannerResultsDataLoader.LoadScannerResultsAsync(ProjectId, folderName, Constants.FileNames.LongMethodsFile);
            if (longMethodsData.Any())
            {
                totalLongMethods = longMethodsData.Count();
                averageLongMethodLength = longMethodsData.Average(x => x.Metric.Value);
            }

            // Magic Numbers
            var magicNumbersData = await ScannerResultsDataLoader.LoadScannerResultsAsync(ProjectId, folderName, Constants.FileNames.MagicNumbersFile);
            if (magicNumbersData.Any())
            {
                totalMagicNumbers = magicNumbersData.Count();
                averageMagicNumberCount = magicNumbersData.Average(x => x.Metric.Value);
            }

            // Load language distribution from JSON file
            var languageDataPath = Path.Combine(folderName, "language_distribution.json");
            if (File.Exists(languageDataPath))
            {
                var languageData = await File.ReadAllTextAsync(languageDataPath);
                languageBreakdown = JsonSerializer.Deserialize<Dictionary<string, double>>(languageData);
            }
            else
            {
                languageBreakdown = new Dictionary<string, double>();
            }
        }

        lastScannedTime = lastRunTime?.ToString("yyyy-MM-dd HH:mm:ss") ?? "Never Scanned";

        isAllDataLoaded = true;
    }

    protected async void ScanProject()
    {
        scanResultsMessage = "⚡ Scanning ⚡";
        var folderPath = await SharedProjectService.GetProjectSourcePath(ProjectId);
        
        try
        {
            var results = ProjectScanner.Scan(folderPath);
            scanResultsMessage = $"Scan of {folderPath} done in {results.TotalSeconds.ToString("F2")} seconds";
        }
        catch (Exception ex)
        {
            scanResultsMessage = $"Scan failed: {ex.Message}!";
        }

        // Refresh UI
        await OnInitializedAsync();
        StateHasChanged();
    }

    protected string GetCountBadgeClass(int count)
    {
        if (count > 10) return "high-count";
        if (count > 5) return "medium-count";
        return "low-count";
    }

    protected string GetCountBadgeText(int count)
    {
        if (count > 10) return "High";
        if (count > 5) return "Medium";
        return "Low";
    }

    protected void NavigateBack()
    {
        NavigationManager.NavigateTo($"/projects");
    }
}
