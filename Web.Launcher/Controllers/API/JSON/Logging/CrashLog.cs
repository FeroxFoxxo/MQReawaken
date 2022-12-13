using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Server.Base.Core.Services;

namespace Web.Launcher.Controllers.API.JSON.Logging;

[Route("/api/json/logging/crash_log")]
public class CrashLog : Controller
{
    private readonly ILogger<CrashLog> _logger;
    private readonly ServerHandler _handler;

    public CrashLog(ILogger<CrashLog> logger, ServerHandler handler)
    {
        _logger = logger;
        _handler = handler;
    }

    [HttpPost]
    public IActionResult PrintCrashReport([FromForm] string log)
    {
        _logger.LogCritical("Game Crash Log:");
        _logger.LogError("{Error}",
            string.Join('\n', log.Split('\n').Select(x => x.Trim()).Where(x => !string.IsNullOrEmpty(x))));

        _handler.KillServer(false);

        return Ok();
    }
}
