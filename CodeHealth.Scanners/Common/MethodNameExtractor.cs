using System.Text.RegularExpressions;

namespace CodeHealth.Scanners.Common;

public static class MethodNameExtractor
{
    public static string ExtractMethodName(string line, int lineNumber, Match match)
    {
        // C-style methods
        if (match.Groups.Count >= 4 && match.Groups[3].Success)
        {
            return match.Groups[3].Value;
        }

        if (match.Value.Contains("=>"))
        {
            return $"anonymous (line {lineNumber + 1})";
        }

        if (match.Value.Contains("function"))
        {
            var afterFunction = line.Substring(line.IndexOf("function") + 8).Trim();
            return afterFunction.Split(new[] { '(', ' ' }, StringSplitOptions.RemoveEmptyEntries).FirstOrDefault()
                ?? $"anonymous (line {lineNumber + 1})";
        }
        

        return "Unknown";
    }
}