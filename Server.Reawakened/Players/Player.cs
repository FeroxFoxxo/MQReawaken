using Microsoft.Extensions.Logging;
using Server.Base.Core.Models;
using Server.Base.Network;
using Server.Base.Network.Services;
using Server.Reawakened.Levels;
using Server.Reawakened.Players.Modals;

namespace Server.Reawakened.Players;

public class Player : INetStateData
{
    private Level _currentLevel;
    private int _playerId;

    public UserInfo UserInfo;

    public Player(UserInfo userInfo) => UserInfo = userInfo;

    public void JoinLevel(NetState state, Level level)
    {
        _currentLevel?.RemoveClient(_playerId);
        _currentLevel = level;
        _playerId = _currentLevel.AddClient(state);
    }

    public int GetLevelId() => _currentLevel != null ? _currentLevel.LevelData.LevelId : -1;

    public void RemovedState(NetState state, NetStateHandler handler,
        Microsoft.Extensions.Logging.ILogger logger)
    {
        if (_currentLevel != null)
        {
            var levelName = _currentLevel.LevelData.Name;

            if (!string.IsNullOrEmpty(levelName))
                logger.LogDebug("Dumped player with ID '{User}' from level '{Level}'", _playerId, levelName);
        }

        _currentLevel?.DumpPlayerToLobby(_playerId);
    }
}
