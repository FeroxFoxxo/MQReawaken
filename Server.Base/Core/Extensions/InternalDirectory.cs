namespace Server.Base.Core.Extensions;

public static class InternalDirectory
{
    private static string _baseDirectory;

    public static void CreateDirectory(string path)
    {
        if (Directory.Exists(path))
            return;

        if (path != null)
            Directory.CreateDirectory(path);
    }

    public static void CreateDirectory(string path1, string path2) => CreateDirectory(Combine(path1, path2));

    public static string Combine(string path1, string path2) => path1.Length == 0 ? path2 : Path.Combine(path1, path2);

    public static string GetBaseDirectory()
    {
        if (_baseDirectory != null) return _baseDirectory;

        try
        {
            _baseDirectory = GetExePath.Path();

            if (_baseDirectory.Length > 0)
                _baseDirectory = Path.GetDirectoryName(_baseDirectory);
        }
        catch
        {
            _baseDirectory = "";
        }

        return _baseDirectory;
    }
}
