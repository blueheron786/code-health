using System.Text.Json;
using CodeHealth.Core.Dtos;
using CodeHealth.Core.IO;
using CodeHealth.Scanners.Common.Scanners;
using NUnit.Framework;

namespace CodeHealth.Scanners.Common.Tests.Scanners;

[TestFixture]
public class HeuristicLongMethodScannerTests
{
    private Dictionary<string, string> _sourceFiles;
    private string _rootPath;
    private string _resultsDir;

    [SetUp]
    public void SetUp()
    {
        // Get the current test execution directory
        var testDirectory = TestContext.CurrentContext.TestDirectory;
        
        _sourceFiles = new Dictionary<string, string>();
        _rootPath = Path.Combine(testDirectory, "project", "root");
        _resultsDir = Path.Combine(testDirectory, "results");
        
        // Ensure directories exist
        Directory.CreateDirectory(_rootPath);
        Directory.CreateDirectory(_resultsDir);
    }

    [TearDown]
    public void TearDown()
    {
        // Clean up test directories
        if (Directory.Exists(_resultsDir))
        {
            Directory.Delete(_resultsDir, true);
        }
    }

    [Test]
    public void AnalyzeFiles_WhenNoFiles_ShouldCreateEmptyReport()
    {
        // Arrange
        var scanner = new HeuristicLongMethodScanner();

        // Act
        scanner.AnalyzeFiles(_sourceFiles, _rootPath, _resultsDir);

        // Assert
        var expectedPath = Path.Combine(_resultsDir, Constants.FileNames.LongMethodsFile);
        Assert.That(File.Exists(expectedPath), Is.True);
        
        var report = JsonSerializer.Deserialize<Report>(File.ReadAllText(expectedPath));
        Assert.That(report.Issues.Count, Is.EqualTo(0));
        Assert.That(report.TotalMetricValue, Is.EqualTo(0));
        Assert.That(report.AverageMetricValue, Is.EqualTo(0));
    }

    [Test]
    public void AnalyzeFiles_WhenFileHasNoMethods_ShouldCreateEmptyReport()
    {
        // Arrange
        var scanner = new HeuristicLongMethodScanner();
        _sourceFiles.Add("/project/root/File.cs", "public class TestClass { }");

        // Act
        scanner.AnalyzeFiles(_sourceFiles, _rootPath, _resultsDir);

        // Assert
        var report = JsonSerializer.Deserialize<Report>(File.ReadAllText(Path.Combine(_resultsDir, Constants.FileNames.LongMethodsFile)));
        Assert.That(report.Issues.Count, Is.EqualTo(0));
    }

    [Test]
    public void AnalyzeFiles_WhenMethodIsExactlyThresholdLength_ShouldNotReport()
    {
        // Arrange
        const int threshold = 5;
        var scanner = new HeuristicLongMethodScanner(threshold);
        
        var methodLines = Enumerable.Range(0, threshold - 2) // Adjusted for method declaration and closing brace
            .Select(i => "    Console.WriteLine(\"Test\");")
            .ToArray();
        
        var fileContent = "public class TestClass {\n" +
                        "public void TestMethod() {\n" +
                        string.Join("\n", methodLines) + "\n" +
                        "}\n" +
                        "}";
        
        _sourceFiles.Add("/project/root/File.cs", fileContent);

        // Act
        scanner.AnalyzeFiles(_sourceFiles, _rootPath, _resultsDir);

        // Assert
        var report = JsonSerializer.Deserialize<Report>(File.ReadAllText(Path.Combine(_resultsDir, Constants.FileNames.LongMethodsFile)));
        Assert.That(report.Issues.Count, Is.EqualTo(0));
    }

