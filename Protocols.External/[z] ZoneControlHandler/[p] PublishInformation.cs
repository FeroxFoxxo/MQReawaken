using Server.Reawakened.Network.Protocols;

namespace Protocols.External._z__ZoneControlHandler;

public class PublishInformation : ExternalProtocol
{
    public override string ProtocolName => "zp";

    public override void Run(string[] message) =>
        SendXt("zp", "unity.game.publishconfig=PublishConfiguration.xml");
}
