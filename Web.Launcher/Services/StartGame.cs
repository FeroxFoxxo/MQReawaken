using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Server.Base.Core.Abstractions;
using Server.Base.Core.Helpers;
using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Xml.Linq;
using Web.AssetBundles.Helpers;
using Web.AssetBundles.Models;
using Web.Launcher.Internal;
using Web.Launcher.Models;

namespace Web.Launcher.Services;

public class StartGame : IService
{
    private readonly IHostApplicationLifetime _appLifetime;
    private readonly LauncherConfig _lConfig;
    private readonly ILogger<StartGame> _logger;
    private readonly BuildAssetBundles _buildBundles;
    private readonly SettingsConfig _sConfig;
    private readonly AssetBundleConfig _aBConfig;
    private readonly EventSink _sink;
    private string _directory;
    private bool _dirSet, _appStart;
    private Process _game;

    public string CurrentVersion { get; private set; }

    public StartGame(EventSink sink, LauncherConfig lConfig, SettingsConfig sConfig,
        IHostApplicationLifetime appLifetime, ILogger<StartGame> logger, BuildAssetBundles buildBundles,
        AssetBundleConfig aBConfig)
    {
        _sink = sink;
        _lConfig = lConfig;
        _sConfig = sConfig;
        _appLifetime = appLifetime;
        _logger = logger;
        _buildBundles = buildBundles;
        _aBConfig = aBConfig;

        _dirSet = false;
        _appStart = false;
    }

    public void Initialize()
    {
        _appLifetime.ApplicationStarted.Register(AppStarted);
        _sink.WorldLoad += GetGameInformation;
        _sink.Shutdown += StopGame;
    }

    private void AppStarted()
    {
        _appStart = true;
        RunGame();
    }

    private void RunGame()
    {
        if (!_appStart || !_dirSet)
            return;

        WriteConfig();

        _buildBundles.GenerateAssetBundles();

        _game = Process.Start(Path.Join(_directory, "launcher", "launcher.exe"));
        _logger.LogDebug("Running game on process: {GamePath}", _game?.ProcessName);
    }

    private void StopGame() => _game.CloseMainWindow();

    public void EnsureSet()
    {
        while (true)
        {
            _logger.LogInformation("Getting Game Executable");

            if (string.IsNullOrEmpty(_lConfig.GameSettingsFile) || !_lConfig.GameSettingsFile.EndsWith("settings.txt"))
            {
                _logger.LogError("Please enter the absolute file path for your game's 'settings.txt' file.");
                _lConfig.GameSettingsFile = Console.ReadLine();
                continue;
            }

            _directory = Path.GetDirectoryName(_lConfig.GameSettingsFile);

            if (string.IsNullOrEmpty(_directory))
                continue;

            CurrentVersion = File.ReadAllText(Path.Join(_directory, "current.txt"));

            _logger.LogDebug("Got launcher directory: {Directory}", Path.GetDirectoryName(_lConfig.GameSettingsFile));
            break;
        }

        while (true)
        {
            _logger.LogInformation("Getting Cache Directory");

            if (string.IsNullOrEmpty(_aBConfig.CacheInfoFile) || !_aBConfig.CacheInfoFile.EndsWith("__info"))
            {
                _logger.LogError("Please enter the absolute file path for your cache's ROOT '__info' file.");
                _aBConfig.CacheInfoFile = Console.ReadLine() ?? string.Empty;
                continue;
            }

            _logger.LogDebug("Got cache directory: {Directory}", Path.GetDirectoryName(_aBConfig.CacheInfoFile));
            break;
        }

        _dirSet = true;
        RunGame();
    }

    private void WriteConfig()
    {
        var config = Path.Join(_directory, "game", "LocalBuildConfig.xml");

        var oldDoc = XDocument.Load(config);
        var newDoc = new XDocument();

        var root = oldDoc.Elements().FirstOrDefault(x => x.Name == "MQBuildConfg") ??
                   new XElement("MQBuildConfg");

        foreach (var item in GetConfigValues())
        {
            var xmlItem = root.Elements().FirstOrDefault(x => x.Attributes().Any(a =>
                a.Name == "name" && a.Value == item.Key
            ));

            if (xmlItem == null)
            {
                xmlItem = new XElement("item");
                xmlItem.Add(new XAttribute("name", item.Key));
                xmlItem.Add(new XAttribute("value", item.Value));
                root.Add(xmlItem);
            }
            else
            {
                xmlItem.Attributes().First(a => a.Name == "value").Value = item.Value;
            }
        }

        newDoc.Add(root);
        newDoc.Save(config);
    }

    private Dictionary<string, string> GetConfigValues()
    {
        // Split to avoid search engine indexing, I don't
        // believe we're doing anything wrong, but I'd
        // rather not chance it. Trademark ran out in 2018.

        const string name = "mon" + "key" + "que" + "st";

        return new Dictionary<string, string>
        {
            { $"{name}.unity.cache.domain", $"{_lConfig.BaseUrl}/Cache" },
            { $"{name}.unity.cache.license", $"{_lConfig.CacheLicense}" },
            { $"{name}.unity.cache.size", "0" },
            { $"{name}.unity.cache.expiration", "0" },
            { "asset.log", _lConfig.LogAssets ? "true" : "false" },
            { "asset.disableversioning", _lConfig.DisableVersions ? "true" : "false" },
            { "asset.jboss", $"{_lConfig.BaseUrl}/Apps" },
            { "asset.bundle", $"{_lConfig.BaseUrl}/Client/Bundles" },
            { "asset.audio", $"{_lConfig.BaseUrl}/Client/Audio" },
            { "logout.url", $"{_lConfig.BaseUrl}/Logout" },
            { "contactus.url", $"{_lConfig.BaseUrl}/Contact" },
            { "tools.urlbase", $"{_lConfig.BaseUrl}/Tools" },
            { "leaderboard.domain", $"{_lConfig.BaseUrl}/Leaderboard" },
            { "analytics.baseurl", $"{_lConfig.BaseUrl}/Analytics" },
            { "analytics.enabled", _lConfig.AnalyticsEnabled ? "true" : "false" },
            { "analytics.apikey", _lConfig.AnalyticsApiKey }
        };
    }

    private void GetGameInformation()
    {
        try
        {
            _lConfig.GameSettingsFile = SetIfNotNull(_lConfig.GameSettingsFile, "Get Settings File",
                "Settings File (*.txt)\0*.txt\0");

            _aBConfig.CacheInfoFile = SetIfNotNull(_aBConfig.CacheInfoFile, "Get Root Cache Info",
                "Root Info File (__info)\0__info\0");

            _sConfig.WriteToSettings(_lConfig);
        }
        catch
        {
            // ignored
        }

        EnsureSet();
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
