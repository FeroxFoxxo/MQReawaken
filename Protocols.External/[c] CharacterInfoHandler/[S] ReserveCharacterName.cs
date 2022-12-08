using Server.Reawakened.Network.Protocols;
using Server.Reawakened.Players.Services;
using Server.Reawakened.XMLs;

namespace Protocols.External._c__CharacterInfoHandler;

public class ReserveCharacterName : ExternalProtocol
{
    public readonly int Count = 4;

    public override string ProtocolName => "cS";

    public NameGenSyllables NameGenSyllables { get; set; }
    public UserInfoHandler UserInfoHandler { get; set; }

    public override void Run(string[] message)
    {
        var isMale = message[5] == "0";
        var name = new[] { message[6], message[7], message[8] };

        if (NameGenSyllables.IsNameReserved(name, UserInfoHandler))
        {
            var names = new List<string[]>();

            for (var i = 0; i < Count; i++)
                names.Add(NameGenSyllables.GetRandomName(isMale, UserInfoHandler));

            SendXt("cT", "0", string.Join('%', names.Select(s => string.Join(',', s))));
        }
        else if (NameGenSyllables.IsPossible(isMale, name))
        {
            SendXt("cT", "1");
        }
        else
        {
            SendXt("cS", "");
        }
    }
}
