using System.IO;
using System.Text.Json;
using CodeHealth.Core.Dtos;
using CodeHealth.Core.Dtos.CyclomaticComplexity;
using CodeHealth.Core.IO;

namespace CodeHealth.UI.Services.DataLoaders;

public static class CyclomaticComplexityDataLoder
{
    public static async Task<List<CyclomaticComplexityData>> LoadCyclomaticComplexityData(string runDirectoryPath)
    {
        var filePath = Path.Combine(runDirectoryPath, Constants.FileNames.CyclomatiComplexityFile);

        if (!File.Exists(filePath))
        {
            return new List<CyclomaticComplexityData>();
        }

        var jsonData = await File.ReadAllTextAsync(filePath);
        var report = JsonSerializer.Deserialize<Report>(jsonData);

        var methods = report.Files
            .SelectMany(file => file.Methods.Select(method => new CyclomaticComplexityData
            {
                File = file.File,
                Method = method.Method,
                Complexity = method.Complexity
            }))
            .OrderByDescending(m => m.Complexity)
            .ToList();

        return methods;
    }
}
