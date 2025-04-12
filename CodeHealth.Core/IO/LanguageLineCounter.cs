namespace CodeHealth.Core.IO;

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.Json;

/// <summary>
/// Counts lines of code and spits out the percentages.
/// Ignores whitespace lines, comment lines, and just curly-braces lines.
/// </summary>
public static class LanguageLineCounter
{
    public static readonly Dictionary<string, string> ExtensionToLanguage = new(StringComparer.OrdinalIgnoreCase)
    {
        [".cs"] = "C#",
        [".java"] = "Java",
        [".kt"] = "Kotlin",
        [".js"] = "Javascript",
        [".jsx"] = "Javascript",
        [".ts"] = "Typescript",
        [".tsx"] = "Typescript",
    };

    public static void AnalyzeLanguageBreakdown(Dictionary<string, string> sourceFiles, string resultsDirectory)
    {
        var languageDistribution = GetLanguageBreakdown(sourceFiles);
        var json = JsonSerializer.Serialize(languageDistribution);
        File.WriteAllText(Path.Combine(resultsDirectory, Constants.FileNames.LanguageDistribution), json);
    }

    private static Dictionary<string, double> GetLanguageBreakdown(Dictionary<string, string> sourceFiles)
    {
        var lineCounts = new Dictionary<string, int>(StringComparer.OrdinalIgnoreCase);

        foreach (var file in sourceFiles)
        {
            var fileName = file.Key;
            var contents = file.Value; // Contents of the file passed in

            var ext = Path.GetExtension(fileName);
            var lang = ExtensionToLanguage.ContainsKey(ext) ? ExtensionToLanguage[ext] : "Other";

            // Count valid lines (ignoring whitespace and bracket-only lines)
            int validLines = contents
                .Split('\n')
                .Count(IsCodeLine);

            // Add valid lines to the respective language count
            lineCounts[lang] = lineCounts.GetValueOrDefault(lang, 0) + validLines;
        }

        int totalLines = lineCounts.Values.Sum();
        if (totalLines == 0)
        {
            return new(); // Avoid division by zero
        }

        // Return language percentages
        return lineCounts.ToDictionary(
            kvp => kvp.Key,
            kvp => Math.Round((double)kvp.Value / totalLines, 4));
    }

    private static bool IsCodeLine(string line)
    {
        var trimmed = line.Trim();
        return !(string.IsNullOrWhiteSpace(trimmed) || trimmed == "{" || trimmed == "}");
    }
}