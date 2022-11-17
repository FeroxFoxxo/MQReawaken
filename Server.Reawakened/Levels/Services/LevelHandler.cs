using System.Runtime.Serialization;
using Server.Base.Core.Abstractions;
using Server.Base.Core.Helpers;
using Server.Base.Logging;
using Server.Reawakened.Data.Modals;
using Server.Reawakened.Data.Services;
using Server.Reawakened.XMLs;

namespace Server.Reawakened.Levels.Services;

public class LevelHandler : IService
{
    private readonly ServerConfig _config;
    private readonly UserHandler _handler;
    private readonly Dictionary<int, Level> _levels;
    private readonly EventSink _sink;
    private readonly WorldGraph _worldGraph;

    public LevelHandler(Logger logger, EventSink sink, ServerConfig config, UserHandler handler)
    {
        _sink = sink;
        _config = config;
        _handler = handler;
        _levels = new Dictionary<int, Level>();
        _worldGraph = FormatterServices.GetUninitializedObject(typeof(WorldGraph)) as WorldGraph;

        if (_worldGraph is null)
            logger.WriteLine<LevelHandler>(ConsoleColor.Red, "World graph was unable to initialize!");
    }

    public void Initialize() => _sink.WorldLoad += LoadLevels;

    private void LoadLevels()
    {
        foreach (var level in _levels.Values.Where(level => level.LevelData.LevelId != -1))
            level.DumpPlayersToLobby();

        _levels.Clear();
        _worldGraph?.Load();
    }

    public Level GetLevelFromId(int levelId)
    {
        if (_levels.ContainsKey(levelId))
            return _levels[levelId];

        var level = new Level(_worldGraph?.GetInfoLevel(levelId), _config, this, _handler);

        if (_worldGraph != null)
            _levels.Add(levelId, level);

        return level;
    }
}
