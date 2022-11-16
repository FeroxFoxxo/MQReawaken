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

    public override void Write(char character) => WriteLine(ConsoleColor.DarkMagenta, character.ToString());

    public override void Write(string line) => Write(ConsoleColor.DarkMagenta, line);

    public override void WriteLine(string line) => WriteLine(ConsoleColor.DarkMagenta, line);

    public void Write(ConsoleColor color, string @string)
    {
        lock (((ICollection)_consoleColors).SyncRoot)
        {
            PushColor(color);
            Console.Write(@string);
            PopColor();
        }
    }

    public void WriteLine(ConsoleColor color, string @string)
    {
        lock (((ICollection)_consoleColors).SyncRoot)
        {
            PushColor(color);
            Console.WriteLine(@string);
            PopColor();
        }
    }

    private void PushColor(ConsoleColor color)
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
            LogException(e);
        }
    }

    private void PopColor()
    {
        try
        {
            lock (((ICollection)_consoleColors).SyncRoot)
                Console.ForegroundColor = _consoleColors.Pop();
        }
        catch (Exception e)
        {
            LogException(e);
        }
    }

    public void LogException(Exception exception)
    {
        WriteLine(ConsoleColor.Red, "Caught Exception:");
        WriteLine(ConsoleColor.DarkRed, exception.ToString());

        Output.WriteLine($"Exception Caught: {DateTime.UtcNow}");
        Output.WriteLine(exception);
        Output.WriteLine();
    }
}
