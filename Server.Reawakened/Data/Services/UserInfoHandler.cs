using Microsoft.Extensions.Logging;
using Server.Base.Accounts.Modals;
using Server.Base.Core.Helpers;
using Server.Base.Core.Services;
using Server.Base.Network;
using Server.Reawakened.Data.Enums;
using Server.Reawakened.Data.Modals;
using Server.Reawakened.Network.Services;
using System.Globalization;

namespace Server.Reawakened.Data.Services;

public class UserInfoHandler : DataHandler<UserInfo>
{
    private readonly ILogger<UserInfo> _logger;
    private readonly RandomKeyGenerator _randomKeyGenerator;

    public UserInfoHandler(EventSink sink, ILogger<UserInfo> logger, RandomKeyGenerator randomKeyGenerator) : base(sink,
        logger)
    {
        _logger = logger;
        _randomKeyGenerator = randomKeyGenerator;
    }

    public void InitializeUser(NetState state)
    {
        var userId = state.Get<Account>()?.UserId ?? throw new ArgumentNullException(nameof(Account));

        if (!Data.ContainsKey(userId))
            throw new NullReferenceException();

        state.Set(new Player(Data[userId]));
    }

    public override UserInfo CreateDefault()
    {
        Gender gender;

        while (true)
        {
            Logger.LogDebug("Gender: ");

            if (Enum.TryParse(Console.ReadLine(), true, out gender))
                break;

            Logger.LogWarning("Incorrect input! Must be either: {Types}",
                string.Join(", ", Enum.GetNames<Gender>()));
        }

        DateTime dob;

        while (true)
        {
            Logger.LogDebug("Date Of Birth: ");

            if (DateTime.TryParse(Console.ReadLine(), out dob))
                break;

            Logger.LogWarning("Incorrect input! Must be a date!");
        }

        return new UserInfo(Data.Count, gender, dob, RegionInfo.CurrentRegion.Name, _randomKeyGenerator);
    }

    public override UserInfo Create(NetState netState, params string[] obj) => throw new NotImplementedException();
}
