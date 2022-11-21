namespace Server.Base.Core.Extensions;

public static class GetFile
{
    public static FileStream GetFileStream(string fileName, string internalDir, FileMode mode)
    {
        var currentLog = Path.Combine(
            InternalDirectory.GetBaseDirectory(), internalDir, fileName);

        var path = Path.GetDirectoryName(currentLog);

        if (!Directory.Exists(path) && path != null)
            Directory.CreateDirectory(path);

        return File.Open(currentLog, mode);
    }

    public static StreamWriter GetStreamWriter(string fileName, string internalDir, FileMode mode) =>
        new(GetFileStream(fileName, internalDir, mode))
        {
            AutoFlush = false
        };
}
