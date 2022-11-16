using System.Collections;
using System.Diagnostics;
using System.IO.Compression;
using Server.Base.Core.Abstractions;
using Server.Base.Core.Extensions;
using Server.Base.Core.Helpers;
using Server.Base.Core.Models;
using Server.Base.Core.Services;
using Server.Base.Logging;
using Server.Base.Timers.Enums;
using Server.Base.Worlds.Events;

namespace Server.Base.Worlds.Services;

public class ArchivedSaves : IService
{
    private readonly string _defaultDestination;

    private readonly EventSink _eventSink;
    private readonly Logger _logger;

    private readonly Action<string> _pack;
    private readonly Action<DateTime> _prune;
    private readonly ServerHandler _serverHandler;

    private readonly AutoResetEvent _sync;
    private readonly object _taskRoot;
    private readonly List<IAsyncResult> _tasks;
    public TimeSpan ExpireAge;
    public MergeType Merge;

    private ArchivedSaves(Logger logger, EventSink eventSink, ServerHandler serverHandler, ServerConfig config)
    {
        _logger = logger;
        _eventSink = eventSink;
        _serverHandler = serverHandler;

        _defaultDestination = Path.Combine(InternalDirectory.GetBaseDirectory(), "Backups", "Archived");
        ExpireAge = TimeSpan.Zero;
        Merge = MergeType.Minutes;
        _sync = new AutoResetEvent(true);
        _tasks = new List<IAsyncResult>(config.BackupCapacity);
        _taskRoot = ((ICollection)_tasks).SyncRoot;

        _pack = InternalPack;
        _prune = InternalPrune;
    }

    public void Initialize()
    {
        _eventSink.Shutdown += Wait;
        _eventSink.WorldSave += Wait;
    }

    public int GetPendingTasks()
    {
        lock (_taskRoot)
            return _tasks.Count - _tasks.RemoveAll(task => task.IsCompleted);
    }

    private void Wait(WorldSaveEventArgs worldSaveEventArgs) => WaitForTaskCompletion();

    private void Wait() => WaitForTaskCompletion();

    private void WaitForTaskCompletion()
    {
        if (!_serverHandler.HasCrashed && !_serverHandler.IsClosing)
            return;

        var pending = GetPendingTasks();

        if (pending <= 0)
            return;

        _logger.WriteLine(ConsoleColor.Cyan, $"Archives: Waiting for {pending} pending tasks...");

        while (GetPendingTasks() > 0)
            _sync.WaitOne(10);

        _logger.WriteLine(ConsoleColor.Cyan, "Archives: All tasks completed.");
    }

    private void InternalPrune(DateTime threshold)
    {
        if (!Directory.Exists(_defaultDestination))
            return;

        _logger.WriteLine(ConsoleColor.Cyan, "Archives: Pruning started...");

        var stopwatch = Stopwatch.StartNew();

        try
        {
            DirectoryInfo root = new(_defaultDestination);

            foreach (var archive in root.GetFiles("*.zip", SearchOption.AllDirectories))
            {
                try
                {
                    if (archive.LastWriteTimeUtc < threshold)
                        archive.Delete();
                }
                catch (Exception exception)
                {
                    _logger.LogException(exception);
                }
            }
        }
        catch (Exception exception)
        {
            _logger.LogException(exception);
        }

        stopwatch.Stop();

        _logger.WriteLine(ConsoleColor.Cyan, $"Archives: Pruning done in {stopwatch.Elapsed.TotalSeconds} seconds.");
    }

    private void InternalPack(string source)
    {
        _logger.WriteLine(ConsoleColor.Cyan, "Archives: Packing started...");

        var stopwatch = Stopwatch.StartNew();

        try
        {
            var now = DateTime.Now;

            var amOrPm = now.Hour < 12 ? "AM" : "PM";
            var twelveHours = now.Hour > 12 ? now.Hour - 12 : now.Hour <= 0 ? 12 : now.Hour;

            var date = Merge switch
            {
                MergeType.Months => $"{now.Month}-{now.Year}",
                MergeType.Days => $"{now.Day}-{now.Month}-{now.Year}",
                MergeType.Hours => $"{now.Day}-{now.Month}-{now.Year} {twelveHours:D2} {amOrPm}",
                _ => $"{now.Day}-{now.Month}-{now.Year} {twelveHours:D2}-{now.Minute:D2} {amOrPm}"
            };

            var fileName = $"Saves ({date}).zip";
            var destinationName = Path.Combine(_defaultDestination, fileName);

            try
            {
                File.Delete(destinationName);
            }
            catch (Exception exception)
            {
                _logger.LogException(exception);
            }

            ZipFile.CreateFromDirectory(source, destinationName, CompressionLevel.Optimal, false);
        }
        catch (Exception exception)
        {
            _logger.LogException(exception);
        }

        try
        {
            Directory.Delete(source, true);
        }
        catch (Exception exception)
        {
            _logger.LogException(exception);
        }

        stopwatch.Stop();

        _logger.WriteLine(ConsoleColor.Cyan, $"Archives: Packing done in {stopwatch.Elapsed.TotalSeconds} seconds.");
    }

    private void BeginPrune(DateTime threshold)
    {
        if (_serverHandler.HasCrashed || _serverHandler.IsClosing)
        {
            _prune.Invoke(threshold);
            return;
        }

        _sync.Reset();

        var asyncResult = _prune.BeginInvoke(threshold, EndPrune, threshold);

        lock (_taskRoot)
            _tasks.Add(asyncResult);
    }

    private void EndPrune(IAsyncResult asyncResult)
    {
        _prune.EndInvoke(asyncResult);

        lock (_taskRoot)
            _tasks.Remove(asyncResult);

        _sync.Set();
    }

    private void BeginPack(string source)
    {
        if (_serverHandler.HasCrashed || _serverHandler.IsClosing)
        {
            _pack.Invoke(source);
            return;
        }

        _sync.Reset();

        var asyncResult = _pack.BeginInvoke(source, EndPack, source);

        lock (_taskRoot)
            _tasks.Add(asyncResult);
    }

    private void EndPack(IAsyncResult asyncResult)
    {
        _pack.EndInvoke(asyncResult);

        lock (_taskRoot)
            _tasks.Remove(asyncResult);

        _sync.Set();
    }

    public bool Process(string source)
    {
        if (!Directory.Exists(_defaultDestination))
            Directory.CreateDirectory(_defaultDestination);

        if (ExpireAge > TimeSpan.Zero)
            BeginPrune(DateTime.UtcNow - ExpireAge);

        if (!string.IsNullOrWhiteSpace(source))
            BeginPack(source);

        return true;
    }
}
