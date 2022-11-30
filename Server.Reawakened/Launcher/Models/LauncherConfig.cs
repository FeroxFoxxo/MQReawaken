using Server.Base.Core.Abstractions;

namespace Server.Reawakened.Launcher.Models;

public class LauncherConfig : IConfig
{
    public string CacheInfoFile { get; set; }
    public string GameSettingsFile { get; set; }
    public string News { get; set; }

    public ulong AnalyticsId { get; set; }
    public bool AnalyticsEnabled { get; set; }
    public string AnalyticsBaseUrl { get; set; }
    public string AnalyticsApiKey { get; set; }

    public LauncherConfig()
    {
        News = $"You expected there to be news here? It's {DateTime.Now.Year}!";

        AnalyticsId = 0;
        AnalyticsEnabled = false;
        AnalyticsBaseUrl = "http://localhost/analytics";
        AnalyticsApiKey = "ANALYTICS_KEY";
    }
}
