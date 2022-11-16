using System.Net;
using System.Net.Sockets;
using Server.Base.Core.Abstractions;
using Server.Base.Core.Events;
using Server.Base.Core.Helpers;
using Server.Base.Core.Models;
using Server.Base.Core.Services;
using Server.Base.Logging;
using Server.Base.Network.Helpers;

namespace Server.Base.Network.Services;

public class MessagePump : IService
{
    private readonly InternalServerConfig _config;
    private readonly NetStateHandler _handler;
    private readonly IPEndPoint[] _ipEndPoints;
    private readonly IpLimiter _limiter;
    private readonly Logger _logger;
    private readonly NetworkLogger _networkLogger;
    private readonly ServerHandler _serverHandler;
    private readonly EventSink _sink;

    public readonly Listener[] Listeners;

    public MessagePump(Logger logger, NetworkLogger networkLogger,
        NetStateHandler handler, IpLimiter limiter,
        InternalServerConfig config, EventSink sink, ServerHandler serverHandler)
    {
        _logger = logger;
        _networkLogger = networkLogger;
        _handler = handler;
        _limiter = limiter;
        _config = config;
        _sink = sink;
        _serverHandler = serverHandler;

        _ipEndPoints = new IPEndPoint[]
        {
            new(IPAddress.Any, 9339)
        };

        Listeners = new Listener[_ipEndPoints.Length];
    }

    public void Initialize()
    {
        _sink.SocketConnect += SocketConnect;

        var success = false;

        do
        {
            for (var i = 0; i < _ipEndPoints.Length; i++)
            {
                Listeners[i] = new Listener(_ipEndPoints[i], _networkLogger, _logger, _serverHandler, _sink);
                success = true;
            }

            if (success) continue;

            _logger.WriteLine(ConsoleColor.Yellow, "MessagePump: Retrying listeners...");

            Thread.Sleep(10000);
        } while (!success);
    }

    private void SocketConnect(SocketConnectEventArgs @event)
    {
        if (!@event.AllowConnection)
            return;

        @event.Socket.SetSocketOption(SocketOptionLevel.Tcp, SocketOptionName.NoDelay, 1);
    }

    public void Slice()
    {
        foreach (var listener in Listeners)
        {
            var accepted = listener.Slice();

            foreach (var socket in accepted)
            {
                NetState netStateInstance = new(socket, _logger, _networkLogger,
                    _handler, _limiter, _config, _sink);

                netStateInstance.Start();

                if (netStateInstance.Running)
                    _logger.WriteLine(ConsoleColor.Green,
                        $"Client: {netStateInstance}: Connected. [{_handler.Instances.Count} Online]"
                    );
            }
        }
    }
}