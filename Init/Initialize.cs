using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using Server.Base.Core.Abstractions;
using Server.Base.Core.Workers;
using Server.Base.Logging;
using Server.Web.Abstractions;

namespace Init;

public class Initialize
{
    public static async Task Main()
    {
        var logger = new Logger("Initialization");
        logger.ShouldDebugWithName(false);

        try
        {
            logger.LogInformation("============ Launching =============");

            var builder = WebApplication.CreateBuilder();

            builder.Services.AddHostedService<ServerWorker>();

            logger.LogInformation("Getting Modules");
            var modules = GetModules(logger);

            logger.LogInformation("Importing Modules");
            InitializeModules(modules, builder, logger);

            logger.LogInformation("Building Application");
            var app = builder.Build();
            logger.LogDebug("Application built");

            logger.LogInformation("Configuring Application");
            ConfigureApp(modules, app, logger);

            logger.LogInformation("======== Running Application =======");

            logger.ShouldDebugWithName(true);
            await app.RunAsync();
        }

        catch (Exception ex)
        {
            logger.LogError(ex, "Could not start application!");
        }
    }

    private static List<Module> GetModules(Logger logger)
    {
        var modules = ImportModules.GetModules(logger);

        foreach (var module in modules)
        {
            logger.LogDebug("Imported {ModuleInfo}", module.GetModuleInformation());

            if (module.Contributors.Length <= 0)
                continue;

            logger.LogTrace("    Contributed By:      ");
            logger.LogTrace("        {Contributors}", string.Join(", ", module.Contributors));
        }

        logger.LogDebug("Fetched {ModuleCount} modules", modules.Count);

        return modules;
    }

    private static void InitializeModules(List<Module> modules, WebApplicationBuilder builder, ILogger logger)
    {
        foreach (var startup in modules)
            startup.AddLogging(builder.Logging);

        logger.LogDebug("Successfully initialized logging");

        foreach (var startup in modules)
            startup.AddServices(builder.Services);

        logger.LogDebug("Successfully initialized services");

        foreach (var startup in modules)
            startup.ConfigureServices(builder.Configuration, builder.Services);

        logger.LogDebug("Successfully configured services");

        var controller = builder.Services.AddControllers()
            .AddNewtonsoftJson(x => { x.SerializerSettings.ReferenceLoopHandling = ReferenceLoopHandling.Ignore; });

        foreach (var module in modules)
        {
            if (module is WebModule m)
                m.InitializeWeb(builder);

            controller.AddApplicationPart(module.GetType().Assembly);
        }

        logger.LogDebug("Successfully initialized web services");
    }

    private static void ConfigureApp(List<Module> modules, WebApplication app, ILogger logger)
    {
        foreach (var startup in modules)
        {
            startup.PostBuild(app.Services);

            if (startup is WebModule module)
                module.PostWebBuild(app);
        }

        logger.LogDebug("Successfully post built application");
    }
}
