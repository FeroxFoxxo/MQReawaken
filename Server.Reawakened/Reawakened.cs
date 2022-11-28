using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Server.Base.Core.Abstractions;
using Server.Reawakened.Network.Helpers;
using SmartFoxClientAPI;
using Thrift.Protocol;

namespace Server.Reawakened;

public class Reawakened : Module
{
    public override int Major => 1;
    public override int Minor => 0;
    public override int Patch => 1;

    public override string[] Contributors { get; } = { "Ferox" };

    public Reawakened(ILogger<Reawakened> logger) : base(logger) => new TBinaryProtocol(null);

    public override void AddServices(IServiceCollection services, IEnumerable<Module> modules) =>
        services
            .AddSingleton<ReflectionUtils>()
            .AddSingleton<SmartFoxClient>();

    public override string GetModuleInformation() =>
        $"{base.GetModuleInformation()} for API {new SmartFoxClient().GetVersion()}";
}
