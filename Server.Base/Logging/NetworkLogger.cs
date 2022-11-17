using System.Net.Sockets;
using System.Text;
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
    private readonly Logger _logger;

    public NetworkLogger(Logger logger)
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

    public void TraceNetworkError(Exception exception, NetState state)
    {
        var builder = new StringBuilder()
            .AppendLine($"# {DateTime.UtcNow} @ Client {state}:")
            .AppendLine()
            .AppendLine(exception.ToString());

        WriteToFile<NetState>("network-errors.log", builder, ConsoleColor.Red);
    }

    public void TraceListenerError(Exception exception, Socket socket)
    {
        var builder = new StringBuilder()
            .AppendLine($"# {DateTime.UtcNow} @ Listener socket {socket}:")
            .AppendLine()
            .AppendLine(exception.ToString());

        WriteToFile<Listener>("listener-errors.log", builder, ConsoleColor.Red);
    }

    private void WriteToFile<T>(string fileName, StringBuilder builder, ConsoleColor color)
    {
        try
        {
            if (!_fileLoggers.ContainsKey(fileName))
                _fileLoggers.Add(fileName, new FileLogger(fileName, true));

            _fileLoggers[fileName].WriteLine(builder);
        }
        catch (Exception ex)
        {
            _logger.LogException<NetworkLogger>(ex);
        }

        _logger.WriteLine<T>(color, builder.ToString());
    }
}
