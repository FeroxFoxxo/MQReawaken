﻿using Server.Base.Core.Abstractions;

namespace Protocols.External;

public class ExternalVersion : Module
{
    public override int Major => 1;
    public override int Minor => 0;
    public override int Patch => 1;
}