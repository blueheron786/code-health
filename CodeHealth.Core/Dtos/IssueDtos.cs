namespace CodeHealth.Core.Dtos;

public class Metric
{
    public string Name { get; set; } = "";
    public int Value { get; set; }
    public int Threshold { get; set; }
}

/// <summary>
/// Commonly used for pretty much every type of issue. Except maybe cyclomatic complexity.
/// </summary>
public class IssueResult
{
    public string Scanner { get; set; } = "CyclomaticComplexity";
    public string Type { get; set; } = "Method";
    public string File { get; set; } = "";

    // Line-level or multi-line
    public int Line { get; set; }
    public int EndLine { get; set; }

    // A single word/token
    public int? Column { get; set; }
    public int? EndColumn { get; set; }

    public string Name { get; set; } = "";
    public Metric Metric { get; set; } = new Metric();
    public string Message { get; set; } = "";
    public string Severity { get; set; } = "High"; // "Low", "Medium", "High"
    public string Suggestion { get; set; } = "Consider refactoring the method to improve readability.";
    public List<string> Tags { get; set; } = new List<string> { "complexity", "refactor" };
    public bool Fixable { get; set; } = true; // Or false if not easily fixable
}

public class Report
{
    public List<IssueResult> Issues { get; set; } = new List<IssueResult>();
    // Total and average metric values. e.g. for long methods, these represent
    // the total number of lines across all long methods, and the average
    // numver of lines within those long methods. Noice.
    // And yeah, for CC, this is the total and average CC.
    public int TotalMetricValue { get; set; }
    public double AverageMetricValue { get; set; }
}
