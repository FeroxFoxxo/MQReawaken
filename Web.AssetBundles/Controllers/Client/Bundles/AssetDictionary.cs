using Microsoft.AspNetCore.Mvc;
using Web.AssetBundles.Services;

namespace Web.AssetBundles.Controllers.Client.Bundles;

[Route("Client/Bundles/assetDictionary.xml")]
public class AssetDictionary : Controller
{
    private readonly BuildPubConfig _bundles;

    public AssetDictionary(BuildPubConfig bundles) => _bundles = bundles;

    [HttpGet]
    public IActionResult GetAssetDictionary() => Ok(_bundles.AssetDictionary);
}
