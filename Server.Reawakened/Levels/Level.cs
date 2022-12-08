using Server.Base.Network;
using Server.Reawakened.Data;
using Server.Reawakened.Levels.Enums;
using Server.Reawakened.Levels.Extensions;
using Server.Reawakened.Levels.Services;
using Server.Reawakened.Network.Extensions;
using Server.Reawakened.Players;
using WorldGraphDefines;

namespace Server.Reawakened.Levels;

public class Level
{
    private readonly HashSet<int> _clientIds;
    private readonly Dictionary<int, NetState> _clients;
    private readonly LevelHandler _handler;
    private readonly ServerConfig _serverConfig;

    public readonly LevelInfo LevelData;

    public Level(LevelInfo levelData, ServerConfig serverConfig, LevelHandler handler)
    {
        LevelData = levelData;
        _serverConfig = serverConfig;
        _handler = handler;
        _clients = new Dictionary<int, NetState>();
        _clientIds = new HashSet<int>();
    }

    public int AddClient(NetState state)
    {
        var playerId = -1;
        JoinReason reason;

        if (_clientIds.Count > _serverConfig.PlayerCap)
        {
            reason = JoinReason.Full;
        }
        else
        {
            playerId = 1;

            while (_clientIds.Contains(playerId))
                playerId++;

            _clients.Add(playerId, state);
            _clientIds.Add(playerId);
            reason = JoinReason.Accepted;
        }

        if (reason == JoinReason.Accepted)
            state.SendXml("joinOK", $"<pid id='{playerId}' /><uLs />");
        else
            state.SendXml("joinKO", $"<error>{reason.GetErrorValue()}</error>");
        return playerId;
    }

    public void DumpPlayersToLobby()
    {
        foreach (var playerId in _clients.Keys)
            DumpPlayerToLobby(playerId);
    }

    public void DumpPlayerToLobby(int playerId)
    {
        var client = _clients[playerId];

        client.Get<Player>().JoinLevel(client, _handler.GetLevelFromId(-1));

        RemoveClient(playerId);
    }

    public void RemoveClient(int playerId)
    {
        _clients.Remove(playerId);
        _clientIds.Remove(playerId);
    }
}
