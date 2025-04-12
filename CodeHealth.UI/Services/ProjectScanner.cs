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

        var sourceFiles = FileDiscoverer.GetSourceFiles(sourcePath!);
        var resultsDirectory = RunInfo.CreateRun(sourcePath, DateTime.Now);

        Console.WriteLine($"Analyzing {sourcePath} ...");

        new CyclomaticComplexityScanner().AnalyzeFiles(sourceFiles, sourcePath, resultsDirectory);
        new TodoCommentScanner().AnalyzeFiles(sourceFiles, sourcePath, resultsDirectory);

        Console.WriteLine($"Analysis complete in {stopwatch.Elapsed}!");
        return stopwatch.Elapsed;
    }
}
