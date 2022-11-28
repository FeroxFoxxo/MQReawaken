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
    private readonly Dictionary<string, Command> _commands;
    private readonly ServerHandler _handler;
    private readonly ILogger<ServerConsole> _logger;
    private readonly EventSink _sink;
    private readonly TimerThread _timerThread;

    private string _command;

    public Timer PollTimer;

    public ServerConsole(EventSink sink, TimerThread timerThread, ServerHandler handler, ILogger<ServerConsole> logger)
    {
        _sink = sink;
        _timerThread = timerThread;
        _handler = handler;
        _logger = logger;

        _commands = new Dictionary<string, Command>();

        AddCommand(new Command(
            "restart",
            "Sends a message to players informing them that the server is\n" +
            "           restarting, performs a forced save, then shuts down and\n" +
            "           restarts the server.",
            _ => _handler.KillServer(true)
        ));

        AddCommand(new Command(
            "shutdown",
            "Performs a forced save then shuts down the server.",
            _ => _handler.KillServer(false)
        ));

        AddCommand(new Command(
            "crash",
            "Forces an exception to be thrown.",
            _ => _timerThread.DelayCall(() => throw new Exception("Forced Crash"))
        ));
    }

    public void Initialize() => _sink.ServerStarted += _ => PollCommands();

    public void AddCommand(Command command) => _commands.Add(command.Name, command);

    private void PollCommands()
    {
        PollTimer = _timerThread.DelayCall(ProcessCommand, TimeSpan.Zero, TimeSpan.FromMilliseconds(100), 0);

        Task.Run(() =>
        {
            try
            {
                ProcessInput(Console.ReadLine());
            }
            catch (IOException)
            {
                // ignored
            }
        });
    }

    private void ProcessInput(string input)
    {
        if (_handler.IsClosing || _handler.HasCrashed)
            return;

        Interlocked.Exchange(ref _command, input);

        Task.Run(() => ProcessInput(Console.ReadLine()));
    }

    private void ProcessCommand()
    {
        if (_handler.IsClosing || _handler.HasCrashed)
            return;

        if (string.IsNullOrEmpty(_command))
            return;

        ProcessCommand(_command);
        Interlocked.Exchange(ref _command, string.Empty);
    }

    private void ProcessCommand(string input)
    {
        var inputs = input.Trim().Split();
        var name = inputs.FirstOrDefault();

        if (name != null && _commands.ContainsKey(name))
            _commands[name].CommandMethod(inputs);
        else
            DisplayHelp();
    }

    private void DisplayHelp()
    {
        _logger.LogInformation("Commands:");

        foreach (var command in _commands.Values)
        {
            var padding = 8 - command.Name.Length;
            if (padding < 0) padding = 0;
            _logger.LogDebug("{Name} - {Description}", command.Name.PadRight(padding), command.Description);
        }
    }
}
