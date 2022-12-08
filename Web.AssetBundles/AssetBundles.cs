using Microsoft.Extensions.Logging;
using Server.Web.Abstractions;

namespace Web.AssetBundles;

public class AssetBundles : WebModule
{
    public override string[] Contributors { get; } = { "Ferox", "Prefare" };

    public AssetBundles(ILogger<AssetBundles> logger) : base(logger)
    {
    }
}
