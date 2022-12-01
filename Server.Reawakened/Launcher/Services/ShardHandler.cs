using Server.Base.Core.Abstractions;
using Server.Base.Core.Helpers;
using Server.Base.Core.Models;

namespace Server.Reawakened.Launcher.Services;

public class ShardHandler : IService
{
    private readonly EventSink _sink;
    private readonly Dictionary<string, List<PersistantData>> _data;

    public ShardHandler(EventSink sink)
    {
        _sink = sink;
        _data = new Dictionary<string, List<PersistantData>>();
    }

    public void Initialize() => _sink.ServerStarted += _ => _data.Clear();

    public T GetData<T>(string id) where T : PersistantData
    {
        EnsureDataExists(id);
        return _data[id].FirstOrDefault(x => x.GetType() == typeof(T)) as T;
    }

    public void AddData(PersistantData data, string id)
    {
        EnsureDataExists(id);
        if (_data[id].All(x => x.GetType() != data.GetType()))
            _data[id].Add(data);
    }

    public void EnsureDataExists(string id)
    {
        if (!_data.ContainsKey(id))
            _data.Add(id, new List<PersistantData>());
    }
}
