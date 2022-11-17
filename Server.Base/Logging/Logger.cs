using System.Collections;
using Microsoft.Extensions.Logging;
using Server.Base.Core.Extensions;

namespace Server.Base.Logging;

public class Logger : ILogger
{
    private const LogLevel Level = LogLevel.Trace;
    private static readonly Stack<ConsoleColor> ConsoleColors = new();
    private static StreamWriter _output;

    private readonly string _categoryName;

    private bool _shouldDebugName;

    private static string LogDirectory => "Logs/Exceptions";

    private static StreamWriter Output
    {
        get
        {
            if (_output != null) return _output;

            var currentLog = Path.Combine(InternalDirectory.GetBaseDirectory(), LogDirectory,
                $"{DateTime.UtcNow.ToShortDateString().Replace('/', '_')}.log");

            _output = new StreamWriter(
                !File.Exists(currentLog)
                    ? File.Create(currentLog)
                    : File.Open(currentLog, FileMode.Append))
            {
                AutoFlush = true
            };

            _output.WriteLine("----------------------------");
            _output.WriteLine($"Exception log started on {DateTime.UtcNow}");
            _output.WriteLine();

            return _output;
        }
    }

    public Logger(string categoryName)
    {
        _categoryName = categoryName;
        _shouldDebugName = true;
    }

    public void Log<TState>(LogLevel logLevel, EventId eventId, TState state, Exception ex,
        Func<TState, Exception, string> formatter)
    {
        if (!IsEnabled(logLevel))
            return;

        var message = formatter(state, ex);

        if (ex != null)
        {
            WriteLine(ConsoleColor.Red, message);
            LogException(ex);
        }
        else
        {
            var color = logLevel switch
            {
                LogLevel.Trace => ConsoleColor.Magenta,
                LogLevel.Debug => ConsoleColor.DarkCyan,
                LogLevel.Information => ConsoleColor.Cyan,
                LogLevel.Warning => ConsoleColor.Yellow,
                LogLevel.Error => ConsoleColor.Red,
                LogLevel.Critical => ConsoleColor.DarkRed,
                _ => ConsoleColor.DarkMagenta
            };

            WriteLine(color, message);
        }
    }

    public bool IsEnabled(LogLevel logLevel) => logLevel >= Level;

    public IDisposable BeginScope<TState>(TState state) => null;

    public void ShouldDebugWithName(bool shouldDebugName) => _shouldDebugName = shouldDebugName;

    private void WriteLine(ConsoleColor color, string message)
    {
        lock (((ICollection)ConsoleColors).SyncRoot)
        {
            PushColor(color);
            Console.WriteLine($"{(_shouldDebugName ? $"{_categoryName}: " : "")}{message}");
            PopColor();
        }
    }

    private void PushColor(ConsoleColor color)
    {
        try
        {
            lock (((ICollection)ConsoleColors).SyncRoot)
            {
                ConsoleColors.Push(Console.ForegroundColor);

                Console.ForegroundColor = color;
            }
        }
        catch (Exception ex)
        {
            LogException(ex);
        }
    }

    private void PopColor()
    {
        try
        {
            lock (((ICollection)ConsoleColors).SyncRoot)
                Console.ForegroundColor = ConsoleColors.Pop();
        }
        catch (Exception ex)
        {
            LogException(ex);
        }
    }

    private void LogException(Exception ex)
    {
        WriteLine(ConsoleColor.Red, "Caught Exception:");
        WriteLine(ConsoleColor.DarkRed, ex.ToString());

        Output.WriteLine($"Exception Caught: {DateTime.UtcNow}");
        Output.WriteLine(ex);
        Output.WriteLine();
    }
}
