using System.Xml;
using Server.Reawakened.Data.Extensions;
using Server.Reawakened.Levels.Services;
using Server.Reawakened.Network.Protocols;

namespace Protocols.System._xml__SysProtocols;

public class AutoJoin : SystemProtocol
{
    public override string ProtocolName => "autoJoin";

    public LevelHandler LevelHandler { get; set; }

    public override void Run(XmlDocument xmlDoc)
    {
        var user = GetUser();
        user.JoinLevel(NetState, LevelHandler.GetLevelFromId(0));
        SendXt("cx", user.UserInfo.GetPropertyValues());
        SendXt("cl", $"{user.UserInfo.LastCharacterSelected}{(user.UserInfo.Characters.Count > 0 ? "%" : "")}" +
                     string.Join('%', user.UserInfo.Characters.Select(c => c.ToServerString()))
        );
    }
}
