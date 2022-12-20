using System.Xml;
using Web.AssetBundles.Models;

namespace Web.AssetBundles.Extensions;

public static class ConvertAssetInfo
{
    public static XmlElement ToAssetXml(this InternalAssetInfo asset, string name, XmlDocument document)
    {
        var assetXml = document.CreateElement(name);

        assetXml.SetAttribute("name", asset.Name);
        assetXml.SetAttribute("version", asset.Version.ToString());
        assetXml.SetAttribute("language", Enum.GetName(asset.Locale)?.Replace('_', '-'));
        assetXml.SetAttribute("size", asset.BundleSize.ToString());

        return assetXml;
    }

    public static XmlElement ToPubXml(this InternalAssetInfo asset, string name, XmlDocument document)
    {
        var assetXml = asset.ToAssetXml(name, document);
        assetXml.SetAttribute("type", Enum.GetName(asset.Type));
        return assetXml;
    }

    public static XmlElement ToStoredXml(this InternalAssetInfo asset, string name, XmlDocument document)
    {
        var assetXml = asset.ToPubXml(name, document);

        var pathXml = document.CreateElement("path");
        pathXml.InnerText = asset.Path;

        assetXml.SetAttribute("unityVersion", asset.UnityVersion);

        assetXml.AppendChild(pathXml);
        return assetXml;
    }

    public static InternalAssetInfo XmlToAsset(this XmlElement assetElement) =>
        new ()
        {
            Name = assetElement.GetAttribute("name"),
            Version = Convert.ToInt32(assetElement.GetAttribute("version")),
            Type = Enum.Parse<AssetInfo.TypeAsset>(assetElement.GetAttribute("type")),
            Locale = Enum.Parse<RFC1766Locales.LanguageCodes>(assetElement.GetAttribute("language")
                .Replace('-', '_')),
            BundleSize = Convert.ToInt32(assetElement.GetAttribute("size")),
            Path = assetElement.ChildNodes.Cast<XmlNode>().FirstOrDefault(x => x.Name == "path").InnerText,
            UnityVersion = assetElement.GetAttribute("unityVersion")
        };
}
