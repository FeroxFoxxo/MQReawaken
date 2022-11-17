using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace Server.Base.Core.Abstractions;

public abstract class Module
{
    public abstract int Major { get; }

    public abstract int Minor { get; }

    public abstract int Patch { get; }

    public abstract string[] Contributors { get; }

    public virtual string GetModuleInformation() =>
        $"{GetType().Namespace} v{Major}.{Minor}.{Patch}";

    public virtual void AddLogging(ILoggingBuilder loggingBuilder)
    {
    }

    public virtual void AddServices(IServiceCollection services)
    {
    }

    public virtual void ConfigureServices(ConfigurationManager configuration, IServiceCollection services)
    {
    }

    public virtual void PostBuild(IServiceProvider services)
    {
    }
}
