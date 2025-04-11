using System;
using System.Diagnostics;
using CodeHealth.Core.IO;
using CodeHealth.Scanners.CSharp.Scanners;

namespace CodeHealth.UI.Services;

public static class ProjectScanner
{
    public static TimeSpan Scan(string sourcePath)
    {
        var stopwatch = new Stopwatch();
        stopwatch.Start();

        var mySourceFiles = FileDiscoverer.GetSourceFiles(sourcePath!);
        var resultsDirectory = RunInfo.CreateRun(sourcePath, DateTime.Now);

        Console.WriteLine($"Analyzing {sourcePath} ...");

        CyclomaticComplexityScanner.AnalyzeFiles(mySourceFiles, sourcePath, resultsDirectory);

        Console.WriteLine($"Analysis complete in {stopwatch.Elapsed}!");
        return stopwatch.Elapsed;
    }
}
