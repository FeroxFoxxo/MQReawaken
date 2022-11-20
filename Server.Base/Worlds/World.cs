using Microsoft.Extensions.Logging;
using Server.Base.Core.Helpers;
using Server.Base.Network.Services;
using Server.Base.Worlds.Events;
using System.Diagnostics;

namespace Server.Base.Worlds;

public class World
{
    private readonly ManualResetEvent _diskWriteHandle;

    private readonly NetStateHandler _handler;
    private readonly ILogger<World> _logger;
    private readonly EventSink _sink;

    public bool Saving { get; private set; }
    public bool Loaded { get; private set; }
    public bool Loading { get; private set; }

    public World(ILogger<World> logger, EventSink sink, NetStateHandler handler)
    {
        _logger = logger;
        _sink = sink;
        _handler = handler;

        Saving = false;
        Loaded = false;
        Loading = false;

        _diskWriteHandle = new ManualResetEvent(true);
    }

    public void Load()
    {
        if (Loaded)
            return;

        Loaded = true;

        _logger.LogInformation("Loading...");

        var stopWatch = Stopwatch.StartNew();

        Loading = true;

        try
        {
            _sink.InvokeWorldLoad();
        }
        catch (Exception ex)
        {
            _logger.LogError("FATAL: Exception in EventSink.WorldLoad", ex);
        }

        Loading = false;

        stopWatch.Stop();

        _logger.LogDebug("Finished loading in {SECONDS} seconds.", stopWatch.Elapsed.TotalSeconds);
    }

    public void WaitForWriteCompletion() => _diskWriteHandle.WaitOne();

    public void NotifyDiskWriteComplete()
    {
        if (_diskWriteHandle.Set())
            _logger.LogDebug("Closing Save Files.");
    }

    public void Save(bool message, bool permitBackgroundWrite)
    {
        if (Saving)
            return;

        _handler.Pause();

        WaitForWriteCompletion();

        Saving = true;

        _diskWriteHandle.Reset();

        _logger.LogInformation("Saving...");

        var stopWatch = Stopwatch.StartNew();

        try
        {
            _sink.InvokeWorldSave(new WorldSaveEventArgs(message));
        }
        catch (Exception ex)
        {
            _logger.LogError("FATAL: Exception in EventSink.WorldSave", ex);
        }

        stopWatch.Stop();

        Saving = false;

        if (!permitBackgroundWrite)
            NotifyDiskWriteComplete();

        _logger.LogDebug("Save finished in {SECONDS} seconds.", stopWatch.Elapsed.TotalSeconds);

        _handler.Resume();
    }

    public void Broadcast(string message)
    {
        _sink.InvokeWorldBroadcast(new WorldBroadcastEventArgs(message));
        _logger.LogInformation("{MESSAGE}", message);
    }
}
