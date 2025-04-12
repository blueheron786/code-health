using System.Diagnostics;
using CodeHealth.Core.IO;
using CodeHealth.Scanners.CSharp;

namespace CodeHealth.UI.Services;

public static class ProjectScanner
{
    public static TimeSpan Scan(string sourcePath)
    {
        var stopwatch = new Stopwatch();
        stopwatch.Start();

        Console.WriteLine($"Analyzing {sourcePath} ...");

        // Common stuff
        var resultsDirectory = RunInfo.CreateRun(sourcePath, DateTime.Now);
        var sourceFiles = FileDiscoverer.DiscoverSourceFiles(sourcePath);

        // Common and language-specific scans
        LanguageLineCounter.AnalyzeLanguageBreakdown(sourceFiles, resultsDirectory);
        RunScanners(sourcePath, sourceFiles, resultsDirectory);
        
        Console.WriteLine($"Analysis complete in {stopwatch.Elapsed}!");
        return stopwatch.Elapsed;
    }
    
    private static void RunScanners(string sourcePath, Dictionary<string, string> sourceFiles, string resultsDirectory)
    {
        new CyclomaticComplexityScanner().AnalyzeFiles(sourceFiles, sourcePath, resultsDirectory);
        new TodoCommentScanner().AnalyzeFiles(sourceFiles, sourcePath, resultsDirectory);
    }
}