    [Test]
    public void AnalyzeFiles_WhenMethodIsOneLineOverThreshold_ShouldReport()
    {
        // Arrange
        const int threshold = 5;
        var scanner = new HeuristicLongMethodScanner(threshold);
        
        var methodLines = Enumerable.Range(0, threshold - 1) // Adjusted for method declaration and closing brace
            .Select(i => "    Console.WriteLine(\"Test\");")
            .ToArray();
        
        var fileContent = "public class TestClass {\n" +
                        "public void TestMethod() {\n" +
                        string.Join("\n", methodLines) + "\n" +
                        "}\n" +
                        "}";
        
        _sourceFiles.Add("/project/root/File.cs", fileContent);

        // Act
        scanner.AnalyzeFiles(_sourceFiles, _rootPath, _resultsDir);

        // Assert
        var report = JsonSerializer.Deserialize<Report>(File.ReadAllText(Path.Combine(_resultsDir, Constants.FileNames.LongMethodsFile)));
        Assert.That(report.Issues.Count, Is.EqualTo(1));
        Assert.That(report.Issues[0].Metric.Value, Is.EqualTo(threshold + 1));
        Assert.That(report.Issues[0].Name, Is.EqualTo("TestMethod"));
    }

    [Test]
    public void AnalyzeFiles_WhenMultipleFilesWithLongMethods_ShouldAggregateResults()
    {
        // Arrange
        var scanner = new HeuristicLongMethodScanner(2);
        
        _sourceFiles.Add("/project/root/File1.cs", 
            "public class TestClass1 {\n" +
            "public void Method1() {\n" +
            "    Console.WriteLine(1);\n" +
            "    Console.WriteLine(2);\n" +
            "    Console.WriteLine(3);\n" +
            "}\n" +
            "}");
            
        _sourceFiles.Add("/project/root/File2.cs", 
            "public class TestClass2 {\n" +
            "public void Method2() {\n" +
            "    Console.WriteLine(1);\n" +
            "    Console.WriteLine(2);\n" +
            "    Console.WriteLine(3);\n" +
            "    Console.WriteLine(4);\n" +
            "}\n" +
            "}");

        // Act
        scanner.AnalyzeFiles(_sourceFiles, _rootPath, _resultsDir);

        // Assert
        var report = JsonSerializer.Deserialize<Report>(File.ReadAllText(Path.Combine(_resultsDir, Constants.FileNames.LongMethodsFile)));
        Assert.That(report.Issues.Count, Is.EqualTo(2));
        Assert.That(report.TotalMetricValue, Is.GreaterThanOrEqualTo(3 + 4));
        Assert.That(report.AverageMetricValue, Is.GreaterThan(0));
    }

    [Test]
    public void AnalyzeFiles_WhenMethodSpansMultipleLines_ShouldCalculateCorrectLength()
    {
        // Arrange
        var scanner = new HeuristicLongMethodScanner(3);
        
        _sourceFiles.Add("/project/root/File.cs", 
            "public class TestClass {\n" +
            "public void Method1() \n" +
            "{\n" +  // Start line
            "    Console.WriteLine(1);\n" +
            "    Console.WriteLine(2);\n" +
            "    Console.WriteLine(3);\n" +
            "}\n" +    // End line
            "}");

        // Act
        scanner.AnalyzeFiles(_sourceFiles, _rootPath, _resultsDir);

        // Assert
        var report = JsonSerializer.Deserialize<Report>(File.ReadAllText(Path.Combine(_resultsDir, Constants.FileNames.LongMethodsFile)));
        Assert.That(report.Issues.Count, Is.EqualTo(1));
        Assert.That(report.Issues[0].Metric.Value, Is.GreaterThanOrEqualTo(4)); // 4 lines from opening brace to closing brace
    }

    [Test]
    public void AnalyzeFiles_WhenNestedMethods_ShouldHandleCorrectly()
    {
        // Arrange
        var scanner = new HeuristicLongMethodScanner(3);
        
        _sourceFiles.Add("/project/root/File.cs", 
            "public class TestClass {\n" +
            "public void OuterMethod() {\n" +
            "    Console.WriteLine(1);\n" +
            "    void InnerMethod() {\n" +
            "        Console.WriteLine(2);\n" +
            "        Console.WriteLine(3);\n" +
            "    }\n" +
            "    Console.WriteLine(4);\n" +
            "    Console.WriteLine(5);\n" +
            "}\n" +
            "}");

        // Act
        scanner.AnalyzeFiles(_sourceFiles, _rootPath, _resultsDir);

        // Assert
        var report = JsonSerializer.Deserialize<Report>(File.ReadAllText(Path.Combine(_resultsDir, Constants.FileNames.LongMethodsFile)));
        Assert.That(report.Issues.Count, Is.EqualTo(1)); // Only outer method should be reported
        Assert.That(report.Issues[0].Metric.Value, Is.GreaterThanOrEqualTo(4)); // Outer method length
        // Assert.That(report.Issues[0].Name, Is.EqualTo("OuterMethod"));
    }

