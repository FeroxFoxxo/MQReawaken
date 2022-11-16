using Server.Base.Network;
using Server.Reawakened.Data.Extensions;
using Server.Reawakened.Data.Services;

namespace Server.Reawakened.Network.Extensions;

public static class SendProtocols
{
    public static void SendXml(this NetState state, UserHandler handler, string actionType, string message) =>
        state.Send(
            $"<msg t=\"sys\"><body action='{actionType}' r='{state.GetUser(handler)?.GetLevelId() ?? -1}'>{message}</body></msg>"
        );

    public static void SendXt(this NetState state, UserHandler handler, string actionType, params string[] messages) =>
        state.Send(
            $"%xt%{actionType}%{state.GetUser(handler)?.GetLevelId() ?? -1}%{string.Join('%', messages)}%"
        );
}
