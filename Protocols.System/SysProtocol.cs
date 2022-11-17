using Server.Base.Core.Abstractions;
using Server.Base.Logging;

namespace Protocols.System;

public class SysProtocol : Module
{
    public override int Major => 1;
    public override int Minor => 0;
    public override int Patch => 1;

    public override string[] Contributors { get; } = { "Ferox" };

    public SysProtocol(Logger logger) : base(logger)
    {
    }
}
