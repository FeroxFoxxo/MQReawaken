using System.Collections.Generic;
using Protocols.External;
using Protocols.System;
using Server.Base;
using Server.Base.Core.Abstractions;
using Server.Base.Logging;
using Server.Reawakened;
using Server.Web;

namespace Init;

public class ImportModules
{
    public static List<Module> GetModules(Logger logger) =>
        new()
        {
            new XTProtocol(logger),
            new SysProtocol(logger),
            new ServerBase(logger),
            new Reawakened(logger),
            new Web(logger)
        };
}
