using AssetStudio;
using Microsoft.Extensions.Logging;
using Server.Base.Core.Abstractions;
using Web.AssetBundles.Helpers;
using Web.AssetBundles.Models;
using Web.Launcher.Helpers;
using Web.Launcher.Models;

namespace Web.AssetBundles.Services;

public class BuildAssetBundles : IService
{
    private readonly LauncherConfig _lConfig;
    private readonly ILogger<BuildAssetBundles> _logger;
    private readonly LauncherSink _sink;

    public BuildAssetBundles(LauncherSink sink, LauncherConfig lConfig, ILogger<BuildAssetBundles> logger)
    {
        _sink = sink;
        _lConfig = lConfig;
        _logger = logger;
    }

    public void Initialize() => _sink.GameLaunching += GameLaunching;

    private void GameLaunching() => GenerateAssetBundles();

    private void GenerateAssetBundles()
    {
        _logger.LogInformation("Generating Publish Configuration");
        Logger.Default = new AssetBundleLogger(_logger);
        var manager = new AssetsManager();
        manager.LoadFolder(Path.GetDirectoryName(_lConfig.CacheInfoFile));

        foreach (var assetFile in manager.assetsFileList)
        {
            var assetName = GetMainAssetName(assetFile);
            _logger.LogDebug("Found main asset: {AssetName} in {FileName}", assetName, assetFile.fileName);
        }

        _logger.LogDebug("Publish configuration generated...");
    }

    private static string GetMainAssetName(SerializedFile assetFile)
    {
        var assetBundle = assetFile.Objects.First(o => o.type == ClassIDType.AssetBundle);

        var tree = GetTree(assetBundle.Dump()
            .Split(new[] { Environment.NewLine }, StringSplitOptions.RemoveEmptyEntries));

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
            else
            {
                if (pair.Key != default)
                    pair.Value.Add(treeTxt[1..]);
            }
        }

        return info.Select(i => new TreeInfo(i.Key, GetTree(i.Value))).ToArray();
    }
}
