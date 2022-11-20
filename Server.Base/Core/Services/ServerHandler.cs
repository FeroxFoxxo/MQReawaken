using Microsoft.Extensions.Logging;
using Server.Base.Core.Abstractions;
using Server.Base.Core.Events;
using Server.Base.Core.Extensions;
using Server.Base.Core.Helpers;
using System.Diagnostics;

namespace Server.Base.Core.Services;

public class ServerHandler : IService
{
    private readonly ILogger<ServerHandler> _logger;
    private readonly EventSink _sink;

    public readonly AutoResetEvent Signal;

    public bool HasCrashed;
    public bool IsClosing;
    public bool Restarting;
    public bool Saving;

    public ServerHandler(EventSink sink, ILogger<ServerHandler> logger)
    {
        _sink = sink;
        _logger = logger;

        IsClosing = false;
        HasCrashed = false;
        Restarting = false;
        Saving = false;

        Signal = new AutoResetEvent(true);
    }

    public void Initialize() => AppDomain.CurrentDomain.UnhandledException += UnhandledException;

    public void UnhandledException(object sender, UnhandledExceptionEventArgs ex)
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

    public void HandleClosed()
    {
        if (IsClosing)
            return;

        IsClosing = true;

        _logger.LogError("Exiting server, please wait!");

        _sink.InvokeInternalShutdown();

        if (!HasCrashed)
            _sink.InvokeShutdown();

        _logger.LogCritical("Successfully quit server.");
    }

    public void Set() => Signal.Set();
}
