using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Server.Base.Accounts.Helpers;
using Server.Base.Core.Abstractions;
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

    public ServerBase(Logger logger) : base(logger)
    {
    }

    public override void AddLogging(ILoggingBuilder loggingBuilder)
    {
        loggingBuilder.ClearProviders();
        loggingBuilder.AddProvider(new LoggerProvider());
    }

    public override void AddServices(IServiceCollection services)
    {
        foreach (var service in AppDomain.CurrentDomain.GetAssemblies()
                     .SelectMany(a =>
                         a.GetTypes().Where(
                             t => typeof(IService).IsAssignableFrom(t) &&
                                  !t.IsInterface &&
                                  !t.IsAbstract
                         )
                     )
                )
        {
            Logger.LogTrace("Loaded: {ServiceName}", service.Name);
            services.AddSingleton(service);
        }

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
        foreach (var service in services.GetServices<IService>())
            service.Initialize();
    }
}
