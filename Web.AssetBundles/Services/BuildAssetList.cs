using AssetStudio;
using Microsoft.Extensions.Logging;
using Server.Base.Core.Abstractions;
using Server.Base.Core.Helpers;
using Server.Base.Core.Helpers.Internal;
using Server.Base.Core.Models;
using Server.Base.Core.Services;
using ShellProgressBar;
using System.Xml;
using Web.AssetBundles.Events;
using Web.AssetBundles.Extensions;
using Web.AssetBundles.Helpers;
using Web.AssetBundles.LocalAssets;
using Web.AssetBundles.Models;

namespace Web.AssetBundles.Services;

public class BuildAssetList : IService
{
    private readonly ILogger<BuildAssetList> _logger;
    private readonly AssetBundleConfig _config;
    private readonly ServerConsole _console;
    private readonly EventSink _sink;
    private readonly AssetEventSink _assetSink;

    public readonly Dictionary<string, string> PublishConfigs;
    public readonly Dictionary<string, string> AssetDict;

    public Dictionary<string, InternalAssetInfo> InternalAssets;
    public string AssetDictLocation;

    public BuildAssetList(ILogger<BuildAssetList> logger, AssetBundleConfig config,
        EventSink sink,
        AssetEventSink assetSink, ServerConsole console)
    {
        _logger = logger;
        _config = config;
        _sink = sink;
        _assetSink = assetSink;
        _console = console;

        PublishConfigs = new Dictionary<string, string>();
        AssetDict = new Dictionary<string, string>();
    }

    public void Initialize() => _sink.WorldLoad += SetAssetBundles;

    public void SetAssetBundles()
    {
        _console.AddCommand(new ConsoleCommand("setAssetsToDefault",
            "Force generates asset dictionary from default caches directory.",
            _ => GenerateDefaultAssetList(true)));

        _console.AddCommand(new ConsoleCommand("changeDefaultCacheDir",
            "Change the default cache directory and regenerate dictionary.",
            _ =>
            {
                _config.CacheInfoFile = TryGetCacheInfoFile(string.Empty);
                GenerateDefaultAssetList(true);
            }));

        _console.AddCommand(new ConsoleCommand("addCachesToDict",
            "Adds a cache directory to the current asset dictionary.",
            _ =>
            {
                var cacheDir = Path.GetDirectoryName(TryGetCacheInfoFile(string.Empty));
                var assets = GetAssetsFromCache(cacheDir).Where(a => !InternalAssets.ContainsKey(a.Key));
                foreach (var asset in assets)
                {
                    _logger.LogDebug("Loading new cache file {Name} ({Type})", asset.Key, asset.Value.Type);
                    InternalAssets.Add(asset.Key, asset.Value);
                }
            }));

        _config.CacheInfoFile = TryGetCacheInfoFile(_config.CacheInfoFile);

        if (!Directory.Exists(_config.SaveDirectory))
            Directory.CreateDirectory(_config.SaveDirectory);

        if (!Directory.Exists(_config.BundleSaveDirectory))
            Directory.CreateDirectory(_config.BundleSaveDirectory);

        AssetDictLocation = Path.Combine(_config.SaveDirectory, _config.StoredAssetDict);

        GenerateDefaultAssetList(false);
    }

    private string TryGetCacheInfoFile(string defaultFile)
    {
        try
        {
            defaultFile = SetFileValue.SetIfNotNull(defaultFile, "Get Root Cache Info",
                "Root Info File (__info)\0__info\0");
        }
        catch
        {
            // ignored
        }

        while (true)
        {
            _logger.LogInformation("Getting Cache Directory");

            if (string.IsNullOrEmpty(defaultFile) || !defaultFile.EndsWith("__info"))
            {
                _logger.LogError("Please enter the absolute file path for your cache's ROOT '__info' file.");
                defaultFile = Console.ReadLine() ?? string.Empty;
                continue;
            }

            _logger.LogDebug("Got cache directory: {Directory}", Path.GetDirectoryName(defaultFile));
            break;
        }

        return defaultFile;
    }

