using System.Text;

namespace Server.Base.Logging.Internal;

public class FileLogger : TextWriter
{
    public const string DateFormat = "[MMMM dd hh:mm:ss.f tt]: ";

    private bool _newLine;

    public string FileName { get; }

    public override Encoding Encoding => Encoding.Default;

    public FileLogger(string file) : this(file, false)
    {
    }

    public FileLogger(string file, bool append)
    {
        FileName = $"Logs/{file}";

        using (
            StreamWriter writer = new(new FileStream(FileName, append ? FileMode.Append : FileMode.Create,
                FileAccess.Write, FileShare.Read)))
            writer.WriteLine(">>>Logging started on {0:f}.", DateTime.Now);

        _newLine = true;
    }

    public override void Write(char character)
    {
        using StreamWriter writer = new(new FileStream(FileName, FileMode.Append, FileAccess.Write, FileShare.Read));
        if (_newLine)
        {
            writer.Write(DateTime.UtcNow.ToString(DateFormat));
            _newLine = false;
        }

        writer.Write(character);
    }

    public override void Write(string @string)
    {
        using StreamWriter writer = new(new FileStream(FileName, FileMode.Append, FileAccess.Write, FileShare.Read));
        if (_newLine)
        {
            writer.Write(DateTime.UtcNow.ToString(DateFormat));
            _newLine = false;
        }

        writer.Write(@string);
    }

    public override void WriteLine(string line)
    {
        using StreamWriter writer = new(new FileStream(FileName, FileMode.Append, FileAccess.Write, FileShare.Read));
        if (_newLine)
            writer.Write(DateTime.UtcNow.ToString(DateFormat));

        writer.WriteLine(line);
        _newLine = true;
    }
}
