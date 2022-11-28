using Server.Base.Core.Abstractions;

namespace Server.Reawakened.Data.Modals;

public class ServerConfig : IConfig
{
    public int RandomKeyLength { get; set; }
    public int PlayerCap { get; set; }

    public ServerConfig()
    {
        RandomKeyLength = 16;
        PlayerCap = 20;
    }
}
