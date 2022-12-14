using Server.Reawakened.Network.Protocols;
using Web.AssetBundles.Models;

namespace Protocols.External._z__ZoneControlHandler;

public class PublishInformation : ExternalProtocol
{
    public override string ProtocolName => "zp";

    public AssetBundleConfig Config { get; set; }

    public override void Run(string[] message) =>
        SendXt("zp", string.Join(',',
            Config.PublishConfigs.Select(x => $"{x.Key}={x.Value}")
        ));
}
