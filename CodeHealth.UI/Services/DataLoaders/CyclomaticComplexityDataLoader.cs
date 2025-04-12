using System.IO;
using System.Text.Json;
using CodeHealth.Core.Dtos;
using CodeHealth.Core.Dtos.CyclomaticComplexity;
using CodeHealth.Core.IO;

namespace CodeHealth.UI.Services.DataLoaders;

public static class CyclomaticComplexityDataLoader
{
    public static async Task<List<CyclomaticComplexityData>> LoadCyclomaticComplexityData(string runDirectoryPath)
    {
        var pattern = "cyclomatic_complexity.*.json";
        var files = Directory.GetFiles(runDirectoryPath, pattern);

        var allMethods = new List<CyclomaticComplexityData>();

        foreach (var filePath in files)
        {
            try
            {
                var jsonData = await File.ReadAllTextAsync(filePath);
                var report = JsonSerializer.Deserialize<Report>(jsonData);

                if (report?.Files != null)
                {
                    var language = GetLanguageFromFile(filePath);

                    var methods = report.Files
                        .SelectMany(file => file.Methods.Select(method => new CyclomaticComplexityData
                        {
                            File = file.File,
                            Method = method.Method,
                            Complexity = method.Complexity,
                            Language = language
                        }));

                    allMethods.AddRange(methods);
                }
            }
            catch
            {
                // Optional: log or skip unreadable/broken files
            }
        }

        return allMethods
            .OrderByDescending(m => m.Complexity)
            .ToList();
    }

    private static string GetLanguageFromFile(string filePath)
    {
        // Extract the language from the file name
        var fileName = Path.GetFileName(filePath);
        var language = fileName?.Split('.').Skip(2).FirstOrDefault()?.ToUpper();

        // Handle cases where language isn't found or is malformed
        return language ?? "Unknown";
    }
}
