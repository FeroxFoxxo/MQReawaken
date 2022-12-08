using AssetStudio;
using Microsoft.Extensions.Logging;
using ILogger = AssetStudio.ILogger;

namespace Web.AssetBundles.Helpers;

public class AssetBundleLogger : ILogger
{
    private readonly Microsoft.Extensions.Logging.ILogger _logger;

    public AssetBundleLogger(Microsoft.Extensions.Logging.ILogger logger) => _logger = logger;

    public void Log(LoggerEvent loggerEvent, string message)
    {
        switch (loggerEvent)
        {
            case LoggerEvent.Verbose:
                _logger.LogTrace(message);
                break;
            case LoggerEvent.Debug:
                _logger.LogDebug(message);
                break;
            case LoggerEvent.Info:
                _logger.LogInformation(message);
                break;
            case LoggerEvent.Warning:
                _logger.LogWarning(message);
                break;
            case LoggerEvent.Error:
                _logger.LogError(message);
                break;
            default:
                throw new ArgumentOutOfRangeException(nameof(loggerEvent), loggerEvent, null);
        }
    }
}
