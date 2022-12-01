using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace Server.Base.Core.Abstractions;

public abstract class Module
{
    public readonly ILogger Logger;

    public abstract string[] Contributors { get; }

    protected Module(ILogger logger) => Logger = logger;

    public virtual string GetModuleInformation() => GetType().Namespace;

    public virtual void AddLogging(ILoggingBuilder loggingBuilder)
    {
    }

    public virtual void AddServices(IServiceCollection services, IEnumerable<Module> modules)
    {
    }

    public virtual void ConfigureServices(ConfigurationManager configuration, IServiceCollection services)
    {
    }

    public virtual void PostBuild(IServiceProvider services, IEnumerable<Module> modules)
    {
    }
}
