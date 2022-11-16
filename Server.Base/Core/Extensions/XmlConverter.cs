using System.Xml;

namespace Server.Base.Core.Extensions;

public static class XmlConverter
{
    public static string GetText(this XmlElement node, string defaultValue) =>
        node == null ? defaultValue : node.InnerText;

    public static string GetAttribute(this XmlElement node, string attributeName, string defaultValue)
    {
        if (node == null)
            return defaultValue;

        var attribute = node.Attributes[attributeName];

        return attribute == null ? defaultValue : attribute.Value;
    }

    public static int GetXmlInt32(string intString, int defaultValue)
    {
        try
        {
            return XmlConvert.ToInt32(intString);
        }
        catch
        {
            return int.TryParse(intString, out var val) ? val : defaultValue;
        }
    }

    public static DateTime GetXmlDateTime(string dateTimeString, DateTime defaultValue)
    {
        try
        {
            return XmlConvert.ToDateTime(dateTimeString, XmlDateTimeSerializationMode.Utc);
        }
        catch
        {
            return DateTime.TryParse(dateTimeString, out var dateTime) ? dateTime : defaultValue;
        }
    }
}
