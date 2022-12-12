using AssetStudio;
using Microsoft.Extensions.Logging;
using Server.Base.Core.Abstractions;
using Server.Base.Core.Helpers;
using Server.Base.Core.Helpers.Internal;
using ShellProgressBar;
using System.Xml;
using Web.AssetBundles.Events;
using Web.AssetBundles.Extensions;
using Web.AssetBundles.Helpers;
using Web.AssetBundles.Models;

namespace Web.AssetBundles.Services;

public class BuildPubConfig : IService
{
    private readonly ILogger<BuildPubConfig> _logger;
    private readonly AssetBundleConfig _config;
    private readonly EventSink _sink;
    private readonly AssetEventSink _assetSink;

    private InternalAssetInfo[] _internalAssets;

    public string AssetDictionary { get; private set; }
    public string PublishConfiguration { get; private set; }

    public BuildPubConfig(ILogger<BuildPubConfig> logger, AssetBundleConfig config,
        EventSink sink,
        AssetEventSink assetSink)
    {
        _logger = logger;
        _config = config;
        _sink = sink;
        _assetSink = assetSink;
    }

    public void Initialize() => _sink.WorldLoad += SetAssetBundles;

    public void SetAssetBundles()
    {
        try
        {
            _config.CacheInfoFile = SetFileValue.SetIfNotNull(_config.CacheInfoFile, "Get Root Cache Info",
                "Root Info File (__info)\0__info\0");
        }
        catch
        {
            // ignored
        }

        while (true)
        {
            _logger.LogInformation("Getting Cache Directory");

            if (string.IsNullOrEmpty(_config.CacheInfoFile) || !_config.CacheInfoFile.EndsWith("__info"))
            {
                _logger.LogError("Please enter the absolute file path for your cache's ROOT '__info' file.");
                _config.CacheInfoFile = Console.ReadLine() ?? string.Empty;
                continue;
            }

            _logger.LogDebug("Got cache directory: {Directory}", Path.GetDirectoryName(_config.CacheInfoFile));
            break;
        }

        GenerateAssetBundles();
    }

    public void GenerateAssetBundles()
    {
        _logger.LogInformation("Getting Publish Configuration");
        var bundlesExist = File.Exists(_config.AssetDictionaryConfig);

        if (!bundlesExist)
        {
            if (_config.ShouldLogAssets)
                Logger.Default = new AssetBundleLogger(_logger);

            _logger.LogInformation("Generating Publish Configuration");
            var assets = new List<InternalAssetInfo>();

            var directories = Directory.GetDirectories(Path.GetDirectoryName(_config.CacheInfoFile)!);

            var options = new ProgressBarOptions
            {
                ForegroundColor = ConsoleColor.Yellow,
                BackgroundColor = ConsoleColor.DarkYellow,
                ForegroundColorDone = ConsoleColor.DarkGreen,
                ProgressCharacter = '─',
                ProgressBarOnBottom = true
            };

            var opt2 = options.DeepCopy();
            opt2.DisableBottomPercentage = true;

            using (var progressBar = new ProgressBar(2, "", opt2))
            {
                using (var child = progressBar.Spawn(directories.Length, _config.Message, options))
                {
                    foreach (var directory in directories)
                    {
                        assets.Add(GetAssetBundle(directory, child));
                        child.Tick();
                    }

                    child.Message = "Finished " + _config.Message;
                }

                progressBar.Tick();

                using (var child = progressBar.Spawn(assets.Count, "Removing Duplicates", options))
                {
                    var singleAssets = new Dictionary<string, InternalAssetInfo>();

                    foreach (var asset in assets)
                    {
                        if (singleAssets.ContainsKey(asset.Name))
                        {
                            var testAsset = singleAssets[asset.Name];

                            if (testAsset.Type == asset.Type)
                            {
                                if (testAsset.BundleSize < asset.BundleSize)
                                    singleAssets[asset.Name] = asset;
                            }
                            else
                            {
                                throw new InvalidDataException();
                            }
                        }
                        else
                        {
                            singleAssets.Add(asset.Name, asset);
                        }

                        child.Tick();
                    }

                    _internalAssets = OrderAssets(singleAssets.Values);
                }

                progressBar.Tick();
            }

            Console.WriteLine();
            File.WriteAllText(_config.AssetDictionaryConfig, GetAssetDictionary(_internalAssets, true));
        }
        else
        {
            _internalAssets = OrderAssets(GetAssetDictionary(File.ReadAllText(_config.AssetDictionaryConfig)));
        }

        _logger.LogDebug("Publish configuration {Type} with {BundleNum} bundles.",
            bundlesExist ? "loaded" : "generated", _internalAssets.Length);

        PublishConfiguration = GetPublishConfiguration(_internalAssets);
        File.WriteAllText(_config.GlobalPublishConfig, PublishConfiguration);

        AssetDictionary = GetAssetDictionary(_internalAssets, false);
        File.WriteAllText(_config.GlobalAssetDictionary, AssetDictionary);

        _assetSink.InvokeAssetBundlesLoaded(new AssetBundleLoadEventArgs(_internalAssets));
    }

