using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Server.Base.Core.Extensions;
using Web.AssetBundles.Extensions;
using Web.AssetBundles.Models;
using Web.AssetBundles.Services;

namespace Web.AssetBundles.Controllers.Client;

[Route("/Client/{folder}/{name}")]
public class AssetHostController : Controller
{
    private readonly ILogger<AssetHostController> _logger;

    private readonly BuildAssetList _bundles;
    private readonly AssetBundleConfig _config;

    public AssetHostController(BuildAssetList bundles, ILogger<AssetHostController> logger,
        AssetBundleConfig config)
    {
        _bundles = bundles;
        _logger = logger;
        _config = config;
    }

    [HttpGet]
    public IActionResult GetAsset([FromRoute] string folder, [FromRoute] string name)
    {
        var publishConfig = _config.PublishConfigs.FirstOrDefault(a => string.Equals(a.Value, name));

        if (!publishConfig.IsDefault())
        {
            _logger.LogDebug("Getting Publish Configuration {Type} ({Folder})", publishConfig.Key, folder);
            return Ok(_bundles.PublishConfigs[publishConfig.Key]);
        }

        var assetDict = _config.AssetDictConfigs.FirstOrDefault(a => string.Equals(a.Value, name));

        if (!assetDict.IsDefault())
        {
            _logger.LogDebug("Getting Asset Dictionary {Type} ({Folder})", assetDict.Key, folder);
            return Ok(_bundles.AssetDict[assetDict.Key]);
        }

        name = name.Split('.')[0];

        if (!_bundles.InternalAssets.ContainsKey(name))
            return NotFound();

        var asset = _bundles.InternalAssets[name];
        _logger.LogDebug("Getting asset {Name} from {File} ({Folder})", asset.Name, Path.GetFileName(asset.Path),
            folder);

        return new FileContentResult(asset.ApplyFixes(), "application/octet-stream");
    }
}
