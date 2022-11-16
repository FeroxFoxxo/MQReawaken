using Server.Reawakened.Data.Services;
using Server.Reawakened.Network.Protocols;
using Server.Reawakened.XMLs;

namespace Protocols.External._c__CharacterInfoHandler;

public class RandomName : ExternalProtocol
{
    public override string ProtocolName => "cR";

    public NameGenSyllables NameGenSyllables { get; set; }
    public UserInfoHandler UserInfoHandler { get; set; }

    public override void Run(string[] message) =>
        SendXt("cR", NameGenSyllables.GetRandomName(message[5] == "0", UserInfoHandler));
}
