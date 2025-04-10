namespace CodeHealth.Core.IO;

public static class RunInfo
{
    public static string GetDirectoryName(DateTime runTime) {
        return Path.Combine("runs", runTime.ToString("yyyy-MM-dd_HH-mm"));
    }
}