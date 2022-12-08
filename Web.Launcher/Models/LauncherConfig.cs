using Server.Base.Core.Abstractions;

namespace Web.Launcher.Models;

public class LauncherConfig : IConfig
{
    public string CacheInfoFile { get; set; }
    public string GameSettingsFile { get; set; }
    public string News { get; set; }

    public ulong AnalyticsId { get; set; }
    public bool AnalyticsEnabled { get; set; }
    public string AnalyticsApiKey { get; set; }

    public string BaseUrl { get; set; }

    public bool LogAssets { get; set; }
    public bool DisableVersions { get; set; }
    public string CacheLicense { get; set; }

    public LauncherConfig()
    {
        News = $"You expected there to be news here? It's {DateTime.Now.Year}!";

        AnalyticsId = 0;
        AnalyticsEnabled = false;
        AnalyticsApiKey = "ANALYTICS_KEY";
        BaseUrl = "http://localhost";

        LogAssets = true;
        DisableVersions = true;
        CacheLicense = "UNKNOWN";
    }
}
