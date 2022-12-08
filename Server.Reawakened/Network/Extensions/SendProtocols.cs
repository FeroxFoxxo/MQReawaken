using Server.Base.Network;
using Server.Reawakened.Data;
using Server.Reawakened.Players;

namespace Server.Reawakened.Network.Extensions;

public static class SendProtocols
{
    public static void SendXml(this NetState state, string actionType, string message) =>
        state.Send(
            $"<msg t=\"sys\"><body action='{actionType}' r='{state.GetLevelId()}'>{message}</body></msg>"
        );

    public static void SendXt(this NetState state, string actionType, params string[] messages) =>
        state.Send(
            $"%xt%{actionType}%{state.GetLevelId()}%{string.Join('%', messages)}%"
        );

    private static int GetLevelId(this NetState state)
    {
        var user = state.Get<Player>();
        return user?.GetLevelId() ?? -1;
    }
}
