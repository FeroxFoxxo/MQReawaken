using AspNetCoreRateLimit;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.DataProtection;
using Microsoft.AspNetCore.DataProtection.AuthenticatedEncryption;
using Microsoft.AspNetCore.DataProtection.AuthenticatedEncryption.ConfigurationModel;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Server.Base.Accounts.Helpers;
using Server.Base.Core.Abstractions;
using Server.Base.Core.Extensions;
using Server.Base.Core.Helpers;
using Server.Base.Core.Services;
using Server.Base.Core.Workers;
using Server.Base.Logging;
using Server.Base.Middleware;
using Server.Base.Network.Helpers;
using Server.Base.Timers.Helpers;
using Server.Base.Timers.Services;
using Server.Base.Worlds;

namespace Server.Base;

public class Server : WebModule
{
    public override int Major => 1;
    public override int Minor => 1;
    public override int Patch => 1;

    public override string[] Contributors { get; } = { "Ferox" };

    public Server(ILogger<Server> logger) : base(logger)
    {
    }

    public override void AddLogging(ILoggingBuilder loggingBuilder)
    {
        loggingBuilder.ClearProviders();
        loggingBuilder.AddProvider(new LoggerProvider());
    }

    public override void AddServices(IServiceCollection services, IEnumerable<Module> modules)
    {
        Logger.LogDebug("Loading Services");
        foreach (var service in RequiredServices.GetServices<IService>(modules))
        {
            Logger.LogTrace("   Loaded: {ServiceName}", service.Name);
            services.AddSingleton(service);
        }

        Logger.LogDebug("Loaded services");

        Logger.LogDebug("Loading Modules");
        foreach (var service in RequiredServices.GetServices<Module>(modules))
        {
            Logger.LogTrace("   Loaded: {ServiceName}", service.Name);
            services.AddSingleton(service);
        }

        Logger.LogDebug("Loaded modules");

        Logger.LogDebug("Loading Configs");

        foreach (var service in RequiredServices.GetServices<IConfig>(modules))
        {
            if (services.LoadConfigsWasFound(service))
                Logger.LogTrace("   Config: Found {Name}", service.Name);
            else
                Logger.LogTrace("   Config: {Name} was not found, creating!", service.Name);
        }

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

        services.AddSingleton<IIpPolicyStore, MemoryCacheIpPolicyStore>();
        services.AddSingleton<IRateLimitCounterStore, MemoryCacheRateLimitCounterStore>();
        services.AddSingleton<IHttpContextAccessor, HttpContextAccessor>();
        services.AddSingleton<IRateLimitConfiguration, RateLimitConfiguration>();
        services.AddSingleton<IProcessingStrategy, AsyncKeyLockProcessingStrategy>();

        services.AddEndpointsApiExplorer();
        services.AddSwaggerGen();
    }

    public override void PostBuild(IServiceProvider services, IEnumerable<Module> modules)
    {
        foreach (var service in services.GetRequiredServices<IService>(modules))
            service.Initialize();

        services.GetRequiredService<ServerWorker>().SetModules(modules);
    }

    public override void ConfigureServices(ConfigurationManager configuration, IServiceCollection services)
    {
        services.Configure<IpRateLimitOptions>(configuration.GetSection("IpRateLimiting"));
        services.Configure<IpRateLimitPolicies>(configuration.GetSection("IpRateLimitPolicies"));
    }

    public override void InitializeWeb(WebApplicationBuilder builder)
    {
        builder.WebHost.CaptureStartupErrors(true);

        builder.WebHost.UseUrls("http://*:80");

        builder.Services.AddMemoryCache();

        builder.Services.AddDataProtection().UseCryptographicAlgorithms(
            new AuthenticatedEncryptorConfiguration
            {
                EncryptionAlgorithm = EncryptionAlgorithm.AES_256_CBC,
                ValidationAlgorithm = ValidationAlgorithm.HMACSHA256
            }
        );
    }

    public override void PostWebBuild(WebApplication app)
    {
        if (app.Environment.IsDevelopment())
        {
            app.UseDeveloperExceptionPage();

            app.UseSwagger();
            app.UseSwaggerUI(options =>
            {
                options.SwaggerEndpoint("/swagger/v1/swagger.json", "v1");
                options.RoutePrefix = string.Empty;
            });
        }

        app.UseIpRateLimiting();

        app.UseMiddleware<RequestLoggingMiddleware>();

        app.UseRouting();

        app.UseAuthentication();
        app.UseAuthorization();

        app.UseEndpoints(endpoints => endpoints.MapControllers());
    }
}
