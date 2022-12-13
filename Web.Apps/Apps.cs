using Microsoft.Extensions.Logging;
using Server.Web.Abstractions;

namespace Web.Apps;

public class Apps : WebModule
{
    public Apps(ILogger<Apps> logger) : base(logger)
    {
    }

    public override string[] Contributors { get; } = { "Ferox" };
}
