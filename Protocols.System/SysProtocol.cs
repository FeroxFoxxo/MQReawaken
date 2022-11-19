using Server.Base.Core.Abstractions;

namespace Protocols.System;

public class SysProtocol : Module
{
    public override int Major => 1;
    public override int Minor => 0;
    public override int Patch => 1;

    public override string[] Contributors { get; } = { "Ferox" };

    public SysProtocol(Microsoft.Extensions.Logging.ILogger<SysProtocol> logger) : base(logger)
    {
    }
}
