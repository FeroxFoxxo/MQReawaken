using Server.Base.Core.Abstractions;
using Server.Base.Core.Extensions;
using Server.Base.Logging;
using Server.Base.Network.Enums;
using Server.Base.Timers.Extensions;
using Server.Base.Timers.Services;

namespace Server.Base.Network.Services;

public class NetStateHandler : IService
{
    public delegate void RunProtocol(NetState state, string protocol);

    private readonly Logger _logger;
    private readonly NetworkLogger _networkLogger;
    private readonly TimerThread _thread;

    public readonly Queue<NetState> Disposed;
    public readonly List<NetState> Instances;

    public readonly Dictionary<char, RunProtocol> Protocols;

    public bool Paused;

    public NetStateHandler(Logger logger, NetworkLogger networkLogger, TimerThread thread)
    {
        _logger = logger;
        _networkLogger = networkLogger;
        _thread = thread;

        Instances = new List<NetState>();
        Disposed = new Queue<NetState>();
        Protocols = new Dictionary<char, RunProtocol>();

        Paused = false;
    }

    public void Initialize() =>
        _thread.DelayCall(CheckAllAlive, TimeSpan.FromMinutes(1.0), TimeSpan.FromMinutes(1.5), 0);

    public void ProcessDisposedQueue()
    {
        lock (Disposed)
        {
            var breakout = 200;

            while (--breakout >= 0 && Disposed.Count > 0)
            {
                var netState = Disposed.Dequeue();

                Instances.Remove(netState);

                var userInfo = netState.Account != null
                    ? $"[{netState.Account.Username}]"
                    : "UNKNOWN";

                _logger.WriteLine(ConsoleColor.Red, $"Disconnected. [{Instances.Count} Online] {userInfo}");
            }
        }
    }

    public void Pause()
    {
        Paused = true;

        foreach (var ns in Instances)
        {
            lock (ns.AsyncLock)
                ns.AsyncState |= AsyncStates.Paused;
        }
    }

    public void Resume()
    {
        Paused = false;

        foreach (var ns in Instances.Where(ns => ns.Socket != null))
        {
            lock (ns.AsyncLock)
            {
                ns.AsyncState &= ~AsyncStates.Paused;

                try
                {
                    if ((ns.AsyncState & AsyncStates.Pending) == 0)
                        ns.BeginReceive();
                }
                catch (Exception ex)
                {
                    _networkLogger.TraceNetworkError(ex, ns);
                    ns.Dispose(false);
                }
            }
        }
    }

    public void CheckAllAlive()
    {
        var curTicks = GetTicks.Ticks;

        var instanceCount = Instances.Count;

        while (--instanceCount >= 0)
        {
            var instance = Instances[instanceCount];

            if (instance == null)
                continue;

            try
            {
                instance.CheckAlive(curTicks);
            }
            catch (Exception ex)
            {
                _networkLogger.TraceNetworkError(ex, instance);
            }
        }
    }
}
