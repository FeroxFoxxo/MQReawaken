﻿using Microsoft.Extensions.DependencyInjection;
using Server.Base.Core.Abstractions;
using Server.Base.Logging;
using Server.Reawakened.Data.Modals;
using Server.Reawakened.Network.Helpers;
using SmartFoxClientAPI;

namespace Server.Reawakened;

public class Reawakened : Module
{
    public override int Major => 1;
    public override int Minor => 0;
    public override int Patch => 1;

    public override string[] Contributors { get; } = { "Ferox" };

    public Reawakened(Logger logger) : base(logger)
    {
    }

    public override void AddServices(IServiceCollection services) =>
        services
            .AddSingleton<ReflectionUtils>()
            .AddSingleton<ServerConfig>();

    public override string GetModuleInformation() =>
        $"{base.GetModuleInformation()} for API {new SmartFoxClient().GetVersion()}";
}
