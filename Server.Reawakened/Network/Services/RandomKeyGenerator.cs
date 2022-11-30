using Server.Base.Core.Abstractions;
using Server.Base.Core.Helpers;
using Server.Base.Network;
using Server.Base.Network.Events;
using Server.Reawakened.Data.Modals;

namespace Server.Reawakened.Network.Services;

public class RandomKeyGenerator : IService
{
    private readonly ServerConfig _config;
    private readonly Dictionary<Type, Dictionary<string, string>> _keys;
    private readonly Random _random;
    private readonly EventSink _sink;

    public RandomKeyGenerator(Random random, EventSink sink, ServerConfig config)
    {
        _random = random;
        _sink = sink;
        _config = config;

        _keys = new Dictionary<Type, Dictionary<string, string>>();
    }

    public void Initialize()
    {
        _sink.NetStateAdded += AddedNetState;
        _sink.NetStateRemoved += RemovedNetState;
    }

    private Dictionary<string, string> CheckIfExists<T>()
    {
        if (!_keys.ContainsKey(typeof(T)))
            _keys.Add(typeof(T), new Dictionary<string, string>());
        return _keys[typeof(T)];
    }

    private void RemovedNetState(NetStateRemovedEventArgs @event)
    {
        var id = @event.State.ToString();

        var rKeys = CheckIfExists<NetState>();

        if (rKeys.ContainsKey(id))
            rKeys.Remove(id);
    }

    private void AddedNetState(NetStateAddedEventArgs @event)
    {
        var id = @event.State.ToString();

        var rKeys = CheckIfExists<NetState>();

        if (!rKeys.ContainsKey(id))
            rKeys.Add(id, GetRandomKey(_config.RandomKeyLength));
    }

    public string GetRandomKey<T>(string id)
    {
        var rKeys = CheckIfExists<T>();

        if (rKeys.ContainsKey(id))
            return rKeys[id];

        while (true)
        {
            var rKey = GetRandomKey(_config.RandomKeyLength);

            if (rKeys.ContainsValue(rKey))
                continue;

            return rKey;
        }
    }

    private string GetRandomKey(int length)
    {
        const string chars = "ABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789";
        return new string(Enumerable.Repeat(chars, length)
            .Select(s => s[_random.Next(s.Length)]).ToArray());
    }
}
