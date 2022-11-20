using Microsoft.Extensions.Logging;
using Server.Base.Core.Events;
using Server.Base.Core.Helpers;
using Server.Base.Core.Services;
using Server.Base.Logging;
using Server.Base.Network.Services;
using System.Collections;
using System.Net;
using System.Net.NetworkInformation;
using System.Net.Sockets;

namespace Server.Base.Network;

public class Listener : IDisposable
{
    private readonly Queue<Socket> _accepted;
    private readonly object _acceptedSyncRoot;
    private readonly Socket[] _emptySockets = Array.Empty<Socket>();
    private readonly ServerHandler _handler;
    private readonly ILogger<MessagePump> _logger;
    private readonly NetworkLogger _networkLogger;
    private readonly AsyncCallback _onAccept;
    private readonly EventSink _sink;

    private Socket _listener;

    public Listener(IPEndPoint ipEp, NetworkLogger networkLogger, ILogger<MessagePump> logger, ServerHandler handler,
        EventSink sink)
    {
        _networkLogger = networkLogger;
        _logger = logger;
        _handler = handler;
        _sink = sink;
        _accepted = new Queue<Socket>();
        _acceptedSyncRoot = ((ICollection)_accepted).SyncRoot;

        _listener = Bind(ipEp);

        if (_listener == null)
            return;

        DisplayListener();

        _onAccept = OnAccept;

        try
        {
            _listener.BeginAccept(_onAccept, _listener);
        }
        catch (SocketException ex)
        {
            networkLogger.TraceListenerError(ex, _listener);
        }
        catch (ObjectDisposedException)
        {
        }
    }

    public void Dispose()
    {
        var socket = Interlocked.Exchange(ref _listener, null);

        socket?.Close();

        GC.SuppressFinalize(this);
    }

    private Socket Bind(IPEndPoint ipEndPoint)
    {
        Socket socket = new(ipEndPoint.AddressFamily, SocketType.Stream, ProtocolType.Tcp);

        try
        {
            if (socket.LingerState != null) socket.LingerState.Enabled = false;

            socket.ExclusiveAddressUse = false;

            socket.Bind(ipEndPoint);
            socket.Listen(8);

            return socket;
        }
        catch (Exception ex)
        {
            if (ex is not SocketException socketException)
                return null;

            switch (socketException.ErrorCode)
            {
                case 10048:
                    _logger.LogError("Failed: {Address}:{Port} (In Use)", ipEndPoint.Address, ipEndPoint.Port);
                    break;
                case 10049:
                    _logger.LogError("Failed: {Address}:{Port} (Unavailable)", ipEndPoint.Address, ipEndPoint.Port);
                    break;
                default:
                    _logger.LogError(ex, "Could not connect to socket");
                    break;
            }

            return null;
        }
    }

    private void DisplayListener()
    {
        if (_listener.LocalEndPoint is not IPEndPoint ipEndPoint)
            return;

        if (ipEndPoint.Address.Equals(IPAddress.Any) || ipEndPoint.Address.Equals(IPAddress.IPv6Any))
        {
            var adapters = NetworkInterface.GetAllNetworkInterfaces();
            foreach (var adapter in adapters)
            {
                var properties = adapter.GetIPProperties();
                foreach (var uniCast in properties.UnicastAddresses)
                {
                    if (ipEndPoint.AddressFamily == uniCast.Address.AddressFamily)
                        _logger.LogInformation("Listening: {Address}:{Port}", uniCast.Address, ipEndPoint.Port);
                }
            }
        }
        else
        {
            _logger.LogInformation("Listening: {Address}:{Port}", ipEndPoint.Address, ipEndPoint.Port);
        }
    }

    private void OnAccept(IAsyncResult asyncResult)
    {
        var listener = (Socket)asyncResult.AsyncState;

        Socket accepted = null;

        try
        {
            if (listener != null)
                accepted = listener.EndAccept(asyncResult);
        }
        catch (SocketException ex)
        {
            _networkLogger.TraceListenerError(ex, _listener);
        }
        catch (ObjectDisposedException)
        {
            return;
        }

        if (accepted != null)
            if (VerifySocket(accepted))
                Enqueue(accepted);
            else
                Release(accepted);

        try
        {
            listener?.BeginAccept(_onAccept, listener);
        }
        catch (SocketException ex)
        {
            _networkLogger.TraceListenerError(ex, _listener);
        }
        catch (ObjectDisposedException)
        {
        }
    }

    private bool VerifySocket(Socket socket)
    {
        try
        {
            SocketConnectEventArgs args = new(socket);

            _sink.InvokeSocketConnect(args);

            return args.AllowConnection;
        }
        catch (Exception ex)
        {
            _networkLogger.TraceListenerError(ex, _listener);

            return false;
        }
    }

    private void Enqueue(Socket socket)
    {
        lock (_acceptedSyncRoot)
            _accepted.Enqueue(socket);

        _handler.Set();
    }

    private void Release(Socket socket)
    {
        try
        {
            socket.Shutdown(SocketShutdown.Both);
        }
        catch (SocketException ex)
        {
            _networkLogger.TraceListenerError(ex, _listener);
        }

        try
        {
            socket.Close();
        }
        catch (SocketException ex)
        {
            _networkLogger.TraceListenerError(ex, _listener);
        }
    }

    public Socket[] Slice()
    {
        Socket[] socketArray;

        lock (_acceptedSyncRoot)
        {
            if (_accepted.Count == 0)
                return _emptySockets;

            socketArray = _accepted.ToArray();
            _accepted.Clear();
        }

        return socketArray;
    }
}
