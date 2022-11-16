namespace Server.Base.Network.Events;

public class NetStateAddedEventArgs
{
    public NetState State { get; }

    public NetStateAddedEventArgs(NetState state) => State = state;
}
