namespace CodeHealth.Core.IO;

public static class RunInfo
{
    public static string CreateRun(DateTime runTime)
    {
        var directory = GetDirectoryName(runTime);
        Directory.CreateDirectory(directory);
        return directory;
    }

    private static string GetDirectoryName(DateTime runTime)
    {
        return Path.Combine("runs", runTime.ToString("yyyy-MM-dd_HH-mm"));
    }
}