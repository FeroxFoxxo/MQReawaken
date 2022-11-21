using Microsoft.Extensions.Logging;
using Server.Base.Core.Extensions;
using System.Collections;

namespace Server.Base.Logging;

public class Logger : ILogger
{
    private const LogLevel Level = LogLevel.Trace;

    private static readonly Stack<ConsoleColor> ConsoleColors = new();

    private static int _offset;
    private static StreamWriter _output;

    private readonly string _categoryName;

    private static StreamWriter Output
    {
        get
        {
            if (_output != null) return _output;

            var fileName = $"{DateTime.UtcNow.ToShortDateString().Replace('/', '_')}.log";

            _output = GetFile.GetStreamWriter(fileName, "Exceptions", FileMode.OpenOrCreate);

            _output.WriteLine("----------------------------");
            _output.WriteLine($"Exception log started on {DateTime.UtcNow}");
            _output.WriteLine();

            return _output;
        }
    }

    public Logger(string categoryName) => _categoryName = categoryName;

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

    private void WriteLine(ConsoleColor color, string message, string shortLogLevel, int eventId)
    {
        var currentTime = DateTime.UtcNow.ToString("yyyy-MM-dd HH:mm:ss");
        var prefix = $"[{currentTime}] [{shortLogLevel}] {_categoryName.Split('.').Last()}[{eventId}]";

        if (_offset < prefix.Length) _offset = prefix.Length;
        var length = _offset - prefix.Length;
        if (length < 0) length = 0;
        var offsetSpaced = string.Concat(Enumerable.Repeat(" ", length));

        lock (((ICollection)ConsoleColors).SyncRoot)
        {
            PushColor(color);

            foreach (var msg in message.Split('\n'))
                Console.WriteLine($"{prefix}:{offsetSpaced} {msg}");

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
        WriteLine(ConsoleColor.DarkRed, $"{ex}", "C", ex.HResult);

        Output.WriteLine($"Exception Caught: {DateTime.UtcNow}");
        Output.WriteLine(ex);
        Output.WriteLine();
    }
}
