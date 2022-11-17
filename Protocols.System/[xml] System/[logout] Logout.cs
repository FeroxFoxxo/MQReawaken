﻿using System.Xml;
using Server.Reawakened.Network.Protocols;

namespace Protocols.System._xml__SysProtocols;

public class Logout : SystemProtocol
{
    public override string ProtocolName => "logout";

    public override void Run(XmlDocument xmlDoc) => SendXml("logout", "");
}