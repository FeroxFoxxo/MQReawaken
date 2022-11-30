using Server.Reawakened.Network.Protocols;
using System.Xml;

namespace Protocols.System._xml__System;

public class Logout : SystemProtocol
{
    public override string ProtocolName => "logout";

    public override void Run(XmlDocument xmlDoc) => SendXml("logout", "");
}
