using Newtonsoft.Json;
using Server.Base.Core.Abstractions;
using System.Dynamic;

namespace Server.Launcher.Models;

public class SettingsConfig : IConfig
{
    public string BaseUrl { get; set; }
    public bool Fullscreen { get; set; }
    public bool OnGameClosePopup { get; set; }
    public string DefaultNews { get; set; }

    public SettingsConfig()
    {
        BaseUrl = "http://localhost";
        Fullscreen = false;
        OnGameClosePopup = false;
        DefaultNews = "You expected there to be news here? This far after closing?";
    }

    public void WriteToSettings(LauncherConfig config)
    {
        if (config.GameSettingsFile == null)
            return;

        dynamic settings = JsonConvert.DeserializeObject<ExpandoObject>(config.GameSettingsFile)!;
        settings.launcher.baseUrl = BaseUrl;
        settings.launcher.fullscreen = Fullscreen;
        settings.launcher.defaultNews = DefaultNews;
        settings.launcher.onGameClosePopup = OnGameClosePopup;
        settings.patcher.baseUrl = BaseUrl;
    }
}