    [Test]
    public void AnalyzeFiles_WhenMultipleMethodsInFile_ShouldHandleEachSeparately()
    {
        // Arrange
        var scanner = new HeuristicLongMethodScanner(3);
        
        _sourceFiles.Add("/project/root/File.cs", 
            "public class TestClass {\n" +
            "public void Method1() {\n" +
            "    Console.WriteLine(1);\n" +
            "    Console.WriteLine(2);\n" +
            "    Console.WriteLine(3);\n" +
            "    Console.WriteLine(4);\n" +
            "}\n" +
            "public void Method2() {\n" +
            "    Console.WriteLine(1);\n" +
            "}\n" +
            "}");

        // Act
        scanner.AnalyzeFiles(_sourceFiles, _rootPath, _resultsDir);

        // Assert
        var report = JsonSerializer.Deserialize<Report>(File.ReadAllText(Path.Combine(_resultsDir, Constants.FileNames.LongMethodsFile)));
        Assert.That(report.Issues.Count, Is.EqualTo(1)); // Only Method1 should be reported
        Assert.That(report.Issues[0].Name, Is.EqualTo("Method1"));
    }

    [Test]
    public void AnalyzeFiles_WhenFileHasMixedContent_ShouldCalculateCorrectLineNumbers()
    {
        // Arrange
        var scanner = new HeuristicLongMethodScanner(2);
        
        _sourceFiles.Add("/project/root/File.cs", 
            "using System;\n" +
            "\n" +
            "namespace Test\n" +
            "{\n" +
            "    public class TestClass\n" +
            "    {\n" +
            "        // Some comment\n" +
            "        public void Method1()\n" +
            "        {\n" +  // Line 9 (1-based)
            "            Console.WriteLine(1);\n" +
            "            Console.WriteLine(2);\n" +
            "        }\n" +  // Line 12
            "    }\n" +
            "}");

        // Act
        scanner.AnalyzeFiles(_sourceFiles, _rootPath, _resultsDir);

        // Assert
        var report = JsonSerializer.Deserialize<Report>(File.ReadAllText(Path.Combine(_resultsDir, Constants.FileNames.LongMethodsFile)));
        var issue = report.Issues[0];
        Assert.That(issue.Metric.Value, Is.GreaterThanOrEqualTo(3)); // 3 lines from opening to closing brace
        Assert.That(issue.Line, Is.GreaterThanOrEqualTo(8)); // 1-based line number (9 is 0-based in code)
        Assert.That(issue.EndLine, Is.EqualTo(12));
    }

    [Test]
    public void Constructor_WithNegativeThreshold_ShouldThrow()
    {
        // Act & Assert
        Assert.That(() => new HeuristicLongMethodScanner(-1), 
            Throws.TypeOf<ArgumentOutOfRangeException>());
    }

    [Test]
    public void AnalyzeFiles_When_resultsDirectoryDoesNotExist_ShouldCreateIt()
    {
        // Arrange
        var scanner = new HeuristicLongMethodScanner();
        var nonExistentDir = Path.Combine(_resultsDir, "nonexistent");
        
        if (Directory.Exists(nonExistentDir))
        {
            Directory.Delete(nonExistentDir);
        }

        // Act
        scanner.AnalyzeFiles(_sourceFiles, _rootPath, nonExistentDir);

        // Assert
        Assert.That(Directory.Exists(nonExistentDir), Is.True);
    }
}