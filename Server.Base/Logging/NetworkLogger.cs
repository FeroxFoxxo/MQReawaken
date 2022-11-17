using System.Net.Sockets;
using System.Text;
using Microsoft.Extensions.Logging;
using Server.Base.Accounts.Helpers;
using Server.Base.Accounts.Modals;
using Server.Base.Logging.Internal;
using Server.Base.Network;
using Server.Base.Network.Helpers;
using Server.Base.Network.Services;

namespace Server.Base.Logging;

public class NetworkLogger
{
    private readonly Dictionary<string, FileLogger> _fileLoggers;
    private readonly ILogger<NetworkLogger> _logger;

    public NetworkLogger(ILogger<NetworkLogger> logger)
    {
        _logger = logger;
        _fileLoggers = new Dictionary<string, FileLogger>();
    }

    public void IpLimitedError(NetState netState)
    {
        var builder = new StringBuilder()
            .AppendLine($"{DateTime.UtcNow}\t" +
                        $"Past IP limit threshold\t" +
                        $"{netState}");

        WriteToFile<IpLimiter>("ipLimits.log", builder, ConsoleColor.DarkGray);
    }

    public void ThrottledError(NetState netState, InvalidAccountAccessLog accessLog)
    {
        var builder = new StringBuilder()
            .AppendLine($"{DateTime.UtcNow}\t" +
                        $"{netState}\t" +
                        $"{accessLog.Counts}");

        WriteToFile<AccountAttackLimiter>("throttle.log", builder, ConsoleColor.DarkGray);
    }

    public void TracePacketError(string packetId, string packet, NetState state)
    {
        if (packet.Length <= 0)
            return;

        var builder = new StringBuilder()
            .AppendLine($"Client: {state}: Unhandled packet '{packetId}'")
            .AppendLine()
            .AppendLine(packet);

        WriteToFile<MessagePump>("packets.log", builder, ConsoleColor.Yellow);
    }

    public void TraceNetworkError(Exception ex, NetState state)
    {
        var builder = new StringBuilder()
            .AppendLine($"# {DateTime.UtcNow} @ Client {state}:")
            .AppendLine()
            .AppendLine(ex.ToString());

        WriteToFile<NetState>("network-errors.log", builder, ConsoleColor.Red);
    }

    public void TraceListenerError(Exception ex, Socket socket)
    {
        var builder = new StringBuilder()
            .AppendLine($"# {DateTime.UtcNow} @ Listener socket {socket}:")
            .AppendLine()
            .AppendLine(ex.ToString());

        WriteToFile<Listener>("listener-errors.log", builder, ConsoleColor.Red);
    }

    private void WriteToFile<T>(string fileName, StringBuilder builder, ConsoleColor color)
    {
        try
        {
            if (!_fileLoggers.ContainsKey(fileName))
                _fileLoggers.Add(fileName, new FileLogger(fileName));

            _fileLoggers[fileName].WriteLine(builder);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Could not log file {NAME}", fileName);
        }

        if (color == ConsoleColor.Red)
            _logger.LogError("{Name}: {Information}", typeof(T).Name, builder.ToString());
        else if (color == ConsoleColor.Yellow)
            _logger.LogWarning("{Name}: {Information}", typeof(T).Name, builder.ToString());
        else if (color == ConsoleColor.DarkGray)
            _logger.LogDebug("{Name}: {Information}", typeof(T).Name, builder.ToString());
    }
}
