using Microsoft.Extensions.Logging;

namespace Server.Base.Logging;

public class LoggerProvider : ILoggerProvider
{
    public ILogger CreateLogger(string categoryName) => new Logger(categoryName);

    public void Dispose() => GC.SuppressFinalize(this);
}
