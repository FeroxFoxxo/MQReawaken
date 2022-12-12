namespace Server.Base.Core.Abstractions;

public interface IInjectModules
{
    public IEnumerable<Module> Modules { get; set; }
}
