namespace Server.Base.Core.Extensions;

public static class GetTime
{
    public static string GetTimeStamp()
    {
        var now = DateTime.UtcNow;

        return $"{now.Day}-{now.Month}-{now.Year}-{now.Hour}-{now.Minute}-{now.Second}";
    }

    public static TimeSpan ToTimeSpan(string value)
    {
        _ = TimeSpan.TryParse(value, out var timeSpan);

        return timeSpan;
    }
}
