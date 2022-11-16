using Server.Base.Network;
using Server.Reawakened.Data.Services;

namespace Server.Reawakened.Data.Extensions;

public static class FetchUser
{
    public static User GetUser(this NetState state, UserHandler handler)
    {
        var name = state.Account?.Username;

        return name != null ? handler.Users[name] : null;
    }
}
