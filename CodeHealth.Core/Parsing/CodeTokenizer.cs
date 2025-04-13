using CodeHealth.Core.Dtos;

namespace CodeHealth.Core.Parsing;

public record CodeToken(string Text, int StartColumn, int EndColumn, List<IssueResult> Issues);

public static class CodeTokenizer
{
    public static List<CodeToken> Tokenize(string line, int lineNumber, List<IssueResult> issues)
    {
        var tokens = new List<CodeToken>();
        int currentIndex = 0;

        foreach (var rawToken in line.Split(' '))
        {
            var trimmed = rawToken.Trim();
            if (string.IsNullOrWhiteSpace(trimmed))
            {
                currentIndex += rawToken.Length + 1;
                continue;
            }

            int start = currentIndex;
            int end = start + trimmed.Length;

            var matching = issues
                .Where(i => i.Line == lineNumber &&
                            i.Column.HasValue &&
                            i.EndColumn.HasValue &&
                            i.Column.Value <= start &&
                            i.EndColumn.Value >= end)
                .ToList();


            tokens.Add(new CodeToken(trimmed, start, end, matching));

            currentIndex = end + 1;
        }

        return tokens;
    }
}
