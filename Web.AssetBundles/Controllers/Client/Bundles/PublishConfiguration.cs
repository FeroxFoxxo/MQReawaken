using Microsoft.AspNetCore.Mvc;
using Web.AssetBundles.Helpers;

namespace Web.AssetBundles.Controllers.Client.Bundles;

[Route("Client/Bundles/PublishConfiguration.xml")]
public class PublishConfiguration : Controller
{
    private readonly BuildAssetBundles _bundles;

    public PublishConfiguration(BuildAssetBundles bundles) => _bundles = bundles;

    [HttpGet]
    public IActionResult GetPublishConfiguration() => Ok(_bundles.PublishConfiguration);
}
