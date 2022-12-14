using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Server.Base.Core.Extensions;
using Web.AssetBundles.Models;
using Web.AssetBundles.Services;

namespace Web.AssetBundles.Controllers.Client;

[Route("/Client/{folder}/{name}")]
public class AssetHostController : Controller
{
    private readonly ILogger<AssetHostController> _logger;

    private readonly BuildPubConfig _bundles;
    private readonly AssetBundleConfig _config;

    public AssetHostController(BuildPubConfig bundles, ILogger<AssetHostController> logger,
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
            _logger.LogDebug("Getting Publish Configuration {Type}", publishConfig.Key);
            return Ok(_bundles.PublishConfigs[publishConfig.Key]);
        }

        var assetDict = _config.AssetDictConfigs.FirstOrDefault(a => string.Equals(a.Value, name));

        if (!assetDict.IsDefault())
        {
            _logger.LogDebug("Getting Asset Dictionary {Type}", assetDict.Key);
            return Ok(_bundles.AssetDict[assetDict.Key]);
        }

        _logger.LogDebug("Getting asset {folder}/{name}", folder, name);

        _logger.LogError("Could not find asset {folder}/{name}", folder, name);

        return NotFound();
    }
}
