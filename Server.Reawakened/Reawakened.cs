using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Server.Base.Core.Abstractions;
using Server.Reawakened.Network.Helpers;
using Server.Reawakened.Players.Helpers;
using SmartFoxClientAPI;

namespace Server.Reawakened;

public class Reawakened : Module
{
    public override string[] Contributors { get; } = { "Ferox" };

    public Reawakened(ILogger<Reawakened> logger) : base(logger)
    {
    }

    public override void AddServices(IServiceCollection services, Module[] modules) =>
        services
            .AddSingleton<ReflectionUtils>()
            .AddSingleton<SmartFoxClient>()
            .AddSingleton<NameGenSyllables>();

    public override string GetModuleInformation() =>
        $"{base.GetModuleInformation()} for API {new SmartFoxClient().GetVersion()}";
}