    private void GenerateDefaultAssetList(bool forceGenerate)
    {
        _logger.LogInformation("Getting Asset Dictionary");

        var dictExists = File.Exists(AssetDictLocation);

        InternalAssets = new Dictionary<string, InternalAssetInfo>();

        InternalAssets = !dictExists || forceGenerate
            ? GetAssetsFromCache(Path.GetDirectoryName(_config.CacheInfoFile))
            : GetAssetsFromDictionary(File.ReadAllText(AssetDictLocation)).OrderAssets();

        InternalAssets.AddLocalXmlFiles(_logger);

        _logger.LogDebug("Loaded {Count} assets to memory.", InternalAssets.Count);
        
        SaveStoredAssets(InternalAssets.Values, AssetDictLocation);

        foreach (var asset in InternalAssets.Values.Where(x => x.Type == AssetInfo.TypeAsset.Unknown))
            _logger.LogError("Could not find type for asset '{Name}' in '{File}'.", asset.Name, asset.Path);

        var vgmtAssets = InternalAssets.Where(x =>
                _config.VirtualGoods.Any(a => string.Equals(a, x.Key) || x.Key.StartsWith($"{a}Dict_")))
            .ToDictionary(x => x.Key, x => x.Value);

        if (!vgmtAssets.Any())
            _logger.LogError("Could not find any virtual good assets! " +
                             "Try adding them into the LocalAsset directory. " +
                             "The game will not run without these.");

        var gameAssets = InternalAssets.Where(x => !vgmtAssets.ContainsKey(x.Key))
            .Select(x => x.Value).ToList();

        PublishConfigs.Clear();
        AssetDict.Clear();

        AddPublishConfiguration(gameAssets, _config.PublishConfigKey);
        AddAssetDictionary(gameAssets, _config.PublishConfigKey);

        AddPublishConfiguration(vgmtAssets.Values, _config.PublishConfigVgmtKey);
        AddAssetDictionary(vgmtAssets.Values, _config.PublishConfigVgmtKey);

        _logger.LogDebug("Generated default dictionaries.");

        _assetSink.InvokeAssetBundlesLoaded(new AssetBundleLoadEventArgs(InternalAssets));
    }

    private Dictionary<string, InternalAssetInfo> GetAssetsFromCache(string directoryPath)
    {
        if (_config.ShouldLogAssets)
            Logger.Default = new AssetBundleLogger(_logger);

        var assets = new List<InternalAssetInfo>();

        var directories = directoryPath.GetLowestDirectories();

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

        var singleAssets = new Dictionary<string, InternalAssetInfo>();

        using (var progressBar = new ProgressBar(directories.Count, "", opt2))
        {
            using var bundleBar = progressBar.Spawn(directories.Count, _config.Message, options);

            foreach (var directory in directories)
            {
                var asset = GetAssetBundle(directory, bundleBar);

                if (asset != null)
                    assets.Add(asset);

                bundleBar.Tick();
                progressBar.Tick();
            }

            bundleBar.Message = $"Finished {_config.Message}";

            foreach (var newAsset in assets)
            {
                if (singleAssets.ContainsKey(newAsset.Name))
                {
                    var oldAsset = singleAssets[newAsset.Name];

                    if (oldAsset.Type == newAsset.Type)
                    {
                        var oldAssetVersion = oldAsset.UnityVersion.GetUnityVersionDouble();
                        var newAssetVersion = newAsset.UnityVersion.GetUnityVersionDouble();

                        if (oldAssetVersion < newAssetVersion || oldAssetVersion == newAssetVersion && oldAsset.BundleSize < newAsset.BundleSize)
                            singleAssets[newAsset.Name] = newAsset;
                    }
                    else
                    {
                        throw new InvalidDataException();
                    }
                }
                else
                {
                    singleAssets.Add(newAsset.Name, newAsset);
                }
            }
        }

        Console.WriteLine();
        _logger.LogDebug("Built asset bundle dictionary");

        return singleAssets.Values.OrderAssets();
    }

