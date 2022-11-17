using Server.Base.Core.Abstractions;
using Server.Base.Logging;
using SmartFoxClientAPI;

namespace Server.Reawakened;

public class Reawakened : Module
{
    public override int Major => 1;
    public override int Minor => 0;
    public override int Patch => 1;

    public override string[] Contributors { get; } = { "Ferox" };

    public Reawakened(Logger logger) : base(logger)
    {
    }

    public override string GetModuleInformation() =>
        $"{base.GetModuleInformation()} for API {new SmartFoxClient().GetVersion()}";
}
