using Microsoft.Extensions.DependencyInjection;
using Server.Base.Core.Abstractions;
using System.Text.Json;

namespace Server.Base.Core.Extensions;

public static class GetConfigs
{
    private const string ConfigDir = "Configs";

    public static bool LoadConfigsWasFound(this IServiceCollection services, Type config)
    {
        var configName = config.Name;
        try
        {
            using var stream = GetFile.GetFileStream($"{configName}.json", ConfigDir, FileMode.Open);
            services.AddSingleton(config,
                JsonSerializer.Deserialize(stream, config) ?? throw new InvalidCastException());
            return true;
        }
        catch (FileNotFoundException)
        {
            services.AddSingleton(config);
            return false;
        }
    }

    public static void SaveConfigs(this IServiceProvider services, IEnumerable<Module> modules)
    {
        foreach (var config in services.GetRequiredServices<IConfig>(modules))
        {
            using var stream = GetFile.GetFileStream($"{config.GetType().Name}.json", ConfigDir, FileMode.Create);
            JsonSerializer.Serialize(stream, config, config.GetType(),
                new JsonSerializerOptions { WriteIndented = true });
        }
    }
}
