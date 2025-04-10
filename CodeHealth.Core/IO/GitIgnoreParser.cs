using System;

namespace CodeHealth.Core.IO;

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;

// TODO: find a library that does this instead of roll-yer-own
public class GitIgnoreParser
{
    private readonly List<Regex> _rules = new();

    public GitIgnoreParser(string gitignorePath)
    {
        if (!File.Exists(gitignorePath)) return;

        foreach (var line in File.ReadLines(gitignorePath))
        {
            var trimmed = line.Trim();
            if (string.IsNullOrEmpty(trimmed) || trimmed.StartsWith("#")) continue;

            var pattern = "^" + Regex.Escape(trimmed)
                .Replace(@"\*", ".*")
                .Replace(@"\?", ".") + "$";

            _rules.Add(new Regex(pattern, RegexOptions.IgnoreCase));
        }
    }

    public bool IsIgnored(string relativePath)
    {
        return _rules.Any(rule => rule.IsMatch(relativePath.Replace("\\", "/")));
    }
}

