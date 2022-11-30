using Microsoft.AspNetCore.Mvc;
using Server.Reawakened.Launcher.Models;

namespace Server.Web.Controllers.API.JSON.DLC;

[Route("api/json/dlc/news")]
public class NewsController : Controller
{
    private readonly LauncherConfig _config;

    public NewsController(LauncherConfig config) => _config = config;

    [HttpGet]
    public IActionResult GetNews() => Ok(_config.News);
}
