using System.Xml;
using Server.Reawakened.Network.Protocols;
using Server.Reawakened.Network.Services;

namespace Protocols.System._xml__SysProtocols;

public class GetRandomKey : SystemProtocol
{
    public override string ProtocolName => "rndK";

    public RandomKeyGenerator KeyGenerator { get; set; }

    public override void Run(XmlDocument xmlDoc) =>
        SendXml("rndK", $"<k>{KeyGenerator.GetRandomKey(NetState)}</k>");
}
