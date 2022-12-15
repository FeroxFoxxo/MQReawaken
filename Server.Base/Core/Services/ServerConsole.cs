using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Server.Base.Core.Abstractions;
using Server.Base.Core.Helpers;
using Server.Base.Core.Models;
using Server.Base.Timers.Extensions;
using Server.Base.Timers.Services;
using Timer = Server.Base.Timers.Timer;

namespace Server.Base.Core.Services;

public class ServerConsole : IService
{
    private readonly Dictionary<string, ConsoleCommand> _commands;
    private readonly ServerHandler _handler;
    private readonly EventSink _sink;
    private readonly ILogger<ServerConsole> _logger;
    private readonly IHostApplicationLifetime _appLifetime;
    private readonly TimerThread _timerThread;

    private string _command;

    public Timer PollTimer;
    private readonly Thread _consoleThread;

    public ServerConsole(TimerThread timerThread, ServerHandler handler, EventSink sink, ILogger<ServerConsole> logger,
        IHostApplicationLifetime appLifetime)
    {
        _timerThread = timerThread;
        _handler = handler;
        _sink = sink;
        _logger = logger;
        _appLifetime = appLifetime;

        _commands = new Dictionary<string, ConsoleCommand>();

        _consoleThread = new Thread(ConsoleLoopThread)
        {
            Name = "Console Thread"
        };
    }

    public void AddCommand(ConsoleCommand consoleCommand) => _commands.Add(consoleCommand.Name, consoleCommand);

    public void Initialize()
    {
        _appLifetime.ApplicationStarted.Register(DisplayHelp);
        _sink.ServerStarted += _ => RunConsoleListener();
    }

    public void RunConsoleListener()
    {
        AddCommand(new ConsoleCommand(
            "restart",
            "Sends a message to players informing them that the server is\n" +
            "           restarting, performs a forced save, then shuts down and\n" +
            "           restarts the server.",
            _ => _handler.KillServer(true)
        ));

        AddCommand(new ConsoleCommand(
            "shutdown",
            "Performs a forced save then shuts down the server.",
            _ => _handler.KillServer(false)
        ));

        AddCommand(new ConsoleCommand(
            "crash",
            "Forces an exception to be thrown.",
            _ => _timerThread.DelayCall(() => throw new Exception("Forced Crash"))
        ));

        PollTimer = _timerThread.DelayCall(ProcessCommand, TimeSpan.Zero, TimeSpan.FromMilliseconds(100), 0);

        _consoleThread.Start();
    }

    public void ConsoleLoopThread()
    {
        try
        {
            while (!_handler.IsClosing && !_handler.HasCrashed)
            {
                var input = Console.ReadLine();

                Interlocked.Exchange(ref _command, input);
            }
        }
        catch (IOException)
        {
            // ignored
        }
    }

    private void ProcessCommand()
    {
        if (_handler.IsClosing || _handler.HasCrashed)
            return;

        ProcessCommand(_command);
        Interlocked.Exchange(ref _command, string.Empty);
    }

    private void ProcessCommand(string input)
    {
        var inputs = input.Trim().Split();
        var name = inputs.FirstOrDefault();

        if (name != null && _commands.ContainsKey(name))
        {
            _commands[name].CommandMethod(inputs);
            _logger.LogInformation("Successfully Ran Command '{Name}'", name);
        }
        else
        {
            DisplayHelp();
        }
    }

    private void DisplayHelp()
    {
        _logger.LogDebug("Commands:");

        foreach (var command in _commands.Values)
        {
            var padding = 8 - command.Name.Length;
            if (padding < 0) padding = 0;
            _logger.LogDebug("{Name} - {Description}", command.Name.PadRight(padding), command.Description);
        }
    }
}
