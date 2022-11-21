using Microsoft.Extensions.Logging;
using Server.Base.Core.Abstractions;

namespace Server.Launcher;

public class Launcher : Module
{
    public override int Major => 1;
    public override int Minor => 0;
    public override int Patch => 1;

    public override string[] Contributors { get; } = { "Ferox" };

    public Launcher(ILogger<Launcher> logger) : base(logger)
    {
    }
}
