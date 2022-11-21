using Server.Base.Core.Abstractions;

namespace Server.Reawakened.Data.Modals;

public class ServerConfig : Config
{
    public int RandomKeyLength { get; set; }
    public int PlayerCap { get; set; }
    public string CacheDirectory { get; set; }
    public string BaseDirectory { get; set; }

    public ServerConfig()
    {
        RandomKeyLength = 16;
        PlayerCap = 20;
        BaseDirectory = "";
        CacheDirectory = "";
    }
}
