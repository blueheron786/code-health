using System.Text.RegularExpressions;

namespace CodeHealth.Scanners.Common;

public static class MethodNameExtractor
{
    public static string ExtractMethodName(string line, int lineNumber, Match match)
    {
        // C-style methods
        if (match.Groups.Count >= 4 && match.Groups[3].Success)
        {
            var candidateName = match.Groups[3].Value;
            // If it's too short, or a regex or something, then it's probably not a method name
            // Also, if it contains a backslash, it's likely a namespace or something else
            if (candidateName.Length < 4 || candidateName.Contains("\\"))
            {
                return "Unknown"; // Too short to be meaningful, might be a mistake, e.g. ")
            }

            return candidateName;
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