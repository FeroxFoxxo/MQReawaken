using Server.Base.Timers.Enums;

namespace Server.Base.Timers.Extensions;

public static class GetPriority
{
    public static TimerPriority ComputePriority(this TimeSpan ts) =>
        ts.TotalMinutes switch
        {
            >= 10.0 => TimerPriority.OneMinute,
            >= 1.0 => TimerPriority.FiveSeconds,
            _ => ts.TotalSeconds switch
            {
                >= 10.0 => TimerPriority.OneSecond,
                >= 5.0 => TimerPriority.TwoFiftyMs,
                >= 2.5 => TimerPriority.FiftyMs,
                >= 1.0 => TimerPriority.TwentyFiveMs,
                >= 0.5 => TimerPriority.TenMs,
                _ => TimerPriority.EveryTick
            }
        };
}
