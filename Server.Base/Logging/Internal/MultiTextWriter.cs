using System.Text;

namespace Server.Base.Logging.Internal;

public class MultiTextWriter : TextWriter
{
    private readonly List<TextWriter> _streams;

    public override Encoding Encoding => Encoding.Default;

    public MultiTextWriter(params TextWriter[] streams)
    {
        _streams = new List<TextWriter>(streams);

        if (_streams.Count < 0)
            throw new ArgumentException("You must specify at least one stream.");
    }

    public void Add(TextWriter textWriter) => _streams.Add(textWriter);

    public void Remove(TextWriter textWriter) => _streams.Remove(textWriter);

    public override void Write(char character)
    {
        foreach (var textWriter in _streams)
            textWriter.Write(character);
    }

    public override void WriteLine(string line)
    {
        foreach (var textWriter in _streams)
            textWriter.WriteLine(line);
    }
}
