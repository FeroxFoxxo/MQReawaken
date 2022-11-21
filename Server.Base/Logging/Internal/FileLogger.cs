using Server.Base.Core.Extensions;
using System.Text;

namespace Server.Base.Logging.Internal;

public class FileLogger : TextWriter
{
    public const string DateFormat = "[MMMM dd hh:mm:ss.f tt]: ";

    private bool _newLine;

    public string FileName { get; }

    public override Encoding Encoding => Encoding.Default;

    public FileLogger(string file)
    {
        FileName = file;

        using var writer = GetFile.GetStreamWriter(file, "Logs", FileMode.Create);
        writer.WriteLine(">>>Logging started on {0:f}.", DateTime.Now);

        _newLine = true;
    }

    public override void Write(char character)
    {
        using var writer = GetFile.GetStreamWriter(FileName, "Logs", FileMode.Append);

        if (_newLine)
        {
            writer.Write(DateTime.UtcNow.ToString(DateFormat));
            _newLine = false;
        }

        writer.Write(character);
    }

    public override void Write(string @string)
    {
        using var writer = GetFile.GetStreamWriter(FileName, "Logs", FileMode.Append);

        if (_newLine)
        {
            writer.Write(DateTime.UtcNow.ToString(DateFormat));
            _newLine = false;
        }

        writer.Write(@string);
    }

    public override void WriteLine(string line)
    {
        using var writer = GetFile.GetStreamWriter(FileName, "Logs", FileMode.Append);

        if (_newLine)
            writer.Write(DateTime.UtcNow.ToString(DateFormat));

        writer.WriteLine(line);
        _newLine = true;
    }
}
