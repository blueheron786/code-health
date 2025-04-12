namespace  CodeHealth.Core.Dtos.CyclomaticComplexity;

public class CyclomaticComplexityData
{
    public string File { get; set; }
    public string Method { get; set; }
    public int Complexity { get; set; }
    public string Language { get; set; } // e.g. csharp
}
