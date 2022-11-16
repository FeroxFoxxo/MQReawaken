using Server.Base.Core.Abstractions;
using Server.Base.Core.Helpers;
using Server.Base.Core.Models;
using Server.Base.Network;
using Server.Base.Network.Events;

namespace Server.Reawakened.Network.Services;

public class RandomKeyGenerator : IService
{
    private readonly ServerConfig _config;
    private readonly Dictionary<string, string> _keys;
    private readonly Random _random;
    private readonly EventSink _sink;

    public RandomKeyGenerator(Random random, EventSink sink, ServerConfig config)
    {
        _random = random;
        _sink = sink;
        _config = config;

        _keys = new Dictionary<string, string>();
    }

    public void Initialize()
    {
        _sink.NetStateAdded += AddedNetState;
        _sink.NetStateRemoved += RemovedNetState;
    }

    private void RemovedNetState(NetStateRemovedEventArgs @event)
    {
        var id = @event.State.ToString();

        if (_keys.ContainsKey(id))
            _keys.Remove(id);
    }

    private void AddedNetState(NetStateAddedEventArgs @event)
    {
        var id = @event.State.ToString();

        if (!_keys.ContainsKey(id))
            _keys.Add(id, GetRandomKey(_config.RandomKeyLength));
    }

    public string GetRandomKey(NetState state)
    {
        var id = state.ToString();

        return _keys.ContainsKey(id) ? _keys[id] : GetRandomKey(_config.RandomKeyLength);
    }

    private string GetRandomKey(int length)
    {
        const string chars = "ABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789";
        return new string(Enumerable.Repeat(chars, length)
            .Select(s => s[_random.Next(s.Length)]).ToArray());
    }
}
