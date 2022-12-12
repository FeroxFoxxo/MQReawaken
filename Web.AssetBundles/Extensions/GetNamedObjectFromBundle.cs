using AssetStudio;
using Object = AssetStudio.Object;

namespace Web.AssetBundles.Extensions;

public static class GetNamedObjectFromBundle
{
    public static GameObject GetGameObject(this IEnumerable<Object> objects, string assetName) =>
        objects.OfType<GameObject>()
            .FirstOrDefault(x => x.m_Name.Equals(assetName, StringComparison.OrdinalIgnoreCase));

    public static AudioClip GetMusic(this IEnumerable<Object> objects, string assetName) =>
        objects.OfType<AudioClip>()
            .FirstOrDefault(x => x.m_Name.Equals(assetName, StringComparison.OrdinalIgnoreCase));

    public static TextAsset GetText(this IEnumerable<Object> objects, string assetName) =>
        objects.OfType<TextAsset>()
            .FirstOrDefault(x => x.m_Name.Equals(assetName, StringComparison.OrdinalIgnoreCase));
}
