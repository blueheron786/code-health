using CodeHealth.Core.IO;

public static class FileDiscoverer
{
    /// <summary>
    /// Finds all source files in the given directory and its subdirectories.
    /// Excludes files that are ignored by gitignore and files that are in test directories.
    /// Returns a dictionary of file name => content, so we don't slam the file system with too many reads.
    /// </summary>
    public static Dictionary<string, string> GetSourceFiles(string rootPath)
    {
        var ignore = new GitIgnoreParser(Path.Combine(rootPath, ".gitignore"));

        var allFiles = Directory.GetFiles(rootPath, "*.cs", SearchOption.AllDirectories)
            .Where(path => !path.EndsWith(".Designer.cs", StringComparison.OrdinalIgnoreCase))
            .Where(path => !path.Contains($"{Path.DirectorySeparatorChar}test{Path.DirectorySeparatorChar}", StringComparison.OrdinalIgnoreCase)
                        && !path.Contains($"{Path.DirectorySeparatorChar}tests{Path.DirectorySeparatorChar}", StringComparison.OrdinalIgnoreCase))
            .Where(path => !ignore.IsIgnored(Path.GetRelativePath(rootPath, path)))
            .ToList();

        var pathToContent = allFiles.ToDictionary(path => path, File.ReadAllText);

        return pathToContent;
    }
}
