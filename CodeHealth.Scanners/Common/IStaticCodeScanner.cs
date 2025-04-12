namespace CodeHealth.Scanners.Common;

/// <summary>
/// Runs a scan on (static) code.
/// </summary>
public interface IStaticCodeScanner
{
    void AnalyzeFiles(Dictionary<string, string> sourceFiles, string rootPath, string outputDir);
}
