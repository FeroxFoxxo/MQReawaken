namespace Web.AssetBundles.Models;

public class InternalAssetInfo
{
    public string Name { get; set; }
    public int Version { get; set; }
    public string UnityVersion { get; set; }
    public AssetInfo.TypeAsset Type { get; set; }
    public int BundleSize { get; set; }
    public RFC1766Locales.LanguageCodes Locale { get; set; }
    public string Path { get; set; }
}
