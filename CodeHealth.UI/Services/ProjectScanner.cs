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
        DetectLanguagesAndRunScanners(sourcePath);
        
        Console.WriteLine($"Analysis complete in {stopwatch.Elapsed}!");
        return stopwatch.Elapsed;
    }

    private static void DetectLanguagesAndRunScanners(string sourcePath)
    {
        var sourceFiles = FileDiscoverer.GetSourceFiles(sourcePath);
        
        var resultsDirectory = RunInfo.CreateRun(sourcePath, DateTime.Now);

        if (sourceFiles.Keys.Any(f => f.EndsWith(".cs", StringComparison.OrdinalIgnoreCase)))
        {
            RunCSharpScanners(sourcePath, sourceFiles, resultsDirectory);
        }

        if (sourceFiles.Keys.Any(f => f.EndsWith(".java", StringComparison.OrdinalIgnoreCase)))
        {
            RunJavaScanners(sourcePath, resultsDirectory);
        }
    }

    private static void RunCSharpScanners(string sourcePath, Dictionary<string, string> sourceFiles, string resultsDirectory)
    {
        new CyclomaticComplexityScanner().AnalyzeFiles(sourceFiles, sourcePath, resultsDirectory);
        new TodoCommentScanner().AnalyzeFiles(sourceFiles, sourcePath, resultsDirectory);
    }

    private static void RunJavaScanners(string sourcePath, string resultsDirectory)
    {
        var process = new Process
        {
            StartInfo = new ProcessStartInfo
            {
                FileName = "java",
                Arguments = $"-jar CodeHealthJavaScanner.jar \"{sourcePath}\" \"{resultsDirectory}\"",
                RedirectStandardOutput = true,
                RedirectStandardError = true,
                UseShellExecute = false,
                CreateNoWindow = true
            }
        };

        process.OutputDataReceived += (sender, args) => Console.WriteLine(args.Data);
        process.ErrorDataReceived += (sender, args) => Console.Error.WriteLine(args.Data);

        process.Start();
        process.BeginOutputReadLine();
        process.BeginErrorReadLine();
        process.WaitForExit();
    }

}