    private static string GetPublishConfiguration(IEnumerable<InternalAssetInfo> assets)
    {
        var document = new XmlDocument();
        var root = document.CreateElement("PublishConfiguration");

        var xmlElements = document.CreateElement("xml_version");
        foreach (var asset in assets.Where(x => x.Type == AssetInfo.TypeAsset.XML))
            xmlElements.AppendChild(asset.ToXml("item", document));
        root.AppendChild(xmlElements);

        var dict = document.CreateElement("item");
        dict.SetAttribute("name", "publish.asset_dictionary");
        dict.SetAttribute("value", "assetDictionary.xml");
        root.AppendChild(dict);

        document.AppendChild(root);

        return document.WriteToString(false);
    }

    private static string GetAssetDictionary(IEnumerable<InternalAssetInfo> assets, bool includePath)
    {
        var document = new XmlDocument();
        var root = document.CreateElement("assets");

        foreach (var asset in assets)
        {
            root.AppendChild(includePath
                ? asset.ToXmlWithTypePath("asset", document)
                : asset.ToXmlWithType("asset", document));
        }

        document.AppendChild(root);

        return document.WriteToString(false);
    }

    private static IEnumerable<InternalAssetInfo> GetAssetDictionary(string xml)
    {
        var configuration = new List<InternalAssetInfo>();

        var document = new XmlDocument();
        document.LoadXml(xml);

        if (document.DocumentElement == null)
            return configuration;

        foreach (XmlNode node in document.DocumentElement.ChildNodes)
        {
            if (node is not XmlElement assetElement)
                continue;

            var asset = new InternalAssetInfo
            {
                Name = assetElement.GetAttribute("name"),
                Version = Convert.ToInt32(assetElement.GetAttribute("version")),
                Type = Enum.Parse<AssetInfo.TypeAsset>(assetElement.GetAttribute("type")),
                Locale = Enum.Parse<RFC1766Locales.LanguageCodes>(assetElement.GetAttribute("language")
                    .Replace('-', '_')),
                BundleSize = Convert.ToInt32(assetElement.GetAttribute("size")),
                Path = assetElement.InnerText
            };

            configuration.Add(asset);
        }

        return configuration;
    }

    private InternalAssetInfo GetAssetBundle(string folderName, ProgressBarBase bar)
    {
        var manager = new AssetsManager();
        manager.LoadFolder(folderName);

        var assetFile = manager.assetsFileList.First();

        var asset = new InternalAssetInfo
        {
            Name = GetMainAssetName(assetFile),
            Path = assetFile.fullName,

            Version = 0,
            Type = AssetInfo.TypeAsset.Unknown,
            BundleSize = Convert.ToInt32(new FileInfo(assetFile.fullName).Length / 1024),
            Locale = RFC1766Locales.LanguageCodes.en_us
        };

        var gameObj = assetFile.ObjectsDic.Values.GetGameObject(asset.Name)?.m_Name;
        var musicObj = assetFile.ObjectsDic.Values.GetMusic(asset.Name)?.m_Name;
        var textObj = assetFile.ObjectsDic.Values.GetText(asset.Name)?.m_Name;

        if (!string.IsNullOrEmpty(gameObj))
        {
            asset.Name = gameObj;

            if (asset.Name.StartsWith("LV"))
                if (!asset.Name.Contains("mesh") && !asset.Name.Contains("plane"))
                {
                    asset.Type = AssetInfo.TypeAsset.Level;
                    bar.Message = _config.Message +
                                  $" - found possible level '{asset.Name}' in {assetFile.fileName.Split('/').Last()}";
                }

            if (asset.Type == AssetInfo.TypeAsset.Unknown)
                asset.Type = AssetInfo.TypeAsset.Prefab;
        }
        else if (!string.IsNullOrEmpty(musicObj))
        {
            asset.Name = musicObj;
            asset.Type = AssetInfo.TypeAsset.Audio;
        }
        else if (!string.IsNullOrEmpty(textObj))
        {
            asset.Name = textObj;

            if (asset.Name.StartsWith("NavMesh"))
            {
                asset.Type = AssetInfo.TypeAsset.NavMesh;
            }
            else
            {
                bar.Message = _config.Message +
                              $" - found possible XML '{asset.Name}' in {assetFile.fileName.Split('/').Last()}";

                if (Enum.TryParse<RFC1766Locales.LanguageCodes>(
                        asset.Name.Split('_').Last().Replace('-', '_'),
                        true,
                        out var type)
                   )
                    asset.Locale = type;

                asset.Type = AssetInfo.TypeAsset.XML;
            }
        }

        return asset.Type == AssetInfo.TypeAsset.Unknown ? throw new InvalidDataException() : asset;
    }

    private static string GetMainAssetName(SerializedFile assetFile)
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

    private static InternalAssetInfo[] OrderAssets(IEnumerable<InternalAssetInfo> assets) =>
        assets.GroupBy(x => x.Type).SelectMany(g => g.OrderBy(x => x.Name).ToList()).ToArray();

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
