using Server.Reawakened.Data.Modals;

namespace Server.Reawakened.Data.Extensions;

public static class GetValues
{
    public static string GetPropertyValues(this UserInfo user) =>
        string.Join('|', user.Properties.Select(x => $"{(int)x.Key}|{x.Value}"));

    public static string GetDebugValues(this UserInfo user) =>
        string.Join('|', user.DebugValues.Select(x => $"{(int)x.Key}|{x.Value}"));
}
