using System.Xml;
using Web.AssetBundles.Models;

namespace Web.AssetBundles.Extensions;

public static class BuildAssetToXml
{
    public static XmlElement ToXml(this InternalAssetInfo asset, string name, XmlDocument document)
    {
        var assetXml = document.CreateElement(name);

        assetXml.SetAttribute("name", asset.Name);
        assetXml.SetAttribute("version", asset.Version.ToString());
        assetXml.SetAttribute("language", Enum.GetName(asset.Locale)?.Replace('_', '-'));
        assetXml.SetAttribute("size", asset.BundleSize.ToString());

        return assetXml;
    }

    public static XmlElement ToXmlWithType(this InternalAssetInfo asset, string name, XmlDocument document)
    {
        var assetXml = asset.ToXml(name, document);
        assetXml.SetAttribute("type", Enum.GetName(asset.Type));
        return assetXml;
    }

    public static XmlElement ToXmlWithTypePath(this InternalAssetInfo asset, string name, XmlDocument document)
    {
        var assetXml = asset.ToXmlWithType(name, document);
        var pathXml = document.CreateElement("path");
        pathXml.InnerText = asset.Path;
        assetXml.AppendChild(pathXml);
        return assetXml;
    }
}
