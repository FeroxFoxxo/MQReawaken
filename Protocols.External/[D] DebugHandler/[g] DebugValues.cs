using Server.Reawakened.Data;
using Server.Reawakened.Network.Protocols;
using Server.Reawakened.Players;
using Server.Reawakened.Players.Extensions;

namespace Protocols.External._D__DebugHandler;

public class DebugValues : ExternalProtocol
{
    public override string ProtocolName => "Dg";

    public override void Run(string[] message) =>
        SendXt("Dg", NetState.Get<Player>().UserInfo.GetDebugValues());
}
