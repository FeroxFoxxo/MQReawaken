using Server.Base.Core.Abstractions;
using Server.Base.Core.Extensions;

namespace Web.AssetBundles.Models;

public class AssetBundleConfig : IConfig
{
    public string GlobalPubConfigFile { get; set; }
    public string PubConfigFile { get; set; }
    public bool ShouldLogAssets { get; set; }
    public string CacheInfoFile { get; set; }
    public string Message { get; set; }

    public AssetBundleConfig()
    {
        CacheInfoFile = string.Empty;
        PubConfigFile = Path.Combine(InternalDirectory.GetBaseDirectory(), "Configs/PublishConfig.xml");
        GlobalPubConfigFile = Path.Combine(InternalDirectory.GetBaseDirectory(), "Configs/GlobalPublishConfig.xml");
        ShouldLogAssets = false;
        Message = "Loading Asset Bundles";
    }
}
