using System.Diagnostics;
using Server.Base.Core.Helpers;
using Server.Base.Logging;
using Server.Base.Network.Services;
using Server.Base.Worlds.Events;

namespace Server.Base.Worlds;

public class World
{
    private readonly ManualResetEvent _diskWriteHandle;

    private readonly NetStateHandler _handler;
    private readonly Logger _logger;
    private readonly EventSink _sink;

    public bool Saving { get; private set; }
    public bool Loaded { get; private set; }
    public bool Loading { get; private set; }

    public World(Logger logger, EventSink sink, NetStateHandler handler)
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

        _logger.WriteLine<World>(ConsoleColor.Green, "Loading...");

        var stopWatch = Stopwatch.StartNew();

        Loading = true;

        _sink.InvokeWorldLoad();

        Loading = false;

        stopWatch.Stop();

        _logger.WriteLine<World>(ConsoleColor.Green, $"Finished loading in {stopWatch.Elapsed.TotalSeconds} seconds.");
    }

    public void WaitForWriteCompletion() => _diskWriteHandle.WaitOne();

    public void NotifyDiskWriteComplete()
    {
        if (_diskWriteHandle.Set())
            _logger.WriteLine<World>(ConsoleColor.Green, "Closing Save Files.");
    }

    public void Save(bool message, bool permitBackgroundWrite)
    {
        if (Saving)
            return;

        _handler.Pause();

        WaitForWriteCompletion();

        Saving = true;

        _diskWriteHandle.Reset();

        _logger.WriteLine<World>(ConsoleColor.Green, "Saving...");

        var stopWatch = Stopwatch.StartNew();

        try
        {
            _sink.InvokeWorldSave(new WorldSaveEventArgs(message));
        }
        catch (Exception e)
        {
            throw new Exception("FATAL: Exception in EventSink.WorldSave", e);
        }

        stopWatch.Stop();

        Saving = false;

        if (!permitBackgroundWrite)
            NotifyDiskWriteComplete();

        _logger.WriteLine<World>(ConsoleColor.Green, $"Save finished in {stopWatch.Elapsed.TotalSeconds} seconds.");

        _handler.Resume();
    }

    public void Broadcast(string message)
    {
        _sink.InvokeWorldBroadcast(new WorldBroadcastEventArgs(message));
        _logger.WriteLine<World>(ConsoleColor.Green, message);
    }
}
