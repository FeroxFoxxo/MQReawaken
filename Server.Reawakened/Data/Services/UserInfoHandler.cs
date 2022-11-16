using Server.Base.Accounts.Services;
using Server.Base.Core.Helpers;
using Server.Base.Logging;
using Server.Reawakened.Data.Modals;

namespace Server.Reawakened.Data.Services;

public class UserInfoHandler : DataHandler<UserInfo>
{
    private readonly UserHandler _handler;

    public UserInfoHandler(EventSink sink, Logger logger, UserHandler handler) : base(sink, logger) =>
        _handler = handler;

    public void AddUserInfo(string username, UserInfo info)
    {
        _handler.AddUser(username, new User(info));
        Data.Add(username, info);
    }
}
