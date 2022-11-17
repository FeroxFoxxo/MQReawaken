namespace Server.Base.Core.Extensions;

public static class GetLogFileStream
{
    public static FileStream GetLogFile(string fileName, string internalDir)
    {
        var currentLog = Path.Combine(
            InternalDirectory.GetBaseDirectory(), "Logs", internalDir, fileName);

        var path = Path.GetDirectoryName(currentLog);

        if (!Directory.Exists(path) && path != null)
            Directory.CreateDirectory(path);

        return !File.Exists(currentLog)
            ? File.Create(currentLog)
            : File.Open(currentLog, FileMode.Append);
    }
}
