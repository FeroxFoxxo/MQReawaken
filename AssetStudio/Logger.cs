using Microsoft.Extensions.Logging;
using System;
using System.Text;

namespace AssetStudio;

public static class Logger
{
    public static ILogger Default;

    public static void Verbose(string message) => Default.LogTrace(message);
    public static void Debug(string message) => Default.LogDebug(message);
    public static void Info(string message) => Default.LogInformation(message);
    public static void Warning(string message) => Default.LogWarning(message);
    public static void Error(string message) => Default.LogError(message);

    public static void Error(string message, Exception e)
    {
        var sb = new StringBuilder();
        sb.AppendLine(message);
        sb.AppendLine(e.ToString());
        Default.LogError(sb.ToString());
    }
}
