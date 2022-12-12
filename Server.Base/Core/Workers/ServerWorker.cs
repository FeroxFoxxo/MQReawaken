using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Server.Base.Core.Events;
using Server.Base.Core.Extensions;
using Server.Base.Core.Helpers;
using Server.Base.Core.Services;
using Server.Base.Logging.Internal;
using Server.Base.Network.Services;
using Server.Base.Timers.Services;
using Server.Base.Worlds;
using System.Reflection;
using System.Runtime;
using System.Runtime.Versioning;

namespace Server.Base.Core.Workers;

public class ServerWorker : IHostedService
{
    private readonly NetStateHandler _handler;
    private readonly ILogger<ServerWorker> _logger;
    private readonly MessagePump _pump;
    private readonly ServerHandler _serverHandler;
    private readonly IServiceProvider _services;
    private readonly EventSink _sink;
    private readonly TimerThread _timerThread;
    private readonly World _world;

    public readonly MultiTextWriter MultiConsoleOut;
    private readonly Thread _serverThread;

    public ServerWorker(NetStateHandler handler, ILogger<ServerWorker> logger,
        ServerHandler serverHandler, MessagePump pump, TimerThread timerThread, World world,
        EventSink sink, IServiceProvider services)
    {
        _handler = handler;
        _logger = logger;
        _serverHandler = serverHandler;
        _pump = pump;
        _timerThread = timerThread;
        _world = world;
        _sink = sink;
        _services = services;

        MultiConsoleOut = new MultiTextWriter(Console.Out, new FileLogger("console.log"));

        _serverThread = new Thread(ServerLoopThread)
        {
            Name = "Server Thread"
        };
    }

    public Task StartAsync(CancellationToken cancellationToken)
    {
        _sink.InternalShutdown += OnClose;
        _sink.ServerStarted += _ => _serverThread.Start();

        Thread.CurrentThread.Name = "Main Thread";

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

        var baseDirectory = InternalDirectory.GetBaseDirectory();

        if (baseDirectory.Length > 0)
            Directory.SetCurrentDirectory(baseDirectory);

        foreach (var module in _serverHandler.Modules)
            _logger.LogDebug("{ModuleInfo}", module.GetModuleInformation());

        if (GetOsType.IsUnix())
            _logger.LogWarning("Unix environment detected");

        var frameworkName = Assembly.GetEntryAssembly()?
            .GetCustomAttribute<TargetFrameworkAttribute>()?.FrameworkName;

        _logger.LogDebug("Compiled for {OS} and running on {NetVersion}", GetOsType.IsUnix() ? "UNIX " : "WINDOWS",
            string.IsNullOrEmpty(frameworkName) ? "UNKNOWN" : frameworkName);

        if (GCSettings.IsServerGC)
            _logger.LogDebug("Server garbage collection mode enabled");

        _world.Load();
        _sink.InvokeServerStarted(new ServerStartedEventArgs(_serverHandler.Modules));

        return Task.CompletedTask;
    }

    public Task StopAsync(CancellationToken cancellationToken)
    {
        _serverHandler.HandleClosed();
        return Task.CompletedTask;
    }

    public void ServerLoopThread()
    {
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
    }

    public void OnClose()
    {
        _world.Save(false, true);

        _services.SaveConfigs(_serverHandler.Modules);

        try
        {
            if (_pump != null)
                foreach (var listener in _pump.Listeners)
                    listener?.Dispose();
        }
        catch (Exception ex)
        {
            _logger.LogCritical(ex, "Unable to dispose of listeners on close.");
        }

        _timerThread.Set();
    }
}
