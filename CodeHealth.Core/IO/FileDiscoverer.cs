using CodeHealth.Core.IO;

public static class FileDiscoverer
{
    private static readonly string[] IgnoredFolders = new[] { "bin", "obj", ".git", ".vs" };

    /// <summary>
    /// Finds all source files in the given directory and its subdirectories.
    /// Excludes files that are ignored by gitignore and files that are in test directories.
    /// </summary>
    public static IEnumerable<string> DiscoverSourceFiles(string rootPath)
    {
        var ignore = new GitIgnoreParser(Path.Combine(rootPath, ".gitignore"));

        var cSharpFiles = Directory.GetFiles(rootPath, "*.cs", SearchOption.AllDirectories)
            .Where(path =>
                // C# code, but not generated, e.g. .Designer.cs files from RESX files
                !path.EndsWith(".Designer.cs", StringComparison.OrdinalIgnoreCase)
                && !path.EndsWith("Tests", StringComparison.OrdinalIgnoreCase));
        
        var javaFiles = Directory.GetFiles(rootPath, "*.java", SearchOption.AllDirectories)
            .Where(path =>
                !path.Contains($"{Path.DirectorySeparatorChar}test{Path.DirectorySeparatorChar}", StringComparison.OrdinalIgnoreCase));

        // Common/global exclusions
        var allFiles = cSharpFiles.Concat(javaFiles)
            .Where(path =>
                // Ignore test folders/files
                !path.Contains($"{Path.DirectorySeparatorChar}test{Path.DirectorySeparatorChar}", StringComparison.OrdinalIgnoreCase)
                && !path.Contains($"{Path.DirectorySeparatorChar}tests{Path.DirectorySeparatorChar}", StringComparison.OrdinalIgnoreCase)
                // Ignore stuff that SHOULD be in .gitignore but might not be, or might be incorrectly listed
                // e.g. bin instead of bin/
                && !IgnoredFolders.Any(folder =>
                    path.Split(Path.DirectorySeparatorChar).Contains(folder, StringComparer.OrdinalIgnoreCase))
                // Flaky/broken .gitignore parsing/excluding
                && !ignore.IsIgnored(Path.GetRelativePath(rootPath, path)));
        
        return allFiles;
    }
}
