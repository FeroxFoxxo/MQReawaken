using Server.Base.Core.Extensions;
using Web.AssetBundles.Models;

namespace Web.AssetBundles.LocalAssets;

public class GetLocalAssets
{
    public static List<InternalAssetInfo> GetLocalXmlFiles() =>
        Directory.GetFiles(Path.Combine(InternalDirectory.GetBaseDirectory(), "LocalAssets"), "*.xml")
            .Select(file => new InternalAssetInfo
            {
                BundleSize = Convert.ToInt32(new FileInfo(file).Length / 1024),
                Locale = RFC1766Locales.LanguageCodes.en_us,
                Name = Path.GetFileName(file).Split('.')[0],
                Type = AssetInfo.TypeAsset.XML,
                Path = file,
                Version = 0
            })
            .ToList();
}
