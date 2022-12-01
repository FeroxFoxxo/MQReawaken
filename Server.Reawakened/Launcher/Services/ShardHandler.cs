using Server.Base.Core.Abstractions;
using Server.Base.Core.Helpers;
using Server.Base.Core.Models;

namespace Server.Reawakened.Launcher.Services;

public class ShardHandler : IService
{
    private readonly EventSink _sink;
    private readonly Dictionary<string, List<JsonData>> _data;

    public ShardHandler(EventSink sink)
    {
        _sink = sink;
        _data = new Dictionary<string, List<JsonData>>();
    }

    public void Initialize() => _sink.ServerStarted += _ => _data.Clear();

    public T GetData<T>(string id) where T : JsonData
    {
        EnsureDataExists(id);
        return _data[id].FirstOrDefault(x => x.GetType() == typeof(T)) as T;
    }

    public void AddData(JsonData data, string id)
    {
        EnsureDataExists(id);
        if (_data[id].All(x => x.GetType() != data.GetType()))
            _data[id].Add(data);
    }

    public void EnsureDataExists(string id)
    {
        if (!_data.ContainsKey(id))
            _data.Add(id, new List<JsonData>());
    }
}
