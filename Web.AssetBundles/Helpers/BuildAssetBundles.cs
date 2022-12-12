using AssetStudio;
using Microsoft.Extensions.Logging;
using ShellProgressBar;
using System.Xml;
using Web.AssetBundles.Models;
using Object = AssetStudio.Object;

namespace Web.AssetBundles.Helpers;

public class BuildAssetBundles
{
    private readonly ILogger<BuildAssetBundles> _logger;
    private readonly AssetBundleConfig _config;

    private List<InternalAssetInfo> _internalAssets;

    public BuildAssetBundles(ILogger<BuildAssetBundles> logger, AssetBundleConfig config)
    {
        _logger = logger;
        _config = config;
    }

    public string PublishConfiguration { get; private set; }

    public void GenerateAssetBundles()
    {
        _logger.LogInformation("Getting Publish Configuration");
        var bundlesExist = File.Exists(_config.PubConfigFile);

        if (!bundlesExist)
        {
            if (_config.ShouldLogAssets)
                Logger.Default = new AssetBundleLogger(_logger);

            _logger.LogInformation("Generating Publish Configuration");
            _internalAssets = new List<InternalAssetInfo>();

            var directories = Directory.GetDirectories(Path.GetDirectoryName(_config.CacheInfoFile)!);

            var options = new ProgressBarOptions
            {
                ForegroundColor = ConsoleColor.Yellow,
                ForegroundColorDone = ConsoleColor.DarkGreen
            };

            using (var progressBar = new ProgressBar(directories.Length, _config.Message, options))
            {
                foreach (var directory in directories)
                {
                    _internalAssets.Add(GetAssetBundle(directory, progressBar));
                    progressBar.Tick();
                }

                progressBar.Message = "Finished " + _config.Message;
            }

            File.WriteAllText(_config.PubConfigFile, GetPublishConfiguration(_internalAssets, true));
        }
        else
        {
            _internalAssets = GetPublishConfiguration(File.ReadAllText(_config.PubConfigFile));
        }

        _logger.LogDebug("Publish configuration {Type} with {BundleNum} bundles.",
            bundlesExist ? "loaded" : "generated", _internalAssets.Count);

        PublishConfiguration = GetPublishConfiguration(_internalAssets, false);
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

        var gameObj = GetNameOfGameObject(assetFile.ObjectsDic.Values, asset.Name);
        var musicObj = GetNameOfMusic(assetFile.ObjectsDic.Values, asset.Name);
        var textObj = GetNameOfText(assetFile.ObjectsDic.Values, asset.Name);

        if (!string.IsNullOrEmpty(gameObj))
        {
            asset.Name = gameObj;
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

    private static string GetNameOfGameObject(IEnumerable<Object> objects, string assetName) =>
        objects.OfType<GameObject>()
            .FirstOrDefault(x => x.m_Name.Equals(assetName, StringComparison.OrdinalIgnoreCase))?.m_Name;

    private static string GetNameOfMusic(IEnumerable<Object> objects, string assetName) =>
        objects.OfType<AudioClip>()
            .FirstOrDefault(x => x.m_Name.Equals(assetName, StringComparison.OrdinalIgnoreCase))?.m_Name;

    private static string GetNameOfText(IEnumerable<Object> objects, string assetName) =>
        objects.OfType<TextAsset>()
            .FirstOrDefault(x => x.m_Name.Equals(assetName, StringComparison.OrdinalIgnoreCase))?.m_Name;

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
                Locale = Enum.Parse<RFC1766Locales.LanguageCodes>(assetElement.GetAttribute("language")),
                BundleSize = Convert.ToInt32(assetElement.GetAttribute("size")),
                Path = assetElement.InnerText
            };

            configuration.Add(asset);
        }

        return configuration;
    }

    private static string GetPublishConfiguration(IEnumerable<InternalAssetInfo> assets, bool includePath)
    {
        var document = new XmlDocument();
        var root = document.CreateElement("assets");

        foreach (var asset in assets.GroupBy(x => x.Type).SelectMany(g => g.ToList()))
        {
            var assetXml = document.CreateElement("asset");
            assetXml.SetAttribute("name", asset.Name);
            assetXml.SetAttribute("version", asset.Version.ToString());
            assetXml.SetAttribute("type", Enum.GetName(asset.Type));
            assetXml.SetAttribute("language", Enum.GetName(asset.Locale));
            assetXml.SetAttribute("size", asset.BundleSize.ToString());

            if (includePath)
            {
                var pathXml = document.CreateElement("path");
                pathXml.InnerText = asset.Path;
                assetXml.AppendChild(pathXml);
            }

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
            else if (pair.Key != default)
            {
                pair.Value.Add(treeTxt[1..]);
            }
        }

        return info.Select(i => new TreeInfo(i.Key, GetTree(i.Value))).ToArray();
    }
}
