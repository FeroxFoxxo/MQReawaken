using Server.Base.Core.Abstractions;

namespace Protocols.System;

public class SystemVersion : Module
{
    public override int Major => 1;
    public override int Minor => 0;
    public override int Patch => 1;
}
