using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Server.Base.Accounts.Helpers;
using Server.Base.Core.Abstractions;
using Server.Base.Core.Extensions;
using Server.Base.Core.Helpers;
using Server.Base.Core.Services;
using Server.Base.Core.Workers;
using Server.Base.Logging;
using Server.Base.Network.Helpers;
using Server.Base.Timers.Helpers;
using Server.Base.Timers.Services;
using Server.Base.Worlds;

namespace Server.Base;

public class Server : Module
{
    public override string[] Contributors { get; } = { "Ferox", "ServUO" };

    public Server(ILogger<Server> logger) : base(logger)
    {
    }

    public override void AddLogging(ILoggingBuilder loggingBuilder)
    {
        loggingBuilder.ClearProviders();
        loggingBuilder.AddProvider(new LoggerProvider());
    }

    public override void AddServices(IServiceCollection services, Module[] modules)
    {
        Logger.LogInformation("Loading Hosted Services");

        {
            services.AddHostedService<ServerWorker>();
            Logger.LogTrace("   Loaded: Service Worker");
        }

        Logger.LogDebug("Loaded hosted services");

        Logger.LogInformation("Loading Services");

        foreach (var service in RequiredServices.GetServices<IService>(modules))
        {
            Logger.LogTrace("   Loaded: {ServiceName}", service.Name);
            services.AddSingleton(service);
        }

        Logger.LogDebug("Loaded services");

        Logger.LogInformation("Loading Modules");
        foreach (var service in RequiredServices.GetServices<Module>(modules))
        {
            Logger.LogTrace("   Loaded: {ServiceName}", service.Name);
            services.AddSingleton(service);
        }

        Logger.LogDebug("Loaded modules");

        Logger.LogInformation("Loading Configs");

        foreach (var service in RequiredServices.GetServices<IConfig>(modules))
            services.LoadConfigs(service, Logger);

        Logger.LogDebug("Loaded configs");

        services
            .AddSingleton<Random>()
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

    public override void PostBuild(IServiceProvider services, Module[] modules)
    {
        foreach (var service in services.GetRequiredServices<IInjectModules>(modules))
            service.Modules = modules;

        foreach (var service in services.GetRequiredServices<IService>(modules))
            service.Initialize();

        services.GetRequiredService<ServerHandler>().SetModules(modules);
    }
}
