using Newtonsoft.Json;
using Server.Base.Core.Abstractions;
using System.Dynamic;

namespace Web.Launcher.Models;

public class SettingsConfig : IConfig
{
    public string BaseUrl { get; set; }
    public bool Fullscreen { get; set; }
    public bool OnGameClosePopup { get; set; }

    public SettingsConfig()
    {
        BaseUrl = "http://localhost";
        Fullscreen = false;
        OnGameClosePopup = false;
    }

    public void WriteToSettings(LauncherConfig config)
    {
        if (config.GameSettingsFile == null)
            return;

        dynamic settings = JsonConvert.DeserializeObject<ExpandoObject>(File.ReadAllText(config.GameSettingsFile))!;
        settings.launcher.baseUrl = BaseUrl;
        settings.launcher.fullscreen = Fullscreen ? "true" : "false";
        settings.launcher.onGameClosePopup = OnGameClosePopup ? "true" : "false";
        settings.patcher.baseUrl = BaseUrl;
        File.WriteAllText(config.GameSettingsFile, JsonConvert.SerializeObject(settings));
    }
}
