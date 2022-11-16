using System.Xml;
using Server.Reawakened.Network.Protocols;
using SmartFoxClientAPI;

namespace Protocols.System._xml__SysProtocols;

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
