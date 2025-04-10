using CodeHealth.Core.IO;

public static class FileDiscoverer
{
    public static List<string> GetSourceFiles(string rootPath)
    {
        var ignore = new GitIgnoreParser(Path.Combine(rootPath, ".gitignore"));

        var allFiles = Directory.GetFiles(rootPath, "*.cs", SearchOption.AllDirectories)
            .Where(path => !path.EndsWith(".Designer.cs", StringComparison.OrdinalIgnoreCase))
            .Where(path => !path.Contains($"{Path.DirectorySeparatorChar}test{Path.DirectorySeparatorChar}", StringComparison.OrdinalIgnoreCase)
                        && !path.Contains($"{Path.DirectorySeparatorChar}tests{Path.DirectorySeparatorChar}", StringComparison.OrdinalIgnoreCase))
            .Where(path => !ignore.IsIgnored(Path.GetRelativePath(rootPath, path)))
            .ToList();

        return allFiles;
    }
}
