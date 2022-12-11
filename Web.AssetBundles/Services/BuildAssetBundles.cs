using AssetStudio;
using Microsoft.Extensions.Logging;
using Server.Base.Core.Abstractions;
using Server.Base.Core.Extensions;
using System.Xml;
using Web.AssetBundles.Helpers;
using Web.AssetBundles.Models;
using Web.Launcher.Helpers;
using Web.Launcher.Models;

namespace Web.AssetBundles.Services;

public class BuildAssetBundles : IService
{
    private readonly LauncherConfig _lConfig;
    private readonly ILogger<BuildAssetBundles> _logger;
    private readonly string _pubConfigFile;
    private readonly bool _shouldLogAssets;
    private readonly LauncherSink _sink;

    private List<InternalAssetInfo> _internalAssets;

    public BuildAssetBundles(LauncherSink sink, LauncherConfig lConfig, ILogger<BuildAssetBundles> logger)
    {
        _sink = sink;
        _lConfig = lConfig;
        _logger = logger;
        _pubConfigFile = Path.Combine(InternalDirectory.GetBaseDirectory(), "Configs/PublishConfig.xml");
        _shouldLogAssets = false;
    }

    public void Initialize() => _sink.GameLaunching += GameLaunching;

    private void GameLaunching() => GenerateAssetBundles();

    private void GenerateAssetBundles()
    {
        _logger.LogInformation("Getting Publish Configuration");
        var bundlesExist = File.Exists(_pubConfigFile);

        if (!bundlesExist)
        {
            if (_shouldLogAssets)
                Logger.Default = new AssetBundleLogger(_logger);

            _logger.LogInformation("Generating Publish Configuration");
            _internalAssets = new List<InternalAssetInfo>();

            var directories = Directory.GetDirectories(Path.GetDirectoryName(_lConfig.CacheInfoFile)!);

            var numberLoaded = 0;
            var total = directories.Length;

            foreach (var directory in directories)
            {
                _internalAssets.Add(GetAssetBundle(directory));
                _logger.LogTrace("Loaded {Current}/{Total} bundles, {Percentage}%", numberLoaded, total,
                    Math.Floor(numberLoaded * 100.0 / total));

                numberLoaded++;
            }

            File.WriteAllText(_pubConfigFile, GetPublishConfiguration(_internalAssets, true));
        }
        else
        {
            _internalAssets = GetPublishConfiguration(File.ReadAllText(_pubConfigFile));
        }

        _logger.LogDebug("Publish configuration {Type} with {BundleNum} bundles.",
            bundlesExist ? "loaded" : "generated", _internalAssets.Count);
    }

    private InternalAssetInfo GetAssetBundle(string folderName)
    {
        var manager = new AssetsManager();
        manager.LoadFolder(folderName);
        var assetFile = manager.assetsFileList.First();

        var assetName = GetMainAssetName(assetFile);

        var gameObj = assetFile.ObjectsDic.Values.OfType<GameObject>()
            .FirstOrDefault(x => x.m_Name.Equals(assetName, StringComparison.OrdinalIgnoreCase));

        if (gameObj is null)
            throw new InvalidDataException();

        assetName = gameObj.m_Name;

        _logger.LogDebug("Found main asset: {AssetName} in {FileName}", assetName,
            assetFile.fileName.Split('/').Last());

        return new InternalAssetInfo
        {
            Name = assetName,
            Path = assetFile.fullName,

            Version = 0,
            Type = AssetInfo.TypeAsset.Prefab,
            BundleSize = Convert.ToInt32(new FileInfo(assetFile.fullName).Length / 1024),
            Locale = RFC1766Locales.LanguageCodes.en_us
        };
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

    private static List<InternalAssetInfo> GetPublishConfiguration(string xml)
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
                    .Replace("-", "_")),
                BundleSize = Convert.ToInt32(assetElement.GetAttribute("size")),
                Path = assetElement.GetAttribute("path")
            };

            configuration.Add(asset);
        }

        return configuration;
    }

    private static string GetPublishConfiguration(List<InternalAssetInfo> assets, bool includePath)
    {
        var document = new XmlDocument();
        var root = document.CreateElement("assets");
        foreach (var asset in assets)
        {
            var assetXml = document.CreateElement("asset");
            assetXml.SetAttribute("name", asset.Name);
            assetXml.SetAttribute("version", asset.Version.ToString());
            assetXml.SetAttribute("type", Enum.GetName(asset.Type));
            assetXml.SetAttribute("language", Enum.GetName(asset.Locale));

            if (includePath)
                assetXml.SetAttribute("path", asset.Path);

            root.AppendChild(assetXml);
        }

        document.AppendChild(root);

        using var stringWriter = new StringWriter();
        using var xmlTextWriter = XmlWriter.Create(stringWriter, new XmlWriterSettings { Indent = true });

        document.WriteTo(xmlTextWriter);
        xmlTextWriter.Flush();

        return stringWriter.GetStringBuilder().ToString();
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
