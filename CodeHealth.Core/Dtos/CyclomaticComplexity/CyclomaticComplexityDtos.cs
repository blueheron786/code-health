namespace CodeHealth.Core.Dtos.CyclomaticComplexity;

public class MethodResult
{
    public string Method { get; set; } = "";
    public int Complexity { get; set; }
    public int StartLine { get; set; }
    public int EndLine { get; set; }
}

public class FileResult
{
    public string File { get; set; } = "";
    public List<MethodResult> Methods { get; set; } = new();
}

public class Report
{
    public List<FileResult> Files { get; set; } = new();
    public int TotalComplexity { get; set; }
    public double AverageComplexity { get; set; }
}