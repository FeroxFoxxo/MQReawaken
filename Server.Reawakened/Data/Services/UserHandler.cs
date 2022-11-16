using Server.Base.Core.Abstractions;
using Server.Base.Core.Helpers;
using Server.Base.Network.Events;

namespace Server.Reawakened.Data.Services;

public class UserHandler : IService
{
    private readonly EventSink _sink;
    public readonly Dictionary<string, User> Users;

    public UserHandler(EventSink sink)
    {
        _sink = sink;
        Users = new Dictionary<string, User>();
    }

    public void Initialize() => _sink.NetStateRemoved += RemoveUser;

    private void RemoveUser(NetStateRemovedEventArgs @event)
    {
        var netState = @event.State;

        var name = netState.Account?.Username;
        if (!string.IsNullOrEmpty(name))
            Users.Remove(name);
    }

    public void AddUser(string username, User user) => Users.Add(username, user);
}
