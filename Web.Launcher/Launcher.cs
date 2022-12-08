using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Server.Base.Core.Abstractions;
using Server.Web.Abstractions;
using Web.Launcher.Helpers;

namespace Web.Launcher;

public class Launcher : WebModule
{
    public override string[] Contributors { get; } = { "Ferox" };

    public Launcher(ILogger<Launcher> logger) : base(logger)
    {
    }

    public override void AddServices(IServiceCollection services, IEnumerable<Module> modules) =>
        services.AddSingleton<LauncherSink>();
}
