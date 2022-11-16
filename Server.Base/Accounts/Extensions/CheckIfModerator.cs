using Server.Base.Accounts.Enums;
using Server.Base.Accounts.Modals;

namespace Server.Base.Accounts.Extensions;

public static class CheckIfModerator
{
    public static int IsModerator(this Account account) => account.AccessLevel >= AccessLevel.Moderator ? 1 : 0;
}
