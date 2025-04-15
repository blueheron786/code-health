using System.Diagnostics;
using CodeHealth.Core.IO;
using CodeHealth.Scanners;
using CodeHealth.Scanners.Common.Scanners;

namespace CodeHealth.UI.Services;

public static class ProjectScanner
{
    private static Dictionary<string, IStaticCodeScanner> _languageSpecificScanners = new()
    {
        { ".cs", new CSharpCyclomaticComplexityScanner() },
        { ".java", new JavaCyclomaticComplexityScanner() },
        { ".kt", new KotlinCyclomaticComplexityScanner() },
        { ".js", new JavascriptTypescriptCyclomaticComplexityScanner() },
        { ".ts", new JavascriptTypescriptCyclomaticComplexityScanner() },
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
            var scanner = kvp.Value;

            if (sourceFiles.Any(f => f.Key.EndsWith(kvp.Key, StringComparison.OrdinalIgnoreCase)))
            {
                scanner.AnalyzeFiles(sourceFiles, sourcePath, resultsDirectory);
            }
        }

        RunCommonScanners(sourceFiles, sourcePath, resultsDirectory);
    }

    private static void RunCommonScanners(Dictionary<string, string> sourceFiles, string sourcePath, string resultsDirectory)
    {
        Parallel.Invoke(
            () => LanguageLineCounter.AnalyzeLanguageBreakdown(sourceFiles, resultsDirectory),
            () => new TodoCommentScanner().AnalyzeFiles(sourceFiles, sourcePath, resultsDirectory),
            () => new HeuristicLongMethodScanner().AnalyzeFiles(sourceFiles, sourcePath, resultsDirectory),
            () => new MagicNumberScanner().AnalyzeFiles(sourceFiles, sourcePath, resultsDirectory)
        );
    }
}
