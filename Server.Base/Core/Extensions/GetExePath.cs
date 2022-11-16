using System.Diagnostics;

namespace Server.Base.Core.Extensions;

public static class GetExePath
{
    public static string Path()
    {
        var processModule = Process.GetCurrentProcess().MainModule;
        return processModule != null ? processModule.FileName : string.Empty;
    }
}
