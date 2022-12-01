using A2m.Server;
using Server.Base.Core.Models;
using Server.Reawakened.Data.Enums;
using Server.Reawakened.Network.Services;
using System.Globalization;

namespace Server.Reawakened.Data.Modals;

public class UserInfo : PersistantData
{
    public string LastCharacterSelected { get; set; }

    public List<CharacterData> Characters { get; set; }

    public string AuthToken { get; set; }

    public Gender Gender { get; set; }

    public string DateOfBirth { get; set; }

    public bool Member { get; set; }

    public string SignUpExperience { get; set; }

    public string Region { get; set; }

    public string TrackingShortId { get; set; }

    public UserInfo()
    {
    }

    public UserInfo(int userId, Gender gender, DateTime dateOfBirth, string region, RandomKeyGenerator kGen)
    {
        Region = region;
        UserId = userId;
        Gender = gender;
        DateOfBirth = dateOfBirth.ToString(CultureInfo.CurrentCulture);
        LastCharacterSelected = "";
        Characters = new List<CharacterData>();
        SignUpExperience = "unknown";
        Member = true;
        TrackingShortId = "false";
        AuthToken = kGen.GetRandomKey<UserInfo>(userId.ToString());
    }
}
