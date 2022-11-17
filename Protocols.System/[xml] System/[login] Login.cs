using System.Xml;
using Server.Base.Accounts.Enums;
using Server.Base.Accounts.Extensions;
using Server.Base.Accounts.Services;
using Server.Reawakened.Data.Services;
using Server.Reawakened.Network.Protocols;

namespace Protocols.System._xml__SysProtocols;

public class Login : SystemProtocol
{
    public override string ProtocolName => "login";

    public AccountHandler AccountHandler { get; set; }
    public UserInfoHandler UserInfoHandler { get; set; }

    public override void Run(XmlDocument xmlDoc)
    {
        var username = xmlDoc.SelectSingleNode("/msg/body/login/nick")?.FirstChild?.Value;
        var password = xmlDoc.SelectSingleNode("/msg/body/login/pword")?.FirstChild?.Value;

        var reason = AccountHandler.GetAccount(username, password, NetState);
        UserInfoHandler.InitializeUser(username);

        if (reason == AlrReason.Accepted)
            SendXml("logOK",
                $"<login id='{NetState.Account.UserId}' mod='{NetState.Account.IsModerator()}' n='{username}' />");
        else
            SendXml("logKO", $"<login e='{reason.GetErrorValue()}' />");
    }
}
