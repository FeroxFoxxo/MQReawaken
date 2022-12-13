using Server.Base.Core.Abstractions;

namespace Web.Apps.Chat.Models;

public class ChatConfig : IConfig
{
    public string CrispKey { get; set; }
    public string TerminationCharacter { get; set; }
    public string[] Words { get; set; }

    public ChatConfig()
    {
        CrispKey = "654472ea5fbeadc4da5f5de312ffae7e";
        TerminationCharacter = "\r\n";
        Words = new[]
        {
            "Example",
            "Test"
        };
    }
}
