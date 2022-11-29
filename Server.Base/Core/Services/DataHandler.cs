using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using Server.Base.Core.Abstractions;
using Server.Base.Core.Helpers;
using Server.Base.Worlds.Events;

namespace Server.Base.Core.Services;

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

    public virtual void Initialize()
    {
        Sink.WorldLoad += Load;
        Sink.WorldSave += Save;
    }

    public string GetFileName() => Path.Combine("Saves", $"{typeof(T).Name.ToLower()}.json");

    public void Load()
    {
        try
        {
            var filePath = GetFileName();

            if (File.Exists(filePath))
            {
                using StreamReader streamReader = new(filePath, false);
                var contents = streamReader.ReadToEnd();

                Data = JsonConvert.DeserializeObject<Dictionary<string, T>>(contents) ??
                       throw new InvalidOperationException();

                var count = Data.Count;

                Logger.LogInformation("Loaded {Count} {Name}{Plural} to memory", count, typeof(T).Name,
                    count != 1 ? "s" : "");

                streamReader.Close();
            }
            else
            {
                Logger.LogWarning("Could not find save file for {FileName}, generating default.", filePath);
            }
        }
        catch (Exception ex)
        {
            Logger.LogError(ex, "Could not deserialize save for {Type}.", typeof(T).Name);
        }

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
