using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Server.Base.Core.Abstractions;
using System.Text.Json;

namespace Server.Base.Core.Extensions;

public static class GetConfigs
{
    private const string ConfigDir = "Configs";

    public static void LoadConfigs(this IServiceCollection services, Type config, ILogger logger)
    {
        var configName = config.Name;
        try
        {
            using var stream = GetFile.GetFileStream($"{configName}.json", ConfigDir, FileMode.Open);
            services.AddSingleton(config,
                JsonSerializer.Deserialize(stream, config) ?? throw new InvalidCastException());
            logger.LogTrace("   Config: Found {Name} in {Directory}", configName, Path.GetDirectoryName(stream.Name));
        }
        catch (FileNotFoundException)
        {
            services.AddSingleton(config);
            logger.LogTrace("   Config: {Name} was not found, creating!", configName);
        }
    }

    public static void SaveConfigs(this IServiceProvider services, IEnumerable<Module> modules)
    {
        foreach (var config in services.GetRequiredServices<IConfig>(modules))
        {
            var configName = config.GetType().Name;
            using var stream = GetFile.GetFileStream($"{config.GetType().Name}.json", ConfigDir, FileMode.Create);
            JsonSerializer.Serialize(stream, config, config.GetType(),
                new JsonSerializerOptions { WriteIndented = true });
        }
    }
}
