using System.Diagnostics;
using System.IO;
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
            RunJavaScanners(sourcePath, resultsDirectory);
        }
    }

    private static void RunCSharpScanners(string sourcePath, Dictionary<string, string> sourceFiles, string resultsDirectory)
    {
        new CyclomaticComplexityScanner().AnalyzeFiles(sourceFiles, sourcePath, resultsDirectory);
        new TodoCommentScanner().AnalyzeFiles(sourceFiles, sourcePath, resultsDirectory);
    }

    private static bool IsJavaInstalled()
    {
        var applicationDirectory = Directory.GetCurrentDirectory();

        try
        {
            using (var process = new Process())
            {
                process.StartInfo = new ProcessStartInfo
                {
                    FileName = "java",
                    Arguments = "-version",
                    RedirectStandardError = true,
                    UseShellExecute = false,
                    CreateNoWindow = true,
                    WorkingDirectory = applicationDirectory,
                };

                process.Start();
                process.WaitForExit(5000);
                return process.ExitCode == 0;
            }
        }
        catch
        {
            return false;
        }
    }


    private static void RunJavaScanners(string sourcePath, string outputDir)
    {
        try
        {
            var applicationDirectory = Directory.GetCurrentDirectory();

            // Verify Java is installed and in PATH
            if (!IsJavaInstalled())
            {
                throw new InvalidOperationException("Java is not installed or not in system PATH");
            }

            // Ensure bin directory exists
            var binDirectory = Path.Combine(applicationDirectory, "bin");
            if (!Directory.Exists(binDirectory))
            {
                Directory.CreateDirectory(binDirectory);
            }

            var javaProcess = new Process
            {
                StartInfo = new ProcessStartInfo
                {
                    FileName = "java",
                    Arguments = $"-jar CodeHealth.Scanners.Java-all.jar \"{sourcePath}\" \"{outputDir}\"",
                    UseShellExecute = false,
                    RedirectStandardOutput = true,
                    RedirectStandardError = true,
                    CreateNoWindow = true,
                    WorkingDirectory = Path.Combine(applicationDirectory, "..", "CodeHealth.Scanners.Java", "binaries"),
                }
            };

            javaProcess.Start();
            string output = javaProcess.StandardOutput.ReadToEnd();
            string error = javaProcess.StandardError.ReadToEnd();
            javaProcess.WaitForExit();

            if (javaProcess.ExitCode != 0)
            {
                throw new InvalidOperationException($"Java scanner failed: {error}");
            }

            Console.WriteLine(output);

        }
        catch (Exception ex)
        {
            throw new InvalidOperationException($"An error occurred while running the Java scanners: {ex.Message}", ex);
        }
    }
}
