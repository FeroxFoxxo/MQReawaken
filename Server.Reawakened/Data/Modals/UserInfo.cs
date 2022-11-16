using A2m.Server;

namespace Server.Reawakened.Data.Modals;

public class UserInfo
{
    public string LastCharacterSelected { get; set; }

    public List<CharacterData> Characters { get; set; }

    public Dictionary<CharacterInfoHandler.ExternalProperties, string> Properties { get; set; }

    public Dictionary<DebugHandler.DebugVariables, string> DebugValues { get; set; }

    public UserInfo()
    {
        DebugValues = new Dictionary<DebugHandler.DebugVariables, string>
        {
            { DebugHandler.DebugVariables.Sharder_active, "On" },
            { DebugHandler.DebugVariables.Sharder_1, "On" },
            { DebugHandler.DebugVariables.Sharder_2, "On" },
            { DebugHandler.DebugVariables.Ewallet, "On" },
            { DebugHandler.DebugVariables.Chat, "On" },
            { DebugHandler.DebugVariables.BugReport, "On" },
            { DebugHandler.DebugVariables.Crisp, "On" },
            { DebugHandler.DebugVariables.Trade, "On" }
        };
        Properties = new Dictionary<CharacterInfoHandler.ExternalProperties, string>
        {
            { CharacterInfoHandler.ExternalProperties.Chat_Level, "3" },
            { CharacterInfoHandler.ExternalProperties.Gender, "Male" },
            { CharacterInfoHandler.ExternalProperties.Country, "US" },
            { CharacterInfoHandler.ExternalProperties.Age, "17" },
            { CharacterInfoHandler.ExternalProperties.Birthdate, "11/24/2004" },
            { CharacterInfoHandler.ExternalProperties.AccountAge, "12000" },
            { CharacterInfoHandler.ExternalProperties.Silent, "0" },
            { CharacterInfoHandler.ExternalProperties.Uuid, "1" },
            { CharacterInfoHandler.ExternalProperties.AccessRights, "2" },
            { CharacterInfoHandler.ExternalProperties.ClearCache, "0" },
            {
                CharacterInfoHandler.ExternalProperties.Now, DateTimeOffset.Now.ToUnixTimeSeconds().ToString()
            },
            { CharacterInfoHandler.ExternalProperties.Subscriber, "1" }
        };
        Characters = new List<CharacterData>();
        LastCharacterSelected = "";
    }
}
