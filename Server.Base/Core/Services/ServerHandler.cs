using System.Diagnostics;
using System.Reflection;
using System.Runtime;
using System.Runtime.Versioning;
using Microsoft.Extensions.DependencyInjection;
using Server.Base.Core.Abstractions;
using Server.Base.Core.Events;
using Server.Base.Core.Extensions;
using Server.Base.Core.Helpers;
using Server.Base.Logging;
using Server.Base.Logging.Internal;
using Server.Base.Network.Services;
using Server.Base.Timers.Services;
using Server.Base.Worlds;
using Module = Server.Base.Core.Abstractions.Module;

namespace Server.Base.Core.Services;

public class ServerHandler : IService
{
    private readonly NetStateHandler _handler;
    private readonly Logger _logger;
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

    public ServerHandler(TimerThread timerThread, World world, EventSink sink, MessagePump pump, Logger logger,
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

    public void Initialize()
    {
        AppDomain.CurrentDomain.UnhandledException += UnhandledException;
        AppDomain.CurrentDomain.ProcessExit += (_, _) => HandleClosed();
    }

    public void StartServer()
    {
        try
        {
            if (!Directory.Exists("Logs"))
                Directory.CreateDirectory("Logs");

            Console.SetOut(MultiConsoleOut);
        }
        catch (Exception e)
        {
            _logger.LogException<ServerHandler>(e);
        }

        Thread.CurrentThread.Name = "Server Thread";

        var baseDirectory = InternalDirectory.GetBaseDirectory();

        if (baseDirectory.Length > 0)
            Directory.SetCurrentDirectory(baseDirectory);

        foreach (var module in _modules)
            _logger.WriteLine<ServerHandler>(ConsoleColor.Cyan, module.GetModuleInformation());

        if (GetOsType.IsUnix())
            _logger.WriteLine<ServerHandler>(ConsoleColor.Yellow, "Unix environment detected");

        var frameworkName = Assembly.GetEntryAssembly()?
            .GetCustomAttribute<TargetFrameworkAttribute>()?.FrameworkName;

        _logger.WriteLine<ServerHandler>(ConsoleColor.Green,
            $"Compiled for {(GetOsType.IsUnix() ? "UNIX " : "WINDOWS")} " +
            $"and running on {(string.IsNullOrEmpty(frameworkName) ? "UNKNOWN" : frameworkName)}"
        );

        if (GCSettings.IsServerGC)
            _logger.WriteLine<ServerHandler>(ConsoleColor.Green, "Server garbage collection mode enabled");

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
        catch (Exception exception)
        {
            UnhandledException(null, new UnhandledExceptionEventArgs(exception, true));
        }
    }

    private void UnhandledException(object sender, UnhandledExceptionEventArgs exception)
    {
        _logger.WriteLine<ServerHandler>(exception.IsTerminating ? ConsoleColor.Red : ConsoleColor.Yellow,
            exception.IsTerminating ? $"Error: {exception.ExceptionObject}" : $"Warning: {exception.ExceptionObject}");

        if (!exception.IsTerminating) return;

        HasCrashed = true;

        var doClose = false;

        CrashedEventArgs arguments = new(exception.ExceptionObject as Exception);

        try
        {
            _sink.InvokeCrashed(arguments);
            doClose = arguments.Close;
        }
        catch (Exception crashedException)
        {
            _logger.LogException<ServerHandler>(crashedException);
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

            _logger.WriteLine<ServerHandler>(ConsoleColor.Red, "This exception is fatal, press return to exit.");
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

        _logger.WriteLine<ServerHandler>(ConsoleColor.Red, "Exiting server, please wait!");

        _world.Save(false, true);

        if (!HasCrashed)
            _sink.InvokeShutdown();

        _timerThread.Set();

        _logger.WriteLine<ServerHandler>(ConsoleColor.Red, "Successfully quit server.");
    }

    public void Set() => Signal.Set();
}
