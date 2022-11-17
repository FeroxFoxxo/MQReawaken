using System.Diagnostics;
using System.Reflection;
using System.Runtime;
using System.Runtime.Versioning;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Server.Base.Core.Events;
using Server.Base.Core.Extensions;
using Server.Base.Core.Helpers;
using Server.Base.Logging.Internal;
using Server.Base.Network.Services;
using Server.Base.Timers.Services;
using Server.Base.Worlds;
using Module = Server.Base.Core.Abstractions.Module;

namespace Server.Base.Core.Services;

public class ServerHandler : IHostedService
{
    private readonly NetStateHandler _handler;
    private readonly ILogger<ServerHandler> _logger;
    private readonly Module[] _modules;
    private readonly MessagePump _pump;
    private readonly EventSink _sink;
    private readonly TimerThread _timerThread;
    private readonly World _world;

    public readonly MultiTextWriter MultiConsoleOut;
    public readonly AutoResetEvent Signal;

    public bool HasCrashed;
    public bool IsClosing;
    public bool Restarting;
    public bool Saving;

    public ServerHandler(TimerThread timerThread, World world, EventSink sink, MessagePump pump,
        ILogger<ServerHandler> logger,
        NetStateHandler handler, IServiceProvider serviceProvider)
    {
        _timerThread = timerThread;
        _world = world;
        _sink = sink;
        _pump = pump;
        _logger = logger;
        _handler = handler;

        _modules = serviceProvider.GetServices<Module>().ToArray();

        IsClosing = false;
        HasCrashed = false;
        Restarting = false;
        Saving = false;

        Signal = new AutoResetEvent(true);
        MultiConsoleOut = new MultiTextWriter(Console.Out, new FileLogger("console.log"));
    }

    public Task StartAsync(CancellationToken cancellationToken)
    {
        AppDomain.CurrentDomain.UnhandledException += UnhandledException;

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
            while (!IsClosing)
            {
                Signal.WaitOne();

                _timerThread.Slice();
                _pump.Slice();

                _handler.ProcessDisposedQueue();
            }
        }
        catch (Exception ex)
        {
            UnhandledException(null, new UnhandledExceptionEventArgs(ex, true));
        }

        return Task.CompletedTask;
    }

    public Task StopAsync(CancellationToken cancellationToken)
    {
        HandleClosed();
        return Task.CompletedTask;
    }

    private void UnhandledException(object sender, UnhandledExceptionEventArgs ex)
    {
        if (ex.IsTerminating)
            _logger.LogError("Unhandled Error: {ERROR}", ex.ExceptionObject);
        else
            _logger.LogWarning("Unhandled Warning: {WARNING}", ex.ExceptionObject);

        if (!ex.IsTerminating) return;

        HasCrashed = true;

        var doClose = false;

        CrashedEventArgs arguments = new(ex.ExceptionObject as Exception);

        try
        {
            _sink.InvokeCrashed(arguments);
            doClose = arguments.Close;
        }
        catch (Exception crashedException)
        {
            _logger.LogError(crashedException, "Unable to invoke crashed arguments");
        }

        if (!doClose)
        {
            try
            {
                if (_pump != null)
                    foreach (var listener in _pump.Listeners)
                        listener.Dispose();
            }
            catch
            {
                // ignored
            }

            _logger.LogCritical("This exception is fatal, press return to exit.");
            Console.ReadLine();
        }

        KillServer(false);
    }

    public void KillServer(bool restart)
    {
        HandleClosed();

        if (restart)
            Process.Start(GetExePath.Path());

        Process.GetCurrentProcess().Kill();
    }

    private void HandleClosed()
    {
        if (IsClosing)
            return;

        IsClosing = true;

        _logger.LogError("Exiting server, please wait!");

        _world.Save(false, true);

        if (!HasCrashed)
            _sink.InvokeShutdown();

        _timerThread.Set();

        _logger.LogCritical("Successfully quit server.");
    }

    public void Set() => Signal.Set();
}
