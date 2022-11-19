﻿using System.Reflection;
using System.Runtime;
using System.Runtime.Versioning;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Server.Base.Core.Extensions;
using Server.Base.Core.Helpers;
using Server.Base.Core.Services;
using Server.Base.Logging.Internal;
using Server.Base.Network.Services;
using Server.Base.Timers.Services;
using Server.Base.Worlds;
using Module = Server.Base.Core.Abstractions.Module;

namespace Server.Base.Core.Workers;

public class ServerWorker : IHostedService
{
    private readonly NetStateHandler _handler;
    private readonly ILogger<ServerWorker> _logger;
    private readonly Module[] _modules;
    private readonly MessagePump _pump;
    private readonly ServerHandler _serverHandler;
    private readonly EventSink _sink;
    private readonly TimerThread _timerThread;
    private readonly World _world;

    public readonly MultiTextWriter MultiConsoleOut;

    public ServerWorker(NetStateHandler handler, IServiceProvider services, ILogger<ServerWorker> logger,
        ServerHandler serverHandler, MessagePump pump, TimerThread timerThread, World world, EventSink sink)
    {
        _handler = handler;
        _logger = logger;
        _serverHandler = serverHandler;
        _pump = pump;
        _timerThread = timerThread;
        _world = world;
        _sink = sink;

        _modules = services.GetRequiredServices<Module>().ToArray();

        MultiConsoleOut = new MultiTextWriter(Console.Out, new FileLogger("console.log"));
    }

    public Task StartAsync(CancellationToken cancellationToken)
    {
        _sink.InternalShutdown += OnClose;

        try
        {
            if (!Directory.Exists("Logs"))
                Directory.CreateDirectory("Logs");

            Console.SetOut(MultiConsoleOut);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Could not get log directory!");
        }

        Thread.CurrentThread.Name = "Server Thread";

        var baseDirectory = InternalDirectory.GetBaseDirectory();

        if (baseDirectory.Length > 0)
            Directory.SetCurrentDirectory(baseDirectory);

        foreach (var module in _modules)
            _logger.LogDebug("{ModuleInfo}", module.GetModuleInformation());

        if (GetOsType.IsUnix())
            _logger.LogWarning("Unix environment detected");

        var frameworkName = Assembly.GetEntryAssembly()?
            .GetCustomAttribute<TargetFrameworkAttribute>()?.FrameworkName;

        _logger.LogDebug("Compiled for {OS} and running on {NETVersion}", GetOsType.IsUnix() ? "UNIX " : "WINDOWS",
            string.IsNullOrEmpty(frameworkName) ? "UNKNOWN" : frameworkName);

        if (GCSettings.IsServerGC)
            _logger.LogDebug("Server garbage collection mode enabled");

        _world.Load();
        _sink.InvokeServerStarted();

        try
        {
            while (!_serverHandler.IsClosing)
            {
                _serverHandler.Signal.WaitOne();

                _timerThread.Slice();
                _pump.Slice();

                _handler.ProcessDisposedQueue();
            }
        }
        catch (Exception ex)
        {
            _serverHandler.UnhandledException(null, new UnhandledExceptionEventArgs(ex, true));
        }

        return Task.CompletedTask;
    }

    public Task StopAsync(CancellationToken cancellationToken)
    {
        _serverHandler.HandleClosed();
        return Task.CompletedTask;
    }

    public void OnClose()
    {
        _world.Save(false, true);

        try
        {
            if (_pump != null)
                foreach (var listener in _pump.Listeners)
                    listener.Dispose();
        }
        catch (Exception ex)
        {
            _logger.LogCritical(ex, "Unable to dispose of listeners on close.");
        }

        _timerThread.Set();
    }
}
