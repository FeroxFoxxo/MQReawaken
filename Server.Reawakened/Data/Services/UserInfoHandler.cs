using Microsoft.Extensions.Logging;
using Server.Base.Accounts.Services;
using Server.Base.Core.Helpers;
using Server.Reawakened.Data.Modals;

namespace Server.Reawakened.Data.Services;

public class UserInfoHandler : DataHandler<UserInfo>
{
    private readonly UserHandler _handler;

    public UserInfoHandler(EventSink sink, ILogger<UserInfo> logger, UserHandler handler) : base(sink, logger) =>
        _handler = handler;

    public void AddUserInfo(string username, UserInfo info)
    {
        if (!_handler.Users.ContainsKey(username))
            _handler.AddUser(username, new User(info));

        if (!Data.ContainsKey(username))
            Data.Add(username, info);
    }

    public void InitializeUser(string username) =>
        AddUserInfo(username, Data.ContainsKey(username) ? Data[username] : new UserInfo());
}
