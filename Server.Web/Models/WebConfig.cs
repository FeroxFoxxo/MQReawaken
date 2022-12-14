using Server.Base.Core.Abstractions;

namespace Server.Web.Models;

public class WebConfig : IConfig
{
    public bool ShouldConcat { get; set; }

    public WebConfig() => ShouldConcat = true;
}
