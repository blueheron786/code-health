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

    private static void RunJavaScanners(string sourcePath, string outputDir)
    {
        try
        {
            var process = new Process
            {
                StartInfo = new ProcessStartInfo
                {
                    FileName = "java",
                    Arguments = $"-jar \"{jarPath}\" \"{sourcePath}\" \"{outputDir}\" {scannerName}",
                    RedirectStandardOutput = true,
                    RedirectStandardError = true,
                    UseShellExecute = false,
                    CreateNoWindow = true
                }
            };

            process.Start();
            string output = process.StandardOutput.ReadToEnd();
            string error = process.StandardError.ReadToEnd();
            process.WaitForExit();

            if (process.ExitCode != 0)
            {
                throw new InvalidOperationException($"Java scanner failed:\n{error}");
            }
        }
        catch (System.ComponentModel.Win32Exception win32Ex) when (win32Ex.Message.Contains("The system cannot find the file"))
        {
            throw new InvalidOperationException("Java is not installed or not on the system PATH. Please install Java and try again.");
        }
        catch (Exception ex)
        {
            throw new InvalidOperationException($"An error occurred while running the Java scanner: {ex.Message}", ex);
        }
    }

}
