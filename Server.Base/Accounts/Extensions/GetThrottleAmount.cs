using Server.Base.Accounts.Modals;

namespace Server.Base.Accounts.Extensions;

public static class GetThrottleAmount
{
    public static TimeSpan ComputeThrottle(this InvalidAccountAccessLog log) =>
        log.Counts switch
        {
            >= 15 => TimeSpan.FromMinutes(5.0),
            >= 10 => TimeSpan.FromMinutes(1.0),
            >= 5 => TimeSpan.FromSeconds(20.0),
            >= 3 => TimeSpan.FromSeconds(10.0),
            >= 1 => TimeSpan.FromSeconds(2.0),
            _ => TimeSpan.Zero
        };
}
