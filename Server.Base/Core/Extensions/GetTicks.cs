using System.Diagnostics;

namespace Server.Base.Core.Extensions;

public static class GetTicks
{
    public static double Ticks => !GetOsType.IsUnix() ? Stopwatch.GetTimestamp() : DateTime.UtcNow.Ticks;
}
