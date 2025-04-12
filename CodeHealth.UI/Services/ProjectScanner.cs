using System.Diagnostics;
using System.IO;
using System.Text;
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

    private static bool IsJavaInstalled()
    {
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
                    CreateNoWindow = true
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

            CompileJavaScannerCode();
            
            // Step 2: Run Java application
            using (var runProcess = new Process())
            {
                runProcess.StartInfo = new ProcessStartInfo
                {
                    FileName = "java",
                    Arguments = $"-cp bin/ com.codehealth.scanners.java.Application \"{sourcePath}\" \"{outputDir}\"",
                    RedirectStandardOutput = true,
                    RedirectStandardError = true,
                    UseShellExecute = false,
                    CreateNoWindow = true,
                    WorkingDirectory = applicationDirectory
                };

                Console.WriteLine($"Running Java analyzers...");
                
                var outputBuilder = new StringBuilder();
                var errorBuilder = new StringBuilder();

                runProcess.OutputDataReceived += (sender, args) => outputBuilder.AppendLine(args.Data);
                runProcess.ErrorDataReceived += (sender, args) => errorBuilder.AppendLine(args.Data);

                runProcess.Start();
                
                // Begin asynchronous reading
                runProcess.BeginOutputReadLine();
                runProcess.BeginErrorReadLine();
                
                // Wait with timeout (e.g., 1 minute)
                if (!runProcess.WaitForExit(1 * 60 * 1000))
                {
                    runProcess.Kill();
                    throw new InvalidOperationException("Java execution timed out");
                }

                string runOutput = outputBuilder.ToString();
                string runError = errorBuilder.ToString();

                Console.WriteLine(runOutput);
                
                if (runProcess.ExitCode != 0)
                {
                    Console.WriteLine(runError);
                    throw new InvalidOperationException($"Java application failed:\n{runError}");
                }
            }
        }
        catch (Exception ex)
        {
            throw new InvalidOperationException($"An error occurred while running the Java scanners: {ex.Message}", ex);
        }
    }

    private static void CompileJavaScannerCode()
    {
        var applicationDirectory = Directory.GetCurrentDirectory();

        var javaFiles = Directory.GetFiles(
            Path.Combine(applicationDirectory, "..", "CodeHealth.Scanners.Java", "src", "main", "com", "codehealth", "scanners", "java"), 
            "*.java", 
            SearchOption.AllDirectories);

        if (javaFiles.Length == 0)
        {
            throw new InvalidOperationException("Could not find Java analyzers files to compile!");
        }

        var compileArguments = string.Join(" ", javaFiles.Select(f => $"\"{f}\""));

        using (var compileProcess = new Process())
        {
            compileProcess.StartInfo = new ProcessStartInfo
            {
                FileName = "javac",
                Arguments = $"-d bin/ {compileArguments}",
                RedirectStandardOutput = true,
                RedirectStandardError = true,
                UseShellExecute = false,
                CreateNoWindow = true,
                WorkingDirectory = applicationDirectory
            };

            Console.WriteLine($"Compiling Java analyzers...");
            
            var outputBuilder = new StringBuilder();
            var errorBuilder = new StringBuilder();

            compileProcess.OutputDataReceived += (sender, args) => outputBuilder.AppendLine(args.Data);
            compileProcess.ErrorDataReceived += (sender, args) => errorBuilder.AppendLine(args.Data);

            compileProcess.Start();
            
            // Begin asynchronous reading
            compileProcess.BeginOutputReadLine();
            compileProcess.BeginErrorReadLine();
            
            // Wait with timeout (e.g., 2 minutes)
            if (!compileProcess.WaitForExit(120000))
            {
                compileProcess.Kill();
                throw new InvalidOperationException("Java compilation timed out");
            }

            string compileOutput = outputBuilder.ToString();
            string compileError = errorBuilder.ToString();

            if (compileProcess.ExitCode != 0)
            {
                Console.WriteLine(compileOutput);
                Console.WriteLine(compileError);
                throw new InvalidOperationException($"Java compilation failed:\n{compileError}");
            }
        }
    }
}
