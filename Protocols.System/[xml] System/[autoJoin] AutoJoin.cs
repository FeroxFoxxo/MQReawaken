using Server.Base.Accounts.Modals;
using Server.Reawakened.Data;
using Server.Reawakened.Data.Extensions;
using Server.Reawakened.Levels.Services;
using Server.Reawakened.Network.Protocols;
using System.Xml;

namespace Protocols.System._xml__System;

public class AutoJoin : SystemProtocol
{
    public override string ProtocolName => "autoJoin";

    public LevelHandler LevelHandler { get; set; }

    public override void Run(XmlDocument xmlDoc)
    {
        var user = NetState.Get<Player>();
        var account = NetState.Get<Account>();
        user.JoinLevel(NetState, LevelHandler.GetLevelFromId(0));
        SendXt("cx", user.UserInfo.GetPropertyValues(account));
        SendXt("cl", $"{user.UserInfo.LastCharacterSelected}{(user.UserInfo.Characters.Count > 0 ? "%" : "")}" +
                     string.Join('%', user.UserInfo.Characters.Select(c => c.ToServerString()))
        );
    }
}