    private InternalAssetInfo GetAssetBundle(string folderName, ProgressBarBase bar)
    {
        var manager = new AssetsManager();
        manager.LoadFolder(folderName);

        var assetFile = manager.assetsFileList.FirstOrDefault();

        if (assetFile == null)
        {
            _logger.LogError("Could not find asset in {folderName}, skipping!", folderName);
            return null;
        }

        var asset = new InternalAssetInfo
        {
            Name = assetFile.GetMainAssetName(),
            Path = assetFile.fullName,

            Version = 0,
            Type = AssetInfo.TypeAsset.Unknown,
            BundleSize = Convert.ToInt32(new FileInfo(assetFile.fullName).Length / 1024),
            Locale = RFC1766Locales.LanguageCodes.en_us,
            UnityVersion = assetFile.unityVersion
        };

        var gameObj = assetFile.ObjectsDic.Values.ToList().GetGameObject(asset.Name)?.m_Name;
        var musicObj = assetFile.ObjectsDic.Values.GetMusic(asset.Name)?.m_Name;
        var textObj = assetFile.ObjectsDic.Values.GetText(asset.Name)?.m_Name;

        if (!string.IsNullOrEmpty(gameObj))
        {
            asset.Name = gameObj;

            if (asset.Name.StartsWith("LV"))
                if (!asset.Name.Contains("mesh") && !asset.Name.Contains("plane"))
                {
                    asset.Type = AssetInfo.TypeAsset.Level;
                    bar.Message =
                        $"{_config.Message} - found possible level '{asset.Name}' in {assetFile.fileName.Split('/').Last()}";
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
                bar.Message =
                    $"{_config.Message} - found possible XML '{asset.Name}' in {assetFile.fileName.Split('/').Last()}";

                if (Enum.TryParse<RFC1766Locales.LanguageCodes>(
                        asset.Name.Split('_').Last().Replace('-', '_'),
                        true,
                        out var type)
                   )
                    asset.Locale = type;

                asset.Type = AssetInfo.TypeAsset.XML;
            }
        }

        if (asset.Type == AssetInfo.TypeAsset.Unknown)
            bar.Message = $"{_config.Message} - WARNING: could not find type of asset {asset.Name}";

        return asset;
    }

    private void AddPublishConfiguration(IEnumerable<InternalAssetInfo> assets, string key)
    {
        var document = new XmlDocument();
        var root = document.CreateElement("PublishConfiguration");

        var xmlElements = document.CreateElement("xml_version");

        foreach (var asset in assets.Where(x => x.Type == AssetInfo.TypeAsset.XML))
            xmlElements.AppendChild(asset.ToAssetXml("item", document));

        root.AppendChild(xmlElements);

        var dict = document.CreateElement("item");
        dict.SetAttribute("name", _config.AssetDictKey);
        dict.SetAttribute("value", _config.AssetDictConfigs[key]);
        root.AppendChild(dict);

        document.AppendChild(root);

        var config = document.WriteToString();
        File.WriteAllText(Path.Combine(_config.SaveDirectory, _config.PublishConfigs[key]), config);
        PublishConfigs.Add(key, config);
    }

    private void AddAssetDictionary(IEnumerable<InternalAssetInfo> assets, string key)
    {
        var document = new XmlDocument();
        var root = document.CreateElement("assets");

        foreach (var asset in assets)
            root.AppendChild(asset.ToPubXml("asset", document));

        document.AppendChild(root);

        var assetDict = document.WriteToString();
        File.WriteAllText(Path.Combine(_config.SaveDirectory, _config.AssetDictConfigs[key]), assetDict);
        AssetDict.Add(key, assetDict);
    }

    private static void SaveStoredAssets(IEnumerable<InternalAssetInfo> assets, string saveDir)
    {
        var document = new XmlDocument();
        var root = document.CreateElement("assets");

        foreach (var asset in assets)
            root.AppendChild(asset.ToStoredXml("asset", document));

        document.AppendChild(root);

        File.WriteAllText(saveDir, document.WriteToString());
    }

    private static IEnumerable<InternalAssetInfo> GetAssetsFromDictionary(string xml)
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

            configuration.Add(assetElement.XmlToAsset());
        }

        return configuration;
    }
}
