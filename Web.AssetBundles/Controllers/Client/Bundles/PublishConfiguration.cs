using Microsoft.AspNetCore.Mvc;
using Web.AssetBundles.Services;

namespace Web.AssetBundles.Controllers.Client.Bundles;

[Route("Client/Bundles/PublishConfiguration.xml")]
public class PublishConfiguration : Controller
{
    private readonly BuildPubConfig _bundles;

    public PublishConfiguration(BuildPubConfig bundles) => _bundles = bundles;

    [HttpGet]
    public IActionResult GetPublishConfiguration() => Ok(_bundles.PublishConfiguration);
}
