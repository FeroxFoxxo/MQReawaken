using Microsoft.Extensions.Logging;
using Server.Base.Core.Abstractions;
using Server.Base.Core.Helpers;
using Server.Base.Core.Models;
using Server.Base.Core.Services;
using Server.Base.Timers.Extensions;
using Server.Base.Timers.Services;
using Timer = Server.Base.Timers.Timer;

namespace Server.Base.Worlds.Services;

public class AutoRestart : Timer, IService
{
    private readonly AutoSave _autoSave;

    private readonly TimeSpan _delayAutoRestart;
    private readonly TimeSpan _delayRestart;
    private readonly TimeSpan _delayWarning;

    private readonly ServerHandler _handler;
    private readonly ILogger<AutoRestart> _logger;
    private readonly EventSink _sink;
    private readonly TimerThread _timerThread;
    private readonly World _world;

    public bool DoneWarning;

    public DateTime RestartTime;

    public AutoRestart(ILogger<AutoRestart> logger, TimerThread timerThread, InternalServerConfig config,
        ServerHandler handler, World world, AutoSave autoSave, EventSink sink)
        : base(TimeSpan.FromSeconds(1.0), TimeSpan.FromSeconds(1.0), 0, timerThread)
    {
        _logger = logger;
        _timerThread = timerThread;
        _handler = handler;
        _world = world;
        _autoSave = autoSave;
        _sink = sink;
        _delayRestart = TimeSpan.FromSeconds(config.RestartDelaySeconds);
        _delayWarning = TimeSpan.FromSeconds(config.RestartWarningSeconds);
        _delayAutoRestart = TimeSpan.FromHours(config.RestartAutomaticallyHours);

        var now = DateTime.Now;
        DateTime force = new(now.Year, now.Month, now.Day, 12, 0, 0);

        if (now > force)
            force += _delayAutoRestart;

        RestartTime = force;
    }

    public void Initialize() => _sink.ServerStarted += ServerStarted;

    private void ServerStarted()
    {
        _logger.LogDebug("Configured for {RestartHour}:{RestartMinute}:00, every {TotalHours} hours!",
            RestartTime.Hour, RestartTime.Minute, _delayAutoRestart.TotalHours);

        _logger.LogDebug("Next Restart: {RestartTime}", RestartTime);

        Start();
    }

    public override void OnTick()
    {
        if (_handler.Restarting)
            return;

        if (_delayWarning > TimeSpan.Zero && !DoneWarning && RestartTime - _delayWarning < DateTime.Now)
        {
            _world.Broadcast($"The server will be going down in about {_delayWarning.TotalMinutes} " +
                             $"minute{(Convert.ToInt32(_delayWarning.TotalMinutes) == 1 ? "" : "s")}.");

            DoneWarning = true;
            return;
        }

        if (DateTime.Now < RestartTime)
            return;

        _autoSave.Save();
        _handler.Restarting = true;

        TimedShutdown(true);
    }

    private void TimedShutdown(bool restart)
    {
        _world.Broadcast($"The server will be going down in about {_delayRestart.TotalSeconds} seconds!");

        _timerThread.DelayCall(() => _handler.KillServer(restart), _delayRestart, TimeSpan.Zero, 1);
    }
}
