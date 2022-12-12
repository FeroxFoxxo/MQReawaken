using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Server.Base.Core.Abstractions;
using Server.Base.Core.Extensions;
using Server.Web.Abstractions;
using System.Runtime.Serialization;
using Web.AssetBundles.Abstractions;
using Web.AssetBundles.Helpers;

namespace Web.AssetBundles;

public class AssetBundles : WebModule
{
    public override string[] Contributors { get; } = { "Ferox", "Prefare" };

    public AssetBundles(ILogger<AssetBundles> logger) : base(logger)
    {
    }

    public override void AddServices(IServiceCollection services, IEnumerable<Module> modules)
    {
        services.AddSingleton<BuildAssetBundles>();

        Logger.LogInformation("Loading Bundles");

        foreach (var xml in RequiredServices.GetServices<IBundledXml>(modules))
        {
            Logger.LogTrace("   Loaded: {ServiceName}", xml.Name);
            services.AddSingleton(xml, FormatterServices.GetUninitializedObject(xml));
        }

        Logger.LogDebug("Loaded bundles");
    }
}
