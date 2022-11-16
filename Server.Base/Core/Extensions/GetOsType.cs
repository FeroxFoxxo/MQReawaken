using System.Runtime.InteropServices;

namespace Server.Base.Core.Extensions;

public static class GetOsType
{
    private static bool? _isUnix;

    public static bool IsUnix() => _isUnix ??= !RuntimeInformation.IsOSPlatform(OSPlatform.Windows);
}
