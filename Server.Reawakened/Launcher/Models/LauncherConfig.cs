using Server.Base.Core.Abstractions;

namespace Server.Reawakened.Launcher.Models;

public class LauncherConfig : IConfig
{
    public string CacheInfoFile { get; set; }
    public string GameSettingsFile { get; set; }
    public string News { get; set; }

    public LauncherConfig() => News = $"You expected there to be news here? It's {DateTime.Now.Year}!";
}
