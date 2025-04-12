using System.Text.Json;
using CodeHealth.Core.Dtos.CyclomaticComplexity;
using CodeHealth.Core.IO;

namespace CodeHealth.Scanners.Common;

public static class CyclomaticComplexityReporter
{
    public static void FinalizeReport(Report report, string outputDir, string outputFilename)
    {
        int methodCount = report.Files.Sum(f => f.Methods.Count);
        report.AverageComplexity = methodCount > 0 ? (double)report.TotalComplexity / methodCount : 0;

        var outputFile = Path.Combine(outputDir, outputFilename);
        var json = JsonSerializer.Serialize(report, new JsonSerializerOptions { WriteIndented = true });
        File.WriteAllText(outputFile, json);
    }
}
