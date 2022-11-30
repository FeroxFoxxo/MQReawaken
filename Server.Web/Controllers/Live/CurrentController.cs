using Microsoft.AspNetCore.Mvc;
using Server.Reawakened.Launcher.Services;

namespace Server.Web.Controllers.Live;

[Route("live/current.txt")]
public class CurrentController : Controller
{
    private readonly StartGame _game;

    public CurrentController(StartGame game) => _game = game;

    [HttpGet]
    public IActionResult GetCurrentData() => Ok(_game.CurrentVersion);
}
