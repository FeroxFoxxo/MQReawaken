using Server.Base.Core.Abstractions;
using Server.Base.Core.Helpers;
using Server.Reawakened.Data;
using Web.AssetBundles.XMLs;

namespace Server.Reawakened.Levels.Services;

public class LevelHandler : IService
{
    private readonly ServerConfig _config;
    private readonly Dictionary<int, Level> _levels;
    private readonly EventSink _sink;
    private readonly WorldGraph _worldGraph;

    public LevelHandler(EventSink sink, ServerConfig config, WorldGraph worldGraph)
    {
        _sink = sink;
        _config = config;
        _worldGraph = worldGraph;
        _levels = new Dictionary<int, Level>();
    }

    public void Initialize() => _sink.WorldLoad += LoadLevels;

    private void LoadLevels()
    {
        foreach (var level in _levels.Values.Where(level => level.LevelData.LevelId != -1))
            level.DumpPlayersToLobby();

        _levels.Clear();
    }

    public Level GetLevelFromId(int levelId)
    {
        if (_levels.ContainsKey(levelId))
            return _levels[levelId];

        var level = new Level(_worldGraph?.GetInfoLevel(levelId), _config, this);

        _levels.Add(levelId, level);

        return level;
    }
}
