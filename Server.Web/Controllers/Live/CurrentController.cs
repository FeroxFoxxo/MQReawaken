using Microsoft.AspNetCore.Mvc;
using Server.Reawakened.Launcher.Enums;
using System.Dynamic;

namespace Server.Web.Controllers.Live;

[Route("live/current.txt")]
public class CurrentController : Controller
{
    [HttpGet]
    public IActionResult GetCurrentData([FromQuery] string? srcVersion, [FromQuery] string? cachebust)
    {
        srcVersion ??= string.Empty;
        cachebust ??= string.Empty;

        var reqType = srcVersion.Split('-')[0];

        var type = reqType switch
        {
            "launcherPatcher" => PatcherType.Launcher,
            "gamePatcher" => PatcherType.Game,
            _ => PatcherType.Unknown
        };

        dynamic pkg = new ExpandoObject();

        dynamic current = new ExpandoObject();
        current.version = srcVersion;
        current.lastUpdate = cachebust;

        dynamic unknown = new ExpandoObject();
        unknown.version = "1.0.0";
        unknown.lastUpdate = DateTime.Now.ToString("yyyyMMddHH");

        pkg.game = unknown;
        pkg.launcher = unknown;

        if (type == PatcherType.Launcher)
            pkg.launcher = current;
        else if (type == PatcherType.Game)
            pkg.game = current;

        return Ok(pkg);
    }
}
