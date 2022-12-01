using Microsoft.Extensions.Logging;
using Server.Base.Core.Abstractions;

namespace Protocols.System;

public class SysProtocol : Module
{
    public override string[] Contributors { get; } = { "Ferox" };

    public SysProtocol(ILogger<SysProtocol> logger) : base(logger)
    {
    }
}
