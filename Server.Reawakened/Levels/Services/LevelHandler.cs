using Microsoft.Extensions.Logging;
using Server.Base.Core.Abstractions;
using Server.Base.Core.Helpers;
using Server.Reawakened.Data;
using Server.Reawakened.XMLs;
using System.Runtime.Serialization;

namespace Server.Reawakened.Levels.Services;

public class LevelHandler : IService
{
    private readonly ServerConfig _config;
    private readonly Dictionary<int, Level> _levels;
    private readonly ILogger<LevelHandler> _logger;
    private readonly EventSink _sink;
    private readonly WorldGraph _worldGraph;

    public LevelHandler(ILogger<LevelHandler> logger, EventSink sink, ServerConfig config)
    {
        _logger = logger;
        _sink = sink;
        _config = config;
        _levels = new Dictionary<int, Level>();
        _worldGraph = FormatterServices.GetUninitializedObject(typeof(WorldGraph)) as WorldGraph;

        if (_worldGraph is null)
            logger.LogError("World graph was unable to initialize!");
    }

    public void Initialize() => _sink.WorldLoad += LoadLevels;

    private void LoadLevels()
    {
        foreach (var level in _levels.Values.Where(level => level.LevelData.LevelId != -1))
            level.DumpPlayersToLobby();

        _levels.Clear();
        _worldGraph?.SetLogger(_logger);
        _worldGraph?.Load();
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
