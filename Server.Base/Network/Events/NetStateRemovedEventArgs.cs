namespace Server.Base.Network.Events;

public class NetStateRemovedEventArgs
{
    public NetState State { get; }

    public NetStateRemovedEventArgs(NetState state) => State = state;
}
