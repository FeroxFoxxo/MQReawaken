namespace Server.Reawakened.Network.Protocols;

public abstract class ExternalProtocol : BaseProtocol
{
    public abstract void Run(string[] message);
}
