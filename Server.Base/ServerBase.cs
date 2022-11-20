using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Server.Base.Accounts.Helpers;
using Server.Base.Core.Abstractions;
using Server.Base.Core.Extensions;
using Server.Base.Core.Helpers;
using Server.Base.Core.Models;
using Server.Base.Core.Services;
using Server.Base.Logging;
using Server.Base.Network.Helpers;
using Server.Base.Timers.Helpers;
using Server.Base.Timers.Services;
using Server.Base.Worlds;

namespace Server.Base;

public class ServerBase : Module
{
    public override int Major => 1;
    public override int Minor => 0;
    public override int Patch => 1;

    public override string[] Contributors { get; } = { "Ferox" };

    public ServerBase(ILogger<ServerBase> logger) : base(logger)
    {
    }

    public override void AddLogging(ILoggingBuilder loggingBuilder)
    {
        loggingBuilder.ClearProviders();
        loggingBuilder.AddProvider(new LoggerProvider());
    }

    public override void AddServices(IServiceCollection services)
    {
        Logger.LogDebug("Loading Services");
        foreach (var service in RequiredServices.GetServices<IService>())
        {
            Logger.LogTrace("Loaded: {ServiceName}", service.Name);
            services.AddSingleton(service);
        }

        Logger.LogDebug("Loaded Services");

        Logger.LogDebug("Loading Modules");
        foreach (var service in RequiredServices.GetServices<Module>())
        {
            Logger.LogTrace("Loaded: {ServiceName}", service.Name);
            services.AddSingleton(service);
        }

        Logger.LogDebug("Loaded Modules");

        services
            .AddSingleton<Random>()
            .AddSingleton<InternalServerConfig>()
            .AddSingleton<ServerHandler>()
            .AddSingleton<EventSink>()
            .AddSingleton<World>()
            .AddSingleton<TimerThread>()
            .AddSingleton<TimerChangePool>()
            .AddSingleton<AccountAttackLimiter>()
            .AddSingleton<PasswordHasher>()
            .AddSingleton<NetworkLogger>()
            .AddSingleton<IpLimiter>();
    }

    public override void PostBuild(IServiceProvider services)
    {
        foreach (var service in services.GetRequiredServices<IService>())
            service.Initialize();
    }
}
