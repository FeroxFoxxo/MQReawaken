using Server.Reawakened.Network.Protocols;

namespace Protocols.External._l__ExtLevelEditor;

public class RoomList : ExternalProtocol
{
    public override string ProtocolName => "lr";

    public override void Run(string[] message) => SendXml("rmList", "");
}
