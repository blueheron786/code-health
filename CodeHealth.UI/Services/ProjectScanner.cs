using System.Diagnostics;
using CodeHealth.Core.IO;
using CodeHealth.Scanners.Common;

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
        DetectLanguagesAndRunScanners(sourcePath, sourceFiles, resultsDirectory);
        
        Console.WriteLine($"Analysis complete in {stopwatch.Elapsed}!");
        return stopwatch.Elapsed;
    }
    
    private static void DetectLanguagesAndRunScanners(string sourcePath, Dictionary<string, string> sourceFiles, string resultsDirectory)
    {
        if (sourceFiles.Any(f => f.Key.EndsWith(".cs", StringComparison.OrdinalIgnoreCase)))
        {
            // Cache files so we don't slam the disk with I/O over and over
            RunCSharpScanners(sourcePath, sourceFiles, resultsDirectory);
        }
        if (sourceFiles.Any(f => f.Key.EndsWith(".java", StringComparison.OrdinalIgnoreCase)))
        {
            RunJavaScanners(sourcePath, sourceFiles, resultsDirectory);
        }

        RunCommonScanners(sourcePath, sourceFiles, resultsDirectory);
    }

    private static void RunCSharpScanners(string sourcePath, Dictionary<string, string> sourceFiles, string resultsDirectory)
    {
        new Scanners.CSharp.CyclomaticComplexityScanner().AnalyzeFiles(sourceFiles, sourcePath, resultsDirectory);
    }

    private static void RunJavaScanners(string sourcePath, Dictionary<string, string> sourceFiles, string resultsDirectory)
    {
        new Scanners.Java.CyclomaticComplexityScanner().AnalyzeFiles(sourceFiles, sourcePath, resultsDirectory);
    }

    private static void RunCommonScanners(string sourcePath, Dictionary<string, string> sourceFiles, string resultsDirectory)
    {
        new TodoCommentScanner().AnalyzeFiles(sourceFiles, sourcePath, resultsDirectory);
    }
}
