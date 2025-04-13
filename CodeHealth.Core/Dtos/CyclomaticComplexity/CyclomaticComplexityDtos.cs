namespace CodeHealth.Core.Dtos.CyclomaticComplexity
{
    public class Metric
    {
        public string Name { get; set; } = "";
        public int Value { get; set; }
        public int Threshold { get; set; }
    }

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
        public int TotalComplexity { get; set; }
        public double AverageComplexity { get; set; }
    }
}
