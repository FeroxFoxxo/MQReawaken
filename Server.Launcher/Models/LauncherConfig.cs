using Server.Base.Core.Abstractions;

namespace Server.Launcher.Models;

public class LauncherConfig : IConfig
{
    public string? CacheInfoFile { get; set; }
    public string? GameSettingsFile { get; set; }
}
