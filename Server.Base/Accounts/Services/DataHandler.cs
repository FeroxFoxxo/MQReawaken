using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using Server.Base.Core.Abstractions;
using Server.Base.Core.Helpers;
using Server.Base.Worlds.Events;

namespace Server.Base.Accounts.Services;

public abstract class DataHandler<T> : IService
{
    public readonly ILogger<T> Logger;
    public readonly EventSink Sink;

    public Dictionary<string, T> Data;

    protected DataHandler(EventSink sink, ILogger<T> logger)
    {
        Sink = sink;
        Logger = logger;
        Data = new Dictionary<string, T>();
    }

    public void Initialize()
    {
        Sink.WorldLoad += Load;
        Sink.WorldSave += Save;
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

            Logger.LogInformation("Loaded {Count} {Name}{Plural} to memory", count, typeof(T).Name,
                count != 1 ? "s" : "");
        }
        catch (Exception ex)
        {
            Logger.LogError(ex, "Could not deserialize save for {Type}.", typeof(T).Name);
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
