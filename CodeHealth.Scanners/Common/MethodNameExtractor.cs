using System.Text.RegularExpressions;

namespace CodeHealth.Scanners.Common;

public static class MethodNameExtractor
{
    private const string CStyleMethodRegex = @"\b(public|private|protected)?\s*(static\s+)?[\w<>\[\]]+\s+(\w+)\s*\(.*\)\s*{?\s*$";
    private const string JsLikeFunctionRegex = @"\bfunction\b|\s*=>\s*{";

    public static MethodInfo? DetectMethodAtLine(string line, int lineNumber)
    {
        // Check for C-style method declaration
        var cStyleMatch = Regex.Match(line, CStyleMethodRegex);
        if (cStyleMatch.Success)
        {
            var methodName = ExtractCStyleMethodName(cStyleMatch);
            return new MethodInfo
            {
                Name = methodName,
                StartLine = lineNumber,
                IsDeclaration = true
            };
        }

        // Check for JavaScript-like function declaration
        var jsLikeMatch = Regex.Match(line, JsLikeFunctionRegex);
        if (jsLikeMatch.Success)
        {
            var methodName = ExtractJsLikeMethodName(line, lineNumber, jsLikeMatch);
            return new MethodInfo
            {
                Name = methodName,
                StartLine = lineNumber,
                IsDeclaration = true
            };
        }

        return null;
    }

    private static string ExtractCStyleMethodName(Match match)
    {
        if (match.Groups.Count >= 4 && match.Groups[3].Success)
        {
            var candidateName = match.Groups[3].Value;
            if (candidateName.Length < 4 || candidateName.Contains("\\"))
            {
                return "Unknown";
            }
            return candidateName;
        }
        return "Unknown";
    }

    private static string ExtractJsLikeMethodName(string line, int lineNumber, Match match)
    {
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

public class MethodInfo
{
    public string Name { get; set; } = "Unknown";
    public int StartLine { get; set; }
    public bool IsDeclaration { get; set; }
}