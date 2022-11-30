using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Server.Base.Core.Abstractions;
using Server.Base.Core.Helpers;
using Server.Reawakened.Launcher.Internal;
using Server.Reawakened.Launcher.Models;
using System.Diagnostics;
using System.Runtime.InteropServices;

namespace Server.Reawakened.Launcher.Services;

public class StartGame : IService
{
    private readonly LauncherConfig _lConfig;
    private readonly SettingsConfig _sConfig;
    private readonly IHostApplicationLifetime _appLifetime;
    private readonly ILogger<StartGame> _logger;
    private readonly EventSink _sink;
    private Process _game;

    public string CurrentVersion { get; private set; }

    public StartGame(EventSink sink, LauncherConfig lConfig, SettingsConfig sConfig,
        IHostApplicationLifetime appLifetime, ILogger<StartGame> logger)
    {
        _sink = sink;
        _lConfig = lConfig;
        _sConfig = sConfig;
        _appLifetime = appLifetime;
        _logger = logger;
    }

    public void Initialize()
    {
        _appLifetime.ApplicationStarted.Register(RunGame);
        _sink.WorldLoad += GetGameInformation;
        _sink.Shutdown += StopGame;
    }

    private void StopGame() => _game.CloseMainWindow();

    public void RunGame()
    {
        if (string.IsNullOrEmpty(_lConfig.GameSettingsFile))
        {
            _logger.LogInformation("Please enter the absolute file path for your game's 'settings.txt' file.");
            _lConfig.GameSettingsFile = Console.ReadLine();
        }

        var directory = Path.GetDirectoryName(_lConfig.GameSettingsFile);
        _game = Process.Start(Path.Join(directory, "launcher", "launcher.exe"));
        CurrentVersion = File.ReadAllText(Path.Join(directory, "current.txt"));

        if (string.IsNullOrEmpty(_lConfig.CacheInfoFile))
        {
            _logger.LogInformation("Please enter the absolute file path for your cache's ROOT '__info' file.");
            _lConfig.CacheInfoFile = Console.ReadLine();
        }

        _logger.LogInformation("Run game for process: {GamePath}", _game?.ProcessName);
    }

    private void GetGameInformation()
    {
        _lConfig.GameSettingsFile = SetIfNotNull(_lConfig.GameSettingsFile, "Get Settings File",
            "Settings File (*.txt)\0*.txt\0");

        _lConfig.CacheInfoFile = SetIfNotNull(_lConfig.CacheInfoFile, "Get Root Cache Info",
            "Root Info File (__info)\0__info\0");

        _sConfig.WriteToSettings(_lConfig);
    }

    private static string SetIfNotNull(string setting, string title, string filter)
    {
        var isWindows = RuntimeInformation.IsOSPlatform(OSPlatform.Windows);

        return string.IsNullOrEmpty(setting)
            ? isWindows
                ? FileDialog.GetFile(title, filter)
                : Console.ReadLine()
            : setting;
    }
}
