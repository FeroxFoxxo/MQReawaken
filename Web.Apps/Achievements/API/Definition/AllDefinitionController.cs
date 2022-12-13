using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using Web.Apps.Achievements.Services;

namespace Web.Apps.Achievements.API.Definition;

[Route("Apps/achievements/api/definition/all")]
public class AllDefinitionController : Controller
{
    private readonly AchievementHandler _handler;

    public AllDefinitionController(AchievementHandler handler) => _handler = handler;

    [HttpGet]
    public IActionResult GetAchievements() => Ok(JsonConvert.SerializeObject(_handler.Definitions));
}
