using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using Server.Base.Core.Abstractions;
using Server.Base.Core.Extensions;
using Server.Base.Core.Helpers;
using Server.Base.Core.Models;
using Server.Base.Network;
using Server.Base.Worlds.Events;

namespace Server.Base.Core.Services;

public abstract class DataHandler<T> : IService where T : PersistantData
{
    public readonly ILogger<T> Logger;
    public readonly EventSink Sink;

    public Dictionary<int, T> Data;

    protected DataHandler(EventSink sink, ILogger<T> logger)
    {
        Sink = sink;
        Logger = logger;
        Data = new Dictionary<int, T>();
    }

    public virtual void Initialize()
    {
        Sink.WorldLoad += Load;
        Sink.WorldSave += Save;
    }

    public string GetDirectory() =>
        Path.Combine(InternalDirectory.GetBaseDirectory(), "Saves");

    public string GetFileName() =>
        Path.Combine(GetDirectory(), $"{typeof(T).Name.ToLower()}.json");

    public void Load()
    {
        try
        {
            var filePath = GetFileName();

            if (File.Exists(filePath))
            {
                using StreamReader streamReader = new(filePath, false);
                var contents = streamReader.ReadToEnd();

                Data = JsonConvert.DeserializeObject<Dictionary<int, T>>(contents) ??
                       throw new InvalidOperationException();

                var count = Data.Count;

                Logger.LogDebug("Loaded {Count} {Name}{Plural} to memory from {Directory}", count,
                    typeof(T).Name.ToLower(), count != 1 ? "s" : "", filePath);

                streamReader.Close();

                if (Data.Count <= 0)
                    CreateDefaultInternal();
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

    private void CreateDefaultInternal()
    {
        var name = typeof(T).Name.ToLower();

        Logger.LogInformation("This server does not have a(n) {Type}.", name);
        Logger.LogInformation("Please create a(n) {Type} for the server owner now", name);

        var t = CreateDefault();

        if (t == null)
        {
            Logger.LogError("Failed to create {Type}.", name);
        }
        else
        {
            Data.Add(Data.Count, t);
            Logger.LogInformation("Created {Name}.", name);
        }
    }

    public abstract T CreateDefault();

    public virtual void OnAfterLoad()
    {
    }

    public void Save(WorldSaveEventArgs worldSaveEventArgs)
    {
        if (!Directory.Exists(GetDirectory()))
            Directory.CreateDirectory(GetDirectory());

        var filePath = GetFileName();

        using StreamWriter streamWriter = new(filePath, false);

        streamWriter.Write(JsonConvert.SerializeObject(Data, Formatting.Indented));

        streamWriter.Close();
    }

    public T Get(int userId)
    {
        Data.TryGetValue(userId, out var type);

        return type;
    }

    public abstract T Create(NetState netState, params string[] obj);
}
