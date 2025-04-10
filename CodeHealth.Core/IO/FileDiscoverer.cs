using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using CodeHealth.Core.IO;

public static class FileDiscoverer
{
    public static (List<string> Files, string OutputDir) GetCSharpFiles(string rootPath)
    {
        var ignore = new GitIgnoreParser(Path.Combine(rootPath, ".gitignore"));

        var allFiles = Directory.GetFiles(rootPath, "*.cs", SearchOption.AllDirectories)
            .Where(path => !path.EndsWith(".Designer.cs", StringComparison.OrdinalIgnoreCase))
            .Where(path => !path.Contains($"{Path.DirectorySeparatorChar}test{Path.DirectorySeparatorChar}", StringComparison.OrdinalIgnoreCase)
                        && !path.Contains($"{Path.DirectorySeparatorChar}tests{Path.DirectorySeparatorChar}", StringComparison.OrdinalIgnoreCase))
            .Where(path => !ignore.IsIgnored(Path.GetRelativePath(rootPath, path)))
            .ToList();

        var outputDir = Path.Combine("runs", DateTime.Now.ToString("yyyy-MM-dd--HH-mm"));
        Directory.CreateDirectory(outputDir);

        return (allFiles, outputDir);
    }
}
