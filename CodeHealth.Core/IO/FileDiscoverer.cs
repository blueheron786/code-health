using CodeHealth.Core.IO;

public static class FileDiscoverer
{
    private static readonly string[] AllowedExtensions = new[] { ".cs", ".java" }; // maybe .kt, .js later?
    // TODO: replace with proper .gitignore parsing
    private static readonly string[] IgnoredFolders = new[] { "bin", "obj", ".git", ".vs" };

    /// <summary>
    /// Finds all source files in the given directory and its subdirectories.
    /// Excludes files that are ignored by gitignore and files that are in test directories.
    /// Returns a dictionary of file name => content, so we don't slam the file system with too many reads.
    /// </summary>
    public static Dictionary<string, string> GetSourceFiles(string rootPath)
    {
        var ignore = new GitIgnoreParser(Path.Combine(rootPath, ".gitignore"));

        var allFiles = Directory.GetFiles(rootPath, "*.*", SearchOption.AllDirectories)
            .Where(path =>
                AllowedExtensions.Contains(Path.GetExtension(path), StringComparer.OrdinalIgnoreCase)
                // C# code, but not generated, e.g. .Designer.cs files from RESX files
                && !path.EndsWith(".Designer.cs", StringComparison.OrdinalIgnoreCase)
                // Ignore test folders/files
                && !path.Contains($"{Path.DirectorySeparatorChar}test{Path.DirectorySeparatorChar}", StringComparison.OrdinalIgnoreCase)
                && !path.Contains($"{Path.DirectorySeparatorChar}tests{Path.DirectorySeparatorChar}", StringComparison.OrdinalIgnoreCase)
                // Ignore stuff that SHOULD be in .gitignore but might not be ...
                && !IgnoredFolders.Any(folder =>
                    path.Split(Path.DirectorySeparatorChar).Contains(folder, StringComparer.OrdinalIgnoreCase))
                // Flaky/broken .gitignore parsing/excluding
                && !ignore.IsIgnored(Path.GetRelativePath(rootPath, path)))
        
        .ToDictionary(path => path, File.ReadAllText);

        return allFiles;
    }

}
