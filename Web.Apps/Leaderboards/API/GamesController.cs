using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using Web.Apps.Leaderboards.Services;

namespace Web.Apps.Leaderboards.API;

[Route("Apps/leaderboards/api/Games")]
public class GamesController : Controller
{
    private readonly LeaderboardHandler _handler;

    public GamesController(LeaderboardHandler handler) => _handler = handler;

    [HttpGet]
    public IActionResult GetGames() => Ok(JsonConvert.SerializeObject(_handler.Games));
}
