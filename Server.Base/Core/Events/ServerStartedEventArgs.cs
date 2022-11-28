using Server.Base.Core.Abstractions;

namespace Server.Base.Core.Events;

public class ServerStartedEventArgs : EventArgs
{
    public IEnumerable<Module> Modules { get; }

    public ServerStartedEventArgs(IEnumerable<Module> modules) => Modules = modules;
}
