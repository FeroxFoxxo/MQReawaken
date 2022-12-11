using Microsoft.Extensions.Logging;
using Server.Base.Core.Extensions;
using Server.Base.Core.Helpers;
using Server.Base.Core.Models;
using Server.Base.Logging;
using Server.Base.Network.Enums;
using Server.Base.Network.Events;
using Server.Base.Network.Helpers;
using Server.Base.Network.Services;
using System.Net;
using System.Net.Sockets;
using System.Text;

namespace Server.Base.Network;

public class NetState : IDisposable
{
    public delegate bool ThrottlePacketCallback(NetState state);

    private readonly NetStateHandler _handler;
    private readonly ILogger<MessagePump> _logger;
    private readonly NetworkLogger _networkLogger;
    private readonly EventSink _sink;

    private readonly string _toString;
    public readonly IPAddress Address;
    public readonly object AsyncLock;
    public readonly byte[] Buffer;
    public readonly DateTime ConnectedOn;
    public readonly int UpdateRange;

    private readonly Dictionary<Type, INetStateData> _data;
    private bool _disposing;
    private double _nextCheckActivity;

    private AsyncCallback _onReceiveCallback, _onSendCallback;

    public AsyncStates AsyncState;
    public ThrottlePacketCallback Throttler;

    public Socket Socket { get; private set; }
    public bool Running { get; private set; }

    public NetState(Socket socket, ILogger<MessagePump> logger,
        NetworkLogger networkLogger, NetStateHandler handler, IpLimiter limiter,
        InternalServerConfig config, EventSink sink)
    {
        Socket = socket;
        AsyncLock = new object();
        Buffer = new byte[config.BufferSize];

        _logger = logger;
        _networkLogger = networkLogger;
        _handler = handler;
        _sink = sink;

        _nextCheckActivity = GetTicks.Ticks + 30000000;

        _handler.Instances.Add(this);
        _data = new Dictionary<Type, INetStateData>();

        try
        {
            Address = limiter.Intern(((IPEndPoint)Socket.RemoteEndPoint)?.Address);
            _toString = Address.ToString();
        }
        catch (Exception ex)
        {
            networkLogger.TraceNetworkError(ex, this);
            Address = IPAddress.None;
            _toString = "(error)";
        }

        ConnectedOn = DateTime.UtcNow;

        UpdateRange = config.GlobalUpdateRange;

        _sink.InvokeNetStateAdded(new NetStateAddedEventArgs(this));
    }

    public void WriteServer(string text) =>
        _logger.LogTrace("{NetState}: {Written} (SERVER)", this, text);

    public void WriteClient(string text) =>
        _logger.LogTrace("{NetState}: {Written} (CLIENT)", this, text);

    public void CheckAlive(double curTicks)
    {
        if (Socket == null)
            return;

        if (_nextCheckActivity - curTicks >= 0)
            return;

        _logger.LogError("{NetState}: Disconnecting due to inactivity...", this);

        Dispose();
    }

    public void Start()
    {
        _onReceiveCallback = OnReceive;
        _onSendCallback = OnSend;

        Running = true;

        lock (_handler.Disposed)
        {
            if (Socket == null || _handler.Paused)
                return;

            _logger.LogInformation("{NetState}: Connected. [{Count} Online]", this, _handler.Instances.Count);
        }

        try
        {
            lock (AsyncLock)
            {
                if ((AsyncState & (AsyncStates.Pending | AsyncStates.Paused)) == 0)
                    BeginReceive();
            }
        }
        catch (Exception ex)
        {
            _networkLogger.TraceNetworkError(ex, this);
            Dispose();
        }
    }

    public void BeginReceive()
    {
        AsyncState |= AsyncStates.Pending;

        Socket.BeginReceive(Buffer, 0, Buffer.Length, SocketFlags.None, _onReceiveCallback, Socket);
    }

    public void Send(string packet)
    {
        if (Socket == null)
            return;

        packet += "\0";

        WriteServer(packet);

        var buffer = Encoding.UTF8.GetBytes(packet);
        var length = buffer.Length;

        if (buffer.Length <= 0)
            return;

        Socket.BeginSend(buffer, 0, length, SocketFlags.None, _onSendCallback, Socket);
    }

    private void OnSend(IAsyncResult asyncResult)
    {
        var socket = (Socket)asyncResult.AsyncState;

        try
        {
            var bytes = socket?.EndSend(asyncResult);

            if (bytes <= 0)
            {
                Dispose();
                return;
            }

            _nextCheckActivity = GetTicks.Ticks + 90000000;
        }
        catch (Exception)
        {
            Dispose();
        }
    }

    private void OnReceive(IAsyncResult asyncResult)
    {
        try
        {
            var socket = (Socket)asyncResult.AsyncState;

            if (socket == null) return;
            var byteCount = socket.EndReceive(asyncResult);

            if (byteCount > 0)
            {
                _nextCheckActivity = GetTicks.Ticks + 900000000;

                if (Throttler != null)
                    if (!Throttler(this))
                        return;
                    else
                        Throttler = null;

                var buffered = new byte[byteCount];

                lock (AsyncLock)
                    Array.Copy(Buffer, buffered, byteCount);

                var packet = Encoding.UTF8.GetString(buffered);

                WriteClient(packet);

                lock (_handler.Protocols)
                {
                    if (_handler.Protocols.ContainsKey(packet[0]))
                        _handler.Protocols[packet[0]](this, packet);
                    else
                        _networkLogger.TracePacketError(packet[0].ToString(), packet, this);
                }

                lock (AsyncLock)
                {
                    AsyncState &= ~AsyncStates.Pending;

                    if ((AsyncState & AsyncStates.Paused) != 0) return;
                    try
                    {
                        BeginReceive();
                    }
                    catch (Exception ex)
                    {
                        _networkLogger.TraceNetworkError(ex, this);
                        Dispose();
                    }
                }
            }
            else
            {
                Dispose();
            }
        }
        catch (Exception ex)
        {
            _networkLogger.TraceNetworkError(ex, this);
            Dispose();
        }
    }

    public virtual void Dispose()
    {
        if (Socket == null || _disposing)
            return;

        _disposing = true;

        try
        {
            Socket.Shutdown(SocketShutdown.Both);
        }
        catch (SocketException ex)
        {
            _networkLogger.TraceNetworkError(ex, this);
        }

        try
        {
            Socket.Close();
        }
        catch (SocketException ex)
        {
            _networkLogger.TraceNetworkError(ex, this);
        }

        _sink.InvokeNetStateRemoved(new NetStateRemovedEventArgs(this));

        Socket = null;

        _onReceiveCallback = null;
        _onSendCallback = null;

        Throttler = null;

        Running = false;

        lock (_handler.Disposed)
            _handler.Disposed.Enqueue(this);
    }

    public override string ToString() => _toString;

    public T Get<T>() where T : class => !_data.ContainsKey(typeof(T)) ? null : _data[typeof(T)] as T;

    public void Set<T>(T data) where T : INetStateData => _data.Add(typeof(T), data);

    public void RemoveAllData()
    {
        foreach (var data in _data)
        {
            lock (_handler.Disposed)
                data.Value?.RemovedState(this, _handler, _logger);
        }
    }
}
