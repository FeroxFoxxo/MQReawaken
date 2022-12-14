using Server.Base.Core.Events;
using Server.Base.Network.Events;
using Server.Base.Worlds.Events;

namespace Server.Base.Core.Helpers;

public class EventSink
{
    public delegate void CrashedEventHandler(CrashedEventArgs @event);

    public delegate void InternalShutdownEventHandler();

    public delegate void NetStateAddedHandler(NetStateAddedEventArgs @event);

    public delegate void NetStateRemovedHandler(NetStateRemovedEventArgs @event);

    public delegate void ServerStartedEventHandler(ServerStartedEventArgs @event);

    public delegate void ShutdownEventHandler();

    public delegate void SocketConnectEventHandler(SocketConnectEventArgs @event);

    public delegate void WorldBroadcastEventHandler(WorldBroadcastEventArgs @event);

    public delegate void WorldLoadEventHandler();

    public delegate void WorldSaveEventHandler(WorldSaveEventArgs @event);

    public event CrashedEventHandler Crashed;
    public event ShutdownEventHandler Shutdown;
    public event InternalShutdownEventHandler InternalShutdown;
    public event ServerStartedEventHandler ServerStarted;
    public event SocketConnectEventHandler SocketConnect;
    public event WorldLoadEventHandler WorldLoad;
    public event WorldSaveEventHandler WorldSave;
    public event WorldBroadcastEventHandler WorldBroadcast;
    public event NetStateRemovedHandler NetStateRemoved;
    public event NetStateAddedHandler NetStateAdded;

    public void InvokeCrashed(CrashedEventArgs @event) => Crashed?.Invoke(@event);
    public void InvokeShutdown() => Shutdown?.Invoke();
    public void InvokeInternalShutdown() => InternalShutdown?.Invoke();
    public void InvokeServerStarted(ServerStartedEventArgs e) => ServerStarted?.Invoke(e);
    public void InvokeSocketConnect(SocketConnectEventArgs e) => SocketConnect?.Invoke(e);
    public void InvokeWorldLoad() => WorldLoad?.Invoke();
    public void InvokeWorldSave(WorldSaveEventArgs e) => WorldSave?.Invoke(e);
    public void InvokeWorldBroadcast(WorldBroadcastEventArgs e) => WorldBroadcast?.Invoke(e);
    public void InvokeNetStateRemoved(NetStateRemovedEventArgs e) => NetStateRemoved?.Invoke(e);
    public void InvokeNetStateAdded(NetStateAddedEventArgs e) => NetStateAdded?.Invoke(e);
}
