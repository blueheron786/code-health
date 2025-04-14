using CodeHealth.Core.Dtos;
using CodeHealth.Core.Dtos.CyclomaticComplexity;
using CodeHealth.UI.Services;
using CodeHealth.UI.Services.DataLoaders;
using Microsoft.AspNetCore.Components;
using System.IO;
using System.Web;

namespace CodeHealth.UI.Components.Pages;

public partial class ViewFilePage : ComponentBase
{
    [Parameter]
    public string ProjectId { get; set; }
    
    [Parameter]
    [SupplyParameterFromQuery]
    public string Path { get; set; }
    
    [Inject]
    protected NavigationManager NavigationManager { get; set; }
    
    protected string FileContent { get; set; }
    protected string[] Lines { get; set; }
    protected List<CyclomaticComplexityData> FileComplexities { get; set; }
    protected string FileName { get; set; }
    protected List<IssueResult> FileIssues { get; set; } = new();

    // Store method start and end lines for highlighting
    protected Dictionary<string, (int start, int end)> MethodRanges { get; set; } = new();

    protected override async Task OnInitializedAsync()
    {
        if (string.IsNullOrEmpty(Path))
        {
            NavigationManager.NavigateTo($"/project/{ProjectId}/cyclomatic-complexity");
            return;
        }

        var decodedPath = HttpUtility.UrlDecode(Path);
        FileName = System.IO.Path.GetFileName(decodedPath);
       
        var runDirectoryPath = await SharedProjectService.GetRunDirectoryPath(ProjectId);
        
        // Load ALL issue types (both cyclomatic complexity and long methods)
        var allIssues = await IssueResultLoader.LoadIssues(runDirectoryPath);
        FileIssues = allIssues
            .Where(i => i.File.Equals(decodedPath, StringComparison.OrdinalIgnoreCase))
            .ToList();
        
        FileContent = await LoadFileContent(decodedPath);
        Lines = FileContent.Split(new[] { "\r\n", "\r", "\n" }, StringSplitOptions.None);

        // Simple heuristic to find method locations (this would need to be language-specific)
        if (FileComplexities?.Any() == true)
        {
            foreach (var method in FileComplexities)
            {
                // This is a simplified approach - you'd want to implement proper parsing
                // for each language to find method start/end lines
                var methodIndex = FindMethodInCode(method.Method);
                if (methodIndex >= 0)
                {
                    MethodRanges[method.Method] = (methodIndex, FindMethodEnd(methodIndex));
                }
            }
        }
        
    }

    private int FindMethodInCode(string methodName)
    {
        // Simplified method finder - you'll need to implement proper parsing
        // based on the language (C#, Java, etc.)
        for (int i = 0; i < Lines.Length; i++)
        {
            if (Lines[i].Contains(methodName) && 
                (Lines[i].Contains(" void ") || 
                 Lines[i].Contains(" int ") || 
                 Lines[i].Contains(" string ") ||
                 Lines[i].Contains(" public ") ||
                 Lines[i].Contains(" private ")))
            {
                return i + 1; // Convert to 1-based line number
            }
        }
        return -1;
    }

    private int FindMethodEnd(int startLine)
    {
        // Simplified method end finder - should implement proper parsing
        int braceCount = 0;
        bool firstBraceFound = false;
        int currentLine = startLine - 1; // Convert to 0-based index
        
        while (currentLine < Lines.Length)
        {
            var line = Lines[currentLine];
            if (line.Contains("{"))
            {
                braceCount++;
                firstBraceFound = true;
            }
            if (line.Contains("}"))
            {
                braceCount--;
            }
            
            if (firstBraceFound && braceCount == 0)
            {
                return currentLine + 1; // Convert to 1-based line number
            }
            
            currentLine++;
        }
        
        return Lines.Length; // Default to end of file if we can't find the closing brace
    }

    protected List<IssueResult> GetIssuesForLine(int lineNumber)
    {
        return FileIssues
            .Where(issue => lineNumber >= issue.Line && lineNumber <= issue.EndLine)
            .ToList();
    }

    protected string GetIssueClass(IssueResult issue)
    {
        // Cyclomatic Complexity
        if (issue.Type == "Method" && issue.Metric?.Name == "Cyclomatic Complexity")
        {
            if (issue.Metric.Value > 20) return "high-complexity";
            if (issue.Metric.Value > 10) return "medium-complexity";
            if (issue.Metric.Value > 5) return "low-complexity";
        }
        // Long Methods
        else if (issue.Type == "Method" && issue.Metric?.Name == "LineCount")
        {
            return "long-method";
        }
        return string.Empty;
    }

    private async Task<string> LoadFileContent(string filePath)
    {
        var projectSourcePath = await SharedProjectService.GetProjectSourcePath(ProjectId);
        var absolutePath = System.IO.Path.Combine(projectSourcePath, filePath);
        if (File.Exists(absolutePath))
        {
            var text = await File.ReadAllTextAsync(absolutePath);
            return text;
        }
        else
        {
            // not necessarily a coding mistake; maybe they deleted the file since the analysis.
            return $"// Source file not found: {absolutePath}";
        }
    }

    protected void NavigateBack()
    {
        NavigationManager.NavigateTo($"/project/{ProjectId}/cyclomatic-complexity");
    }
}