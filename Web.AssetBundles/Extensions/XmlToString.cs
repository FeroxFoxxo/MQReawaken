using System.Xml;

namespace Web.AssetBundles.Extensions;

public static class XmlToString
{
    public static string WriteToString(this XmlDocument document, bool omitXml)
    {
        using var stringWriter = new StringWriter();

        using var xmlTextWriter = XmlWriter.Create(stringWriter,
            new XmlWriterSettings { Indent = true, OmitXmlDeclaration = omitXml });

        document.WriteTo(xmlTextWriter);
        xmlTextWriter.Flush();

        return stringWriter.GetStringBuilder().ToString();
    }
}
