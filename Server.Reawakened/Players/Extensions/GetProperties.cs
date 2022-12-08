using A2m.Server;
using Server.Base.Accounts.Modals;
using Server.Reawakened.Players.Modals;

namespace Server.Reawakened.Players.Extensions;

public static class GetProperties
{
    public static Dictionary<CharacterInfoHandler.ExternalProperties, string> GenerateProperties(this UserInfo info,
        Account account) =>
        new()
        {
            { CharacterInfoHandler.ExternalProperties.Chat_Level, "3" },
            { CharacterInfoHandler.ExternalProperties.Gender, Enum.GetName(info.Gender) },
            { CharacterInfoHandler.ExternalProperties.Country, info.Region },
            {
                CharacterInfoHandler.ExternalProperties.Age,
                Convert.ToInt32(Math.Floor((DateTime.UtcNow - DateTime.Parse(info.DateOfBirth)).TotalDays / 365.2425))
                    .ToString()
            },
            { CharacterInfoHandler.ExternalProperties.Birthdate, info.DateOfBirth },
            { CharacterInfoHandler.ExternalProperties.AccountAge, account.Created },
            { CharacterInfoHandler.ExternalProperties.Silent, "0" },
            { CharacterInfoHandler.ExternalProperties.Uuid, info.UserId.ToString() },
            { CharacterInfoHandler.ExternalProperties.AccessRights, "2" },
            { CharacterInfoHandler.ExternalProperties.ClearCache, "0" },
            {
                CharacterInfoHandler.ExternalProperties.Now, DateTimeOffset.Now.ToUnixTimeSeconds().ToString()
            },
            { CharacterInfoHandler.ExternalProperties.Subscriber, info.Member ? "1" : "0" }
        };

    public static string GetPropertyValues(this UserInfo user, Account account) =>
        string.Join('|', user.GenerateProperties(account).Select(x => $"{(int)x.Key}|{x.Value}"));
}
