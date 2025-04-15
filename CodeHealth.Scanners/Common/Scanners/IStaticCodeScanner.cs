namespace CodeHealth.Scanners.Common.Scanners;

/// <summary>
/// Runs a scan on (static) code.
/// </summary>
public interface IStaticCodeScanner
{
    void AnalyzeFiles(Dictionary<string, string> sourceFiles, string rootPath, string outputDir);
}
