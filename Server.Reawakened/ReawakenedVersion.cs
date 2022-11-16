using Server.Base.Core.Abstractions;
using SmartFoxClientAPI;

namespace Server.Reawakened;

public class ReawakenedVersion : Module
{
    public override int Major => 1;
    public override int Minor => 0;
    public override int Patch => 1;

    public override string GetModuleInformation() =>
        $"{base.GetModuleInformation()} for API {new SmartFoxClient().GetVersion()}";
}
