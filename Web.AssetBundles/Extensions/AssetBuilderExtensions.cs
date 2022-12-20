using Web.AssetBundles.Models;

namespace Web.AssetBundles.Extensions;

public static class AssetBuilderExtensions
{
    public static List<string> GetLowestDirectories(this string directory)
    {
        var list = new List<string>();
        GetLowestDirectories(directory, list);
        return list;
    }

    private static void GetLowestDirectories(string directory, List<string> directories)
    {
        var subDirs = Directory.GetDirectories(directory);

        if (subDirs.Length > 0)
            foreach (var subDir in subDirs)
                GetLowestDirectories(subDir, directories);
        else
            directories.Add(directory);
    }

    public static Dictionary<string, InternalAssetInfo> OrderAssets(this IEnumerable<InternalAssetInfo> assets) =>
        assets.GroupBy(x => x.Type)
            .SelectMany(g => g.OrderBy(x => x.Name).ToList())
            .ToDictionary(x => x.Name, x => x);
}
