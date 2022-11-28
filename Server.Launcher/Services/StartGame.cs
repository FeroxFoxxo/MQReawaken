using Server.Base.Core.Abstractions;
using Server.Base.Core.Helpers;
using Server.Launcher.Internal;
using Server.Launcher.Models;
using System.Runtime.InteropServices;

namespace Server.Launcher.Services;

public class StartGame : IService
{
    private readonly LauncherConfig _lConfig;
    private readonly SettingsConfig _sConfig;
    private readonly EventSink _sink;

    public StartGame(EventSink sink, LauncherConfig lConfig, SettingsConfig sConfig)
    {
        _sink = sink;
        _lConfig = lConfig;
        _sConfig = sConfig;
    }

    public void Initialize() => _sink.WorldLoad += GetGameInformation;

    private void GetGameInformation()
    {
        _lConfig.GameSettingsFile = SetIfNotNull(_lConfig.GameSettingsFile, "Get Settings File",
            "Settings File (*.txt)\0*.txt\0");

        _lConfig.CacheInfoFile = SetIfNotNull(_lConfig.CacheInfoFile, "Get Root Cache Info",
            "Root Info File (__info)\0__info\0");

        _sConfig.WriteToSettings(_lConfig);
    }

    private static string? SetIfNotNull(string? setting, string title, string filter)
    {
        var isWindows = RuntimeInformation.IsOSPlatform(OSPlatform.Windows);

        return string.IsNullOrEmpty(setting)
            ? isWindows
                ? FileDialog.GetFile(title, filter)
                : Console.ReadLine()
            : setting;
    }
}
