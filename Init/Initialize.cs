using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Server.Base.Core.Abstractions;
using Server.Base.Logging;
using Server.Web.Abstractions;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace Init;

public class Initialize
{
    public static async Task Main()
    {
        var logger = new Logger("Initialization");

        try
        {
            logger.LogInformation("============ Launching =============");

            var builder = WebApplication.CreateBuilder();

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

            await app.RunAsync();
        }

        catch (Exception ex)
        {
            logger.LogError(ex, "Could not start application!");
        }
    }

    private static Module[] GetModules(ILogger logger)
    {
        var modules = ImportModules.GetModules();

        foreach (var module in modules)
        {
            logger.LogDebug("Imported {ModuleInfo}", module.GetModuleInformation());

            if (module.Contributors.Length <= 0)
                continue;

            logger.LogTrace("    Contributed By:      ");
            logger.LogTrace("        {Contributors}", string.Join(", ", module.Contributors));
        }

        logger.LogDebug("Fetched {ModuleCount} modules", modules.Count());

        return modules;
    }

    private static void InitializeModules(Module[] modules, WebApplicationBuilder builder, ILogger logger)
    {
        logger.LogInformation("Initializing Logging");
        foreach (var startup in modules)
            startup.AddLogging(builder.Logging);
        logger.LogDebug("Successfully initialized logging");

        logger.LogInformation("Initializing Services");
        foreach (var startup in modules)
            startup.AddServices(builder.Services, modules);
        logger.LogDebug("Successfully initialized services");

        logger.LogInformation("Configuring Services");
        foreach (var startup in modules)
            startup.ConfigureServices(builder.Configuration, builder.Services);
        logger.LogDebug("Successfully configured services");

        logger.LogInformation("Initializing Web Services");

        var controller = builder.Services.AddControllers();

        foreach (var module in modules)
        {
            if (module is WebModule m)
                m.InitializeWeb(builder);

            controller.AddApplicationPart(module.GetType().Assembly);
        }

        logger.LogDebug("Successfully initialized web services");
    }

    private static void ConfigureApp(Module[] modules, WebApplication app, ILogger logger)
    {
        foreach (var startup in modules)
        {
            startup.PostBuild(app.Services, modules);

            if (startup is WebModule module)
                module.PostWebBuild(app);
        }

        logger.LogDebug("Successfully post built application");
    }
}
