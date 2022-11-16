using Server.Base.Network;
using Server.Reawakened.Data;
using Server.Reawakened.Data.Extensions;
using Server.Reawakened.Data.Services;
using Server.Reawakened.Network.Extensions;

namespace Server.Reawakened.Network.Protocols;

public abstract class BaseProtocol
{
    public NetState NetState;
    public UserHandler UserHandler;

    public abstract string ProtocolName { get; }

    public void InitializeProtocol(NetState state, UserHandler userHandler)
    {
        NetState = state;
        UserHandler = userHandler;
    }

    public User GetUser() =>
        NetState.GetUser(UserHandler);

    public void SendXml(string actionType, string message) =>
        NetState.SendXml(UserHandler, actionType, message);

    public void SendXt(string actionType, params string[] messages) =>
        NetState.SendXt(UserHandler, actionType, messages);
}
