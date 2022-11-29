using Microsoft.Extensions.Hosting;
using Server.Base.Core.Abstractions;
using Server.Base.Core.Helpers;
using Server.Launcher.Internal;
using Server.Launcher.Models;
using System.Diagnostics;
using System.Runtime.InteropServices;

namespace Server.Launcher.Services;

public class StartGame : IService
{
    private readonly LauncherConfig _lConfig;
    private readonly SettingsConfig _sConfig;
    private readonly IHostApplicationLifetime _appLifetime;
    private readonly EventSink _sink;
    private Process _game;

    public StartGame(EventSink sink, LauncherConfig lConfig, SettingsConfig sConfig,
        IHostApplicationLifetime appLifetime)
    {
        _sink = sink;
        _lConfig = lConfig;
        _sConfig = sConfig;
        _appLifetime = appLifetime;
    }

    public void Initialize()
    {
        _appLifetime.ApplicationStarted.Register(RunGame);
        _sink.WorldLoad += GetGameInformation;
        _sink.Shutdown += StopGame;
    }

    private void StopGame() => _game.CloseMainWindow();

    public void RunGame() =>
        _game = Process.Start(Path.Join(Path.GetDirectoryName(_lConfig.GameSettingsFile), "launcher", "launcher.exe"));

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
