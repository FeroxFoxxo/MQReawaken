using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Server.Base.Accounts.Services;
using Server.Base.Core.Models;
using Server.Reawakened.Data.Services;
using Server.Reawakened.Launcher.Services;
using Server.Reawakened.Network.Services;

namespace Server.Web.Controllers.API.JSON.DLC;

[Route("api/json/dlc/shard")]
public class ShardController : Controller
{
    private readonly AccountHandler _accHandler;
    private readonly UserInfoHandler _userInfoHandler;
    private readonly ShardHandler _shardHandler;
    private readonly RandomKeyGenerator _keyGenerator;
    private readonly InternalServerConfig _config;

    public ShardController(AccountHandler accHandler, UserInfoHandler userInfoHandler, ShardHandler shardHandler,
        RandomKeyGenerator keyGenerator, InternalServerConfig config)
    {
        _accHandler = accHandler;
        _userInfoHandler = userInfoHandler;
        _shardHandler = shardHandler;
        _keyGenerator = keyGenerator;
        _config = config;
    }

    [HttpPost]
    public IActionResult GetShardInfo([FromForm] string username, [FromForm] string authToken, [FromForm] int uuid)
    {
        if (!_accHandler.Data.ContainsKey(uuid) || !_userInfoHandler.Data.ContainsKey(uuid))
            return Unauthorized();

        var account = _accHandler.Data[uuid];
        var user = _userInfoHandler.Data[uuid];

        if (account.Username != username || user.AuthToken != authToken)
            return Unauthorized();

        var sId = _keyGenerator.GetRandomKey<ShardHandler>(uuid.ToString());

        _shardHandler.AddData(user, sId);
        _shardHandler.AddData(account, sId);

        var json = new JObject
        {
            { "status", true },
            {
                "sharder", new JObject
                {
                    { "unity.login.sid", sId },
                    { "unity.login.host", $"{_config.GetHostName()}:" }
                }
            }
        };

        return Ok(JsonConvert.SerializeObject(json));
    }
}
