using AssetStudio;
using Microsoft.Extensions.Logging;
using Server.Base.Core.Abstractions;
using Server.Base.Core.Extensions;
using Server.Base.Core.Services;
using System.Text;
using Web.AssetBundles.Abstractions;
using Web.AssetBundles.Events;
using Web.AssetBundles.Extensions;
using Web.AssetBundles.Helpers;

namespace Web.AssetBundles.Services;

public class BuildXmlFiles : IService, IInjectModules
{
    private readonly AssetEventSink _eventSink;
    private readonly IServiceProvider _services;
    private readonly ILogger<BuildXmlFiles> _logger;
    private readonly ServerHandler _handler;

    public IEnumerable<Module> Modules { get; set; }

    public BuildXmlFiles(AssetEventSink eventSink, IServiceProvider services, ILogger<BuildXmlFiles> logger,
        ServerHandler handler)
    {
        _eventSink = eventSink;
        _services = services;
        _logger = logger;
        _handler = handler;
    }

    public void Initialize() => _eventSink.AssetBundlesLoaded += LoadXmlFiles;

    private void LoadXmlFiles(AssetBundleLoadEventArgs assetLoadEvent)
    {
        _logger.LogInformation("Reading XML Files From Bundles");

        var assets = assetLoadEvent.InternalAssets
            .Select(x => x.Value)
            .Where(x => x.Type == AssetInfo.TypeAsset.XML);

        foreach (var xmlBundle in _services.GetRequiredServices<IBundledXml>(Modules))
        {
            var asset = assets.FirstOrDefault(x =>
                string.Equals(x.Name, xmlBundle.BundleName, StringComparison.OrdinalIgnoreCase));

            if (asset == null)
            {
                _logger.LogCritical("Could not find XML bundle for {BundleName}, returning...", xmlBundle.BundleName);
                _logger.LogCritical("Possible XML files:");

                foreach (var foundAsset in assets)
                    _logger.LogError("    {BundleName}", foundAsset.Name);

                _handler.KillServer(false);
                return;
            }

            try
            {
                var manager = new AssetsManager();
                manager.LoadFiles(asset.Path);

                var textAsset = manager.assetsFileList.First().ObjectsDic.Values.GetText(asset.Name);

                var text = Encoding.UTF8.GetString(textAsset.m_Script);
                var length = text.Split('\n').Length;

                xmlBundle.LoadBundle(text);
                _logger.LogTrace("Read XML {XMLName} for {Lines} lines.", asset.Name, length);
            }
            catch
            {
                _logger.LogWarning("{Name} could not load! Skipping...", GetType().Name);
            }
        }

        _logger.LogDebug("Read XML files");
    }
}
