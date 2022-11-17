using System.Collections;
using System.Text;

namespace Server.Base.Logging;

public class Logger : TextWriter
{
    private readonly Stack<ConsoleColor> _consoleColors = new();
    private StreamWriter _output;
    public string LogDirectory { get; set; }

    public override Encoding Encoding => Encoding.Default;

    public StreamWriter Output
    {
        get
        {
            if (_output != null) return _output;

            _output = new StreamWriter(Path.Combine(LogDirectory, $"{DateTime.UtcNow.ToLongDateString()}.log"), true)
            {
                AutoFlush = true
            };

            _output.WriteLine("----------------------------");
            _output.WriteLine($"Exception log started on {DateTime.UtcNow}");
            _output.WriteLine();

            return _output;
        }
    }

    public void Write<T>(ConsoleColor color, string @string)
    {
        lock (((ICollection)_consoleColors).SyncRoot)
        {
            PushColor<T>(color);
            Console.Write($"{typeof(T).Name}: {@string}");
            PopColor<T>();
        }
    }

    public void WriteLine<T>(ConsoleColor color, string @string)
    {
        lock (((ICollection)_consoleColors).SyncRoot)
        {
            PushColor<T>(color);
            Console.WriteLine($"{typeof(T).Name}: {@string}");
            PopColor<T>();
        }
    }

    public void WriteNewLine() => Console.WriteLine();

    private void PushColor<T>(ConsoleColor color)
    {
        try
        {
            lock (((ICollection)_consoleColors).SyncRoot)
            {
                _consoleColors.Push(Console.ForegroundColor);

                Console.ForegroundColor = color;
            }
        }
        catch (Exception e)
        {
            LogException<T>(e);
        }
    }

    private void PopColor<T>()
    {
        try
        {
            lock (((ICollection)_consoleColors).SyncRoot)
                Console.ForegroundColor = _consoleColors.Pop();
        }
        catch (Exception e)
        {
            LogException<T>(e);
        }
    }

    public void LogException<T>(Exception exception)
    {
        WriteLine<T>(ConsoleColor.Red, "Caught Exception:");
        WriteLine<T>(ConsoleColor.DarkRed, exception.ToString());

        Output.WriteLine($"Exception Caught: {DateTime.UtcNow}");
        Output.WriteLine(exception);
        Output.WriteLine();
    }
}
