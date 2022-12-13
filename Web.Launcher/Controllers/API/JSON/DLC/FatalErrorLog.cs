using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;

namespace Web.Launcher.Controllers.API.JSON.DLC;

[Route("/api/json/dlc/fatalErrorLog")]
public class FatalErrorLog : Controller
{
    private readonly ILogger<FatalErrorLog> _logger;

    public FatalErrorLog(ILogger<FatalErrorLog> logger) => _logger = logger;

    [HttpGet]
    public IActionResult OutputFatalCrash([FromQuery] string username)
    {
        _logger.LogCritical("User '{Username}' encountered a fatal crash!", username);
        return Ok();
    }
}
