using Newtonsoft.Json;
using Server.Base.Core.Abstractions;
using Server.Base.Core.Helpers;
using Server.Base.Logging;
using Server.Base.Worlds.Events;

namespace Server.Base.Accounts.Services;

public abstract class DataHandler<T> : IService
{
    private readonly Logger _logger;
    private readonly EventSink _sink;

    public Dictionary<string, T> Data;

    protected DataHandler(EventSink sink, Logger logger)
    {
        _sink = sink;
        _logger = logger;
        Data = new Dictionary<string, T>();
    }

    public void Initialize()
    {
        _sink.WorldLoad += Load;
        _sink.WorldSave += Save;
    }

    public string GetFileName() => Path.Combine("Saves", $"{typeof(T).Name.ToLower()}.json");

    public void Load()
    {
        var filePath = GetFileName();

        if (!File.Exists(filePath))
            return;

        using StreamReader streamReader = new(filePath, false);
        var contents = streamReader.ReadToEnd();

        try
        {
            Data = JsonConvert.DeserializeObject<Dictionary<string, T>>(contents) ??
                   throw new InvalidOperationException();

            var count = Data.Count;

            _logger.WriteLine<T>(ConsoleColor.Green,
                $"{GetType().Name}: Loaded {count} {typeof(T).Name}{(count != 1 ? "s" : "")} to memory.");
        }
        catch (Exception exception)
        {
            _logger.LogException<T>(exception);
        }

        streamReader.Close();

        OnAfterLoad();
    }

    public virtual void OnAfterLoad()
    {
    }

    public void Save(WorldSaveEventArgs worldSaveEventArgs)
    {
        if (!Directory.Exists("Saves"))
            Directory.CreateDirectory("Saves");

        var filePath = GetFileName();

        using StreamWriter streamWriter = new(filePath, false);

        streamWriter.Write(JsonConvert.SerializeObject(Data, Formatting.Indented));

        streamWriter.Close();
    }

    public T Get(string username)
    {
        Data.TryGetValue(username, out var type);

        return type;
    }
}
