using AssetStudio;
using Web.AssetBundles.Models;

namespace Web.AssetBundles.Extensions;

public static class GetMainAsset
{
    public static string GetMainAssetName(this SerializedFile assetFile)
    {
        var assetBundle = assetFile.ObjectsDic.Values.First(o => o.type == ClassIDType.AssetBundle);

        var dump = assetBundle.Dump();
        var lines = dump.Split(new[] { Environment.NewLine }, StringSplitOptions.RemoveEmptyEntries);
        var tree = GetTree(lines);

        var baseBundle = tree.First(a => a.Name == "AssetBundle Base");
        var mainAsset = baseBundle.SubTrees.First(a => a.Name == "AssetInfo m_MainAsset");
        var asset = GetAssetString(mainAsset);

        var container = baseBundle.SubTrees.First(a => a.Name == "map m_Container");
        var array = container.SubTrees.First(a => a.Name.StartsWith("int size = "));

        foreach (var data in array.SubTrees.Where(a => a.Name == "pair data"))
        {
            var dAssetInfo = data.SubTrees.First(a => a.Name == "AssetInfo second");

            if (GetAssetString(dAssetInfo) != asset)
                continue;

            const string nameStart = "string first = \"";
            return data.SubTrees.First(a => a.Name.StartsWith(nameStart)).Name[nameStart.Length..][..^1];
        }

        throw new InvalidDataException();
    }

    private static string GetAssetString(TreeInfo info) =>
        GenerateStringFromTree(info.SubTrees.First(a => a.Name == "PPtr<Object> asset"));

    private static string GenerateStringFromTree(TreeInfo tree) =>
        $"{tree.Name}\n{string.Join('\t', tree.SubTrees.Select(GenerateStringFromTree))}";

    private static TreeInfo[] GetTree(IEnumerable<string> tree)
    {
        var info = new List<KeyValuePair<string, List<string>>>();
        KeyValuePair<string, List<string>> pair = default;

        foreach (var treeTxt in tree)
        {
            if (!treeTxt.StartsWith('\t'))
            {
                pair = new KeyValuePair<string, List<string>>(treeTxt, new List<string>());
                if (pair.Key != default)
                    info.Add(pair);
            }
            else if (pair.Key != default)
            {
                pair.Value.Add(treeTxt[1..]);
            }
        }

        return info.Select(i => new TreeInfo(i.Key, GetTree(i.Value))).ToArray();
    }
}
