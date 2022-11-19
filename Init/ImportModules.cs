using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Protocols.External;
using Protocols.System;
using Server.Base;
using Server.Base.Core.Abstractions;
using Server.Base.Core.Extensions;
using Server.Base.Logging;
using Server.Reawakened;
using Server.Web;
using System.Collections.Generic;

namespace Init;

public static class ImportModules
{
    public static void AddModules(this IServiceCollection services) =>
        services
            .AddSingleton<Web>()
            .AddSingleton<Reawakened>()
            .AddSingleton<ServerBase>()
            .AddSingleton<SysProtocol>()
            .AddSingleton<XtProtocol>();

    public static IEnumerable<Module> GetModules()
    {
        var services = new ServiceCollection();
        services.AddModules();
        services.AddLogging(l => l.AddProvider(new LoggerProvider()));
        var provider = services.BuildServiceProvider();
        return provider.GetRequiredServices<Module>();
    }
}
