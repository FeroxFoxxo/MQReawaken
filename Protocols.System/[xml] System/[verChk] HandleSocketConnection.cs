using Server.Reawakened.Network.Protocols;
using SmartFoxClientAPI;
using System.Xml;

namespace Protocols.System._xml__System;

public class HandleSocketConnection : SystemProtocol
{
    public override string ProtocolName => "verChk";

    public SmartFoxClient SmartFoxClient { get; set; }

    public override void Run(XmlDocument xmlDoc)
    {
        var version = xmlDoc.SelectSingleNode("/msg/body/ver/@v")?.Value;

        SendXml(
            version == SmartFoxClient.GetVersion().Replace(".", "")
                ? "apiOK"
                : "apiKO",
            "");
    }
}
