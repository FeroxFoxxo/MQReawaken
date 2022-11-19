using System.ComponentModel;

namespace Server.Reawakened.Game;

public class CommandLine
{
    [Description("login.fullscreen")] public bool Fullscreen { get; set; }

    [Description("login.autologin")] public bool AutoLogin { get; set; }

    [Description("login.user")] public string User { get; set; }

    [Description("login.puid")] public string NickUser { get; set; }

    [Description("login.host")] public string Host { get; set; }

    [Description("login.character")] public string Character { get; set; }

    [Description("login.devuser")] public string DevUser { get; set; }

    [Description("setting.quality")] public string Quality { get; set; }

    [Description("login.pw")] public string Pw { get; set; }

    [Description("login.pk")] public string Pk { get; set; }

    [Description("sid")] public string Sid { get; set; }

    [Description("screen-width")] public int ScreenWidth { get; set; }

    [Description("screen-height")] public int ScreenHeight { get; set; }

    [Description("ongameclosepopup")] public string OnGameClosePopup { get; set; }

    [Description("analytics.sessionId")] public string SessionId { get; set; }

    [Description("analytics.accountCreated")]
    public string AccountCreated { get; set; }

    [Description("analytics.trackingshortid")]
    public string TrackingShortId { get; set; }

    [Description("analytics.firsttimelogin")]
    public string FirstTimeLogin { get; set; }
}
