using System;

namespace CodeHealth.Scanners.CSharp;

/// <summary>
/// Runs a scan on (static) code.
/// </summary>
public interface IStaticCodeScanner
{
    void AnalyzeFiles(Dictionary<string, string> sourceFiles, string rootPath, string outputDir);
}
