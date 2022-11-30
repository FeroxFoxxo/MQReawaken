using Server.Base.Accounts.Enums;
using Server.Base.Accounts.Extensions;
using Server.Base.Accounts.Modals;
using Server.Base.Accounts.Services;
using Server.Reawakened.Data.Services;
using Server.Reawakened.Network.Protocols;
using System.Xml;

namespace Protocols.System._xml__System;

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

        if (reason == AlrReason.Accepted)
        {
            UserInfoHandler.InitializeUser(NetState);
            SendXml("logOK",
                $"<login id='{NetState.Get<Account>().UserId}' mod='{NetState.Get<Account>().IsModerator()}' n='{username}' />");
        }
        else
        {
            SendXml("logKO", $"<login e='{reason.GetErrorValue()}' />");
        }
    }
}
