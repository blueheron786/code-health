using System.Diagnostics;
using CodeHealth.Core.IO;
using CodeHealth.Scanners.Common;

namespace CodeHealth.UI.Services;

public static class ProjectScanner
{
    private static Dictionary<string, Action<string, Dictionary<string, string>, string>> _languageSpecificScanners = new()
    {
        { ".cs", RunCSharpScanners },
        { ".java", RunJavaScanners },
        { ".kt", RunKotlinScanners },
    };


    public static TimeSpan Scan(string sourcePath)
    {
        var stopwatch = new Stopwatch();
        stopwatch.Start();

        Console.WriteLine($"Analyzing {sourcePath} ...");

        // Common stuff
        var resultsDirectory = RunInfo.CreateRun(sourcePath, DateTime.Now);
        var sourceFiles = FileDiscoverer.DiscoverSourceFiles(sourcePath);

        // Common and language-specific scans
        DetectLanguagesAndRunScanners(sourcePath, sourceFiles, resultsDirectory);
        
        Console.WriteLine($"Analysis complete in {stopwatch.Elapsed}!");
        return stopwatch.Elapsed;
    }
    
    private static void DetectLanguagesAndRunScanners(string sourcePath, Dictionary<string, string> sourceFiles, string resultsDirectory)
    {
        foreach (var kvp in _languageSpecificScanners)
        {
            var extension = kvp.Key;
            var scannersMethod = kvp.Value;

            if (sourceFiles.Any(f => f.Key.EndsWith(kvp.Key, StringComparison.OrdinalIgnoreCase)))
            {
                scannersMethod.Invoke(sourcePath, sourceFiles, resultsDirectory);
            }
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

    private static void RunKotlinScanners(string sourcePath, Dictionary<string, string> sourceFiles, string resultsDirectory)
    {
        new Scanners.Kotlin.CyclomaticComplexityScanner().AnalyzeFiles(sourceFiles, sourcePath, resultsDirectory);
    }

    private static void RunCommonScanners(string sourcePath, Dictionary<string, string> sourceFiles, string resultsDirectory)
    {
        LanguageLineCounter.AnalyzeLanguageBreakdown(sourceFiles, resultsDirectory);
        new TodoCommentScanner().AnalyzeFiles(sourceFiles, sourcePath, resultsDirectory);
    }
}
