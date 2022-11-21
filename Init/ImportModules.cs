using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Protocols.External;
using Protocols.System;
using Server.Base.Core.Abstractions;
using Server.Base.Core.Extensions;
using Server.Base.Logging;
using Server.Launcher;
using Server.Reawakened;
using System.Collections.Generic;

namespace Init;

public static class ImportModules
{
    public static void AddModules(this IServiceCollection services) =>
        services
            .AddSingleton<Reawakened>()
            .AddSingleton<Launcher>()
            .AddSingleton<Server.Base.Server>()
            .AddSingleton<SysProtocol>()
            .AddSingleton<XtProtocol>();

    public static IEnumerable<Module> GetModules()
    {
        var services = new ServiceCollection();
        services.AddLogging(l =>
        {
            l.AddProvider(new LoggerProvider());
            l.SetMinimumLevel(LogLevel.Trace);
        });
        services.AddModules();
        var provider = services.BuildServiceProvider();
        return provider.GetRequiredServices<Module>();
    }
}
