using Server.Base.Core.Abstractions;
using Server.Base.Core.Extensions;

namespace Web.AssetBundles.Models;

public class AssetBundleConfig : IConfig
{
    public string GlobalAssetDictionary { get; set; }
    public string GlobalPublishConfig { get; set; }
    public string AssetDictionaryConfig { get; set; }

    public bool ShouldLogAssets { get; set; }
    public string CacheInfoFile { get; set; }
    public string Message { get; set; }

    public AssetBundleConfig()
    {
        AssetDictionaryConfig = Path.Combine(InternalDirectory.GetBaseDirectory(), "Configs/AssetDictionaryConfig.xml");

        GlobalPublishConfig = Path.Combine(InternalDirectory.GetBaseDirectory(), "Configs/GlobalPublishConfig.xml");
        GlobalAssetDictionary = Path.Combine(InternalDirectory.GetBaseDirectory(), "Configs/GlobalAssetDictionary.xml");

        ShouldLogAssets = false;
        Message = "Loading Asset Bundles";
        CacheInfoFile = string.Empty;
    }
}
