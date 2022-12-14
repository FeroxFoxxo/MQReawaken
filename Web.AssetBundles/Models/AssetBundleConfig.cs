using Server.Base.Core.Abstractions;
using Server.Base.Core.Extensions;

namespace Web.AssetBundles.Models;

public class AssetBundleConfig : IConfig
{
    public string AssetDictKey { get; set; }

    public string SaveDirectory { get; set; }
    public string StoredAssetDict { get; set; }

    public bool ShouldLogAssets { get; set; }
    public string CacheInfoFile { get; set; }
    public string Message { get; set; }

    public string PublishConfigKey { get; set; }
    public string PublishConfigVgmtKey { get; set; }

    public Dictionary<string, string> PublishConfigs { get; set; }
    public Dictionary<string, string> AssetDictConfigs { get; set; }
    public List<string> VirtualGoods { get; set; }

    public AssetBundleConfig()
    {
        AssetDictKey = "publish.asset_dictionary";

        SaveDirectory = Path.Combine(InternalDirectory.GetBaseDirectory(), "Assets");
        StoredAssetDict = "CachedAssetDictionary.xml";

        ShouldLogAssets = false;
        CacheInfoFile = string.Empty;
        Message = "Loading Asset Bundles";

        PublishConfigKey = "unity.game.publishconfig";
        PublishConfigVgmtKey = "unity.game.vgmt.publishconfig";

        PublishConfigs = new Dictionary<string, string>
        {
            { PublishConfigKey, "PublishConfiguration.xml" },
            { PublishConfigVgmtKey, "PublishConfiguration.VGMT.xml" }
        };

        AssetDictConfigs = new Dictionary<string, string>
        {
            { PublishConfigKey, "assetDictionary.xml" },
            { PublishConfigVgmtKey, "assetDictionary.VGMT.xml" }
        };

        VirtualGoods = new List<string>
        {
            "ItemCatalog",
            "PetAbilities",
            "UserGiftMessage",
            "vendor_catalogs",
            "IconBank_VGMT",
            "IconBank_Pets"
        };
    }
}
