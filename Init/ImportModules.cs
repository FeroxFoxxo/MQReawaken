using System.Collections.Generic;
using Protocols.External;
using Protocols.System;
using Server.Base;
using Server.Base.Core.Abstractions;
using Server.Reawakened;
using Server.Web;

namespace Init;

public class ImportModules
{
    public static List<Module> GetModules() =>
        new()
        {
            new XTProtocol(),
            new SysProtocol(),
            new ServerBase(),
            new Reawakened(),
            new Web()
        };
}
