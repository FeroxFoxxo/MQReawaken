using Web.AssetBundles.Models;

namespace Web.AssetBundles.Events;

public class AssetBundleLoadEventArgs
{
    public readonly InternalAssetInfo[] InternalAssets;

    public AssetBundleLoadEventArgs(InternalAssetInfo[] internalAssets) => InternalAssets = internalAssets;
}
