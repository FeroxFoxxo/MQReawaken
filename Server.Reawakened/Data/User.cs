using Server.Base.Network;
using Server.Reawakened.Data.Modals;
using Server.Reawakened.Levels;

namespace Server.Reawakened.Data;

public class User
{
    private Level _currentLevel;
    private int _playerId;

    public UserInfo UserInfo;

    public User(UserInfo userInfo) => UserInfo = userInfo;

    public void JoinLevel(NetState state, Level level)
    {
        _currentLevel?.RemoveClient(_playerId);
        _currentLevel = level;
        _playerId = _currentLevel.AddClient(state);
    }

    public int GetLevelId() => _currentLevel != null ? _currentLevel.LevelData.LevelId : -1;
}
