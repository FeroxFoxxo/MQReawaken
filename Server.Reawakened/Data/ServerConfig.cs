using Server.Base.Core.Abstractions;

namespace Server.Reawakened.Data;

public class ServerConfig : IConfig
{
    public int RandomKeyLength { get; set; }
    public int PlayerCap { get; set; }

    public ServerConfig()
    {
        RandomKeyLength = 24;
        PlayerCap = 20;
    }
}
