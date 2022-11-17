using Microsoft.AspNetCore.Builder;
using Server.Base.Core.Abstractions;
using Server.Base.Logging;

namespace Server.Web.Abstractions;

public abstract class WebModule : Module
{
    protected WebModule(Logger logger) : base(logger)
    {
    }

    public virtual void InitializeWeb(WebApplicationBuilder builder)
    {
    }

    public virtual void PostWebBuild(WebApplication app)
    {
    }

    public override string GetModuleInformation() => $"{base.GetModuleInformation()} (WEB)";
}
