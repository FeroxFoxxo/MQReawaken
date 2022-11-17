using Microsoft.AspNetCore.Builder;
using Server.Base.Core.Abstractions;

namespace Server.Web.Abstractions;

public abstract class WebModule : Module
{
    public virtual void InitializeWeb(WebApplicationBuilder builder)
    {
    }

    public virtual void PostWebBuild(WebApplication app)
    {
    }

    public override string GetModuleInformation() => $"{base.GetModuleInformation()} (WEB)";
}
