using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Server.Base.Logging;

namespace Server.Base.Core.Abstractions;

public abstract class Module
{
    public readonly Logger Logger;
    public abstract int Major { get; }

    public abstract int Minor { get; }

    public abstract int Patch { get; }

    public abstract string[] Contributors { get; }

    protected Module(Logger logger) => Logger = logger;

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
