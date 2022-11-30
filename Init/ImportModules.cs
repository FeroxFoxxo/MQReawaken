using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Protocols.External;
using Protocols.System;
using Server.Base.Core.Abstractions;
using Server.Base.Logging;
using Server.Reawakened;
using Server.Web;
using System.Collections.Generic;
using System.Linq;

namespace Init;

public static class ImportModules
{
    public static IEnumerable<Module> GetModules()
    {
        var modules = new[]
        {
            typeof(Reawakened),
            typeof(Web),
            typeof(Server.Base.Server),
            typeof(SysProtocol),
            typeof(XtProtocol)
        };

        var services = new ServiceCollection();
        services.AddLogging(l =>
        {
            l.AddProvider(new LoggerProvider());
            l.SetMinimumLevel(LogLevel.Trace);
        });

        foreach (var type in modules)
            services.AddSingleton(type);

        var provider = services.BuildServiceProvider();

        return modules.Select(module => provider.GetRequiredService(module) as Module).ToList();
    }
}
