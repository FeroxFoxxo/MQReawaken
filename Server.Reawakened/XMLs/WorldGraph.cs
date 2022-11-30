using Microsoft.Extensions.Logging;
using Server.Reawakened.Levels.Services;
using System.Reflection;
using WorldGraphDefines;

namespace Server.Reawakened.XMLs;

public class WorldGraph : WorldGraphXML
{
    private ILogger<LevelHandler> _logger;

    public void SetLogger(ILogger<LevelHandler> logger) => _logger = logger;

    public void Load()
    {
        _rootXmlName = "world_graph";
        _hasLocalizationDict = false;

        var wGType = typeof(WorldGraphXML);

        var field = wGType.GetField("_worldGraphNodes", BindingFlags.NonPublic | BindingFlags.Instance);
        field?.SetValue(this, new Dictionary<int, List<DestNode>>());
        field = wGType.GetField("_levelNameToID", BindingFlags.NonPublic | BindingFlags.Instance);
        field?.SetValue(this, new Dictionary<string, int>());
        field = wGType.GetField("_levelInfos", BindingFlags.NonPublic | BindingFlags.Instance);
        field?.SetValue(this, new Dictionary<int, LevelInfo>());

        try
        {
            ReadDescriptionXml(File.ReadAllText("XMLs/WorldGraph.xml"));
        }
        catch
        {
            _logger.LogWarning("{Name} could not load! Skipping...", GetType().Name);
        }
    }
}
