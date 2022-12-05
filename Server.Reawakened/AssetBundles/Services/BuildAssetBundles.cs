using Server.Base.Core.Abstractions;
using Server.Reawakened.Launcher.Helpers;
using Server.Reawakened.Launcher.Models;

namespace Server.Reawakened.AssetBundles.Services;

public class BuildAssetBundles : IService
{
    private readonly LauncherConfig _lConfig;
    private readonly LauncherSink _sink;

    public BuildAssetBundles(LauncherSink sink, LauncherConfig lConfig)
    {
        _sink = sink;
        _lConfig = lConfig;
    }

    public void Initialize() => _sink.GameLaunching += GameLaunching;

    private void GameLaunching()
    {
    }
}
