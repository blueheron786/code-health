using System.Text.Json;
using CodeHealth.Core.Dtos;

namespace CodeHealth.Scanners.Common
{
    public static class CyclomaticComplexityReporter
    {
        public static void FinalizeReport(Report report, string outputDir, string outputFilename)
        {
            // Sum complexity from all issues
            int methodCount = report.Issues.Count;
            int totalComplexity = report.Issues.Sum(issue => issue.Metric.Value);
            
            // Set average complexity if methodCount > 0
            report.TotalComplexity = totalComplexity;
            report.AverageComplexity = methodCount > 0 ? (double)totalComplexity / methodCount : 0;

            // Create the output file
            var outputFile = Path.Combine(outputDir, outputFilename);
            var json = JsonSerializer.Serialize(report, new JsonSerializerOptions { WriteIndented = true });
            File.WriteAllText(outputFile, json);
        }
    }
}
