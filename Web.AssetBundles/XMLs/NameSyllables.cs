using System.Reflection;
using Web.AssetBundles.Abstractions;

namespace Web.AssetBundles.XMLs;

public class NameSyllables : NamegenSyllablesXML, IBundledXml
{
    public Dictionary<bool, List<List<string>>> Syllables;

    public void LoadBundle(string xml)
    {
        Syllables = new Dictionary<bool, List<List<string>>>();

        var wGType = typeof(NamegenSyllablesXML);

        var field = wGType.GetField("_nameSyllables", BindingFlags.NonPublic | BindingFlags.Instance);
        field?.SetValue(this, new Dictionary<int, List<string>>());

        ReadDescriptionXml(xml);

        Syllables = new Dictionary<bool, List<List<string>>>
        {
            {
                false,
                new List<List<string>>
                {
                    GetSyllables(0, false),
                    GetSyllables(1, false),
                    GetSyllables(2, false)
                }
            },
            {
                true,
                new List<List<string>>
                {
                    GetSyllables(0, true),
                    GetSyllables(1, true),
                    GetSyllables(2, true)
                }
            }
        };
    }
}
