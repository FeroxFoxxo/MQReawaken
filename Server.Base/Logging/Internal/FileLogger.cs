using System.Text;
using Server.Base.Core.Extensions;

namespace Server.Base.Logging.Internal;

public class FileLogger : TextWriter
{
    public const string DateFormat = "[MMMM dd hh:mm:ss.f tt]: ";

    private bool _newLine;

    public string FileName { get; }

    public override Encoding Encoding => Encoding.Default;

    public FileLogger(string file)
    {
        using (
            var fileStream = GetLogFileStream.GetLogFile(file, ""))
        {
            FileName = fileStream.Name;

            using var writer = new StreamWriter(fileStream)
            {
                AutoFlush = true
            };

            writer.WriteLine(">>>Logging started on {0:f}.", DateTime.Now);
        }

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
