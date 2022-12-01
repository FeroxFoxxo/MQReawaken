using Microsoft.Extensions.Logging;
using Server.Base.Core.Abstractions;

namespace Protocols.External;

public class XtProtocol : Module
{
    public override string[] Contributors { get; } = { "Ferox" };

    public XtProtocol(ILogger<XtProtocol> logger) : base(logger)
    {
    }
}
