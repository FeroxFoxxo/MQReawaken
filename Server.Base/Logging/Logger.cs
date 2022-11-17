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

            var currentLog = Path.Combine(
                InternalDirectory.GetBaseDirectory(), LogDirectory,
                $"{DateTime.UtcNow.ToShortDateString().Replace('/', '_')}.log");

            var path = Path.GetDirectoryName(currentLog);

            if (!Directory.Exists(path) && path != null)
                Directory.CreateDirectory(path);

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
            WriteLine(ConsoleColor.Red, message, "E", eventId.Id);
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

            var shortLogLevel = logLevel switch
            {
                LogLevel.Trace => "T",
                LogLevel.Debug => "D",
                LogLevel.Information => "I",
                LogLevel.Warning => "W",
                LogLevel.Error => "E",
                LogLevel.Critical => "C",
                _ => "U"
            };

            WriteLine(color, message, shortLogLevel, eventId.Id);
        }
    }

    public bool IsEnabled(LogLevel logLevel) => logLevel >= Level;

    public IDisposable BeginScope<TState>(TState state) => null;

    public void ShouldDebugWithName(bool shouldDebugName) => _shouldDebugName = shouldDebugName;

    private void WriteLine(ConsoleColor color, string message, string shortLogLevel, int eventId)
    {
        var currentTime = DateTime.UtcNow.ToString("yyyy-MM-dd HH:mm:ss");
        var prefix = $"[{currentTime}] [{shortLogLevel}]{(_shouldDebugName ? $" {_categoryName}[{eventId}]" : "")}: ";

        lock (((ICollection)ConsoleColors).SyncRoot)
        {
            PushColor(color);
            Console.WriteLine($"{(_shouldDebugName ? $"{prefix}: " : "")}{message}");
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
        WriteLine(ConsoleColor.DarkRed, ex.ToString(), "C", ex.HResult);

        Output.WriteLine($"Exception Caught: {DateTime.UtcNow}");
        Output.WriteLine(ex);
        Output.WriteLine();
    }
}
