using Microsoft.Extensions.Logging;
using Server.Base.Core.Abstractions;
using Server.Base.Core.Extensions;
using Server.Base.Core.Helpers;
using Server.Base.Core.Models;
using Server.Base.Core.Services;
using Server.Base.Timers.Extensions;
using Server.Base.Timers.Services;

namespace Server.Base.Worlds.Services;

public class AutoSave : IService
{
    private readonly ArchivedSaves _archives;

    private readonly string[] _backups;
    private readonly TimeSpan _delayAutoSave;
    private readonly TimeSpan _delayWarning;

    private readonly ServerHandler _handler;
    private readonly ILogger<AutoSave> _logger;
    private readonly EventSink _sink;
    private readonly TimerThread _timerThread;
    private readonly World _world;

    public AutoSave(InternalServerConfig config, ILogger<AutoSave> logger, ServerHandler handler, World world,
        ArchivedSaves archives,
        EventSink sink, TimerThread timerThread)
    {
        _logger = logger;
        _handler = handler;
        _world = world;
        _archives = archives;
        _sink = sink;
        _timerThread = timerThread;

        _delayAutoSave = TimeSpan.FromMinutes(config.SaveAutomaticallyMinutes);
        _delayWarning = TimeSpan.FromMinutes(config.SaveWarningMinutes);
        _backups = config.Backups;
    }

    public void Initialize() => _sink.ServerStarted += _ => RunAutoSaveTimer();

    private void RunAutoSaveTimer()
    {
        var timer = _timerThread.DelayCall(Tick, _delayAutoSave - _delayWarning, _delayAutoSave, 0);
        timer.Stop();
    }

    public void Save()
    {
        if (_handler.Restarting)
            return;

        _world.WaitForWriteCompletion();

        try
        {
            if (!Backup())
                _logger.LogError("Automatic backup failed: backup returned false");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Automatic backup failed");
        }

        _world.Save(true, false);
    }

    private static DirectoryInfo Match(IEnumerable<string> paths, string match) =>
        paths.Select(t => new DirectoryInfo(t))
            .FirstOrDefault(directoryInfo => directoryInfo.Name.StartsWith(match));

    private bool Backup()
    {
        if (_backups.Length == 0)
            return false;

        var root = Path.Combine(InternalDirectory.GetBaseDirectory(), "Backups/Automatic");

        if (!Directory.Exists(root))
            Directory.CreateDirectory(root);

        var tempRoot = Path.Combine(InternalDirectory.GetBaseDirectory(), "Backups/Temp");

        if (Directory.Exists(tempRoot))
            Directory.Delete(tempRoot, true);

        var existing = Directory.GetDirectories(root);

        var anySuccess = existing.Length == 0;

        for (var i = 0; i < _backups.Length; ++i)
        {
            var directoryInfo = Match(existing, _backups[i]);

            if (directoryInfo == null)
                continue;

            if (i > 0)
            {
                try
                {
                    directoryInfo.MoveTo(Path.Combine(root, _backups[i - 1]));

                    anySuccess = true;
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Could not move directory");
                }
            }
            else
            {
                var delete = true;

                try
                {
                    directoryInfo.MoveTo(tempRoot);

                    delete = !_archives.Process(tempRoot);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Could not move directory");
                }

                if (!delete)
                    continue;

                try
                {
                    directoryInfo.Delete(true);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Could not delete directory");
                }
            }
        }

        var saves = Path.Combine(InternalDirectory.GetBaseDirectory(), "Saves");

        if (Directory.Exists(saves))
            Directory.Move(saves, Path.Combine(root, _backups[^1]));

        return anySuccess;
    }

    private void Tick()
    {
        if (_handler.Restarting)
            return;

        if (_delayWarning == TimeSpan.Zero)
        {
            Save();
        }
        else
        {
            var seconds = (int)_delayWarning.TotalSeconds;
            var minutes = seconds / 60;
            seconds %= 60;

            switch (minutes)
            {
                case > 0 when seconds > 0:
                    _world.Broadcast(
                        $"The world will save in {minutes} minute{(minutes != 1 ? "s" : "")} and {seconds} second{(seconds != 1 ? "s" : "")}.");
                    break;
                case > 0:
                    _world.Broadcast($"The world will save in {minutes} minute{(minutes != 1 ? "s" : "")}.");
                    break;
                default:
                    _world.Broadcast($"The world will save in {seconds} second{(seconds != 1 ? "s" : "")}.");
                    break;
            }

            _timerThread.DelayCall(Save, _delayWarning, TimeSpan.Zero, 1);
        }
    }
}
