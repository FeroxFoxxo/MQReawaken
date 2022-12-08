using A2m.Server;
using Server.Reawakened.Players.Modals;

namespace Server.Reawakened.Players.Extensions;

public static class GetDebugs
{
    public static Dictionary<DebugHandler.DebugVariables, bool> DefaultDebugValues(UserInfo _)
        => new()
        {
            { DebugHandler.DebugVariables.Sharder_active, false },
            { DebugHandler.DebugVariables.Sharder_1, false },
            { DebugHandler.DebugVariables.Sharder_2, false },
            { DebugHandler.DebugVariables.Ewallet, true },
            { DebugHandler.DebugVariables.Chat, true },
            { DebugHandler.DebugVariables.BugReport, true },
            { DebugHandler.DebugVariables.Crisp, true },
            { DebugHandler.DebugVariables.Trade, true }
        };

    public static string GetDebugValues(this UserInfo user) =>
        string.Join('|', DefaultDebugValues(user).Select(x => $"{(int)x.Key}|{(x.Value ? "On" : "Off")}"));
}
