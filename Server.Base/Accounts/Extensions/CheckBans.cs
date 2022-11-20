using Server.Base.Accounts.Modals;
using Server.Base.Core.Extensions;
using System.Xml;

namespace Server.Base.Accounts.Extensions;

public static class CheckBans
{
    public static bool IsBanned(this Account account)
    {
        var isBanned = account.GetFlag(0);

        if (!isBanned)
            return false;

        if (!account.GetBanTags(out var banTime, out var banDuration) || banDuration == TimeSpan.MaxValue ||
            DateTime.UtcNow < banTime + banDuration)
            return true;

        account.SetUnspecifiedBan(null);
        account.SetBanned(false);
        return false;
    }

    public static void SetBanned(this Account account, bool isBanned) =>
        account.SetFlag(0, isBanned);

    public static void SetBanTags(this Account account, string from, DateTime banTime, TimeSpan banDuration)
    {
        if (from == null)
            account.RemoveTag("BanDealer");
        else
            account.SetTag("BanDealer", from);

        if (banTime == DateTime.MinValue)
            account.RemoveTag("BanTime");
        else
            account.SetTag("BanTime", XmlConvert.ToString(banTime, XmlDateTimeSerializationMode.Utc));

        if (banDuration == TimeSpan.Zero)
            account.RemoveTag("BanDuration");
        else
            account.SetTag("BanDuration", banDuration.ToString());
    }

    public static void SetUnspecifiedBan(this Account account, string from) =>
        account.SetBanTags(from, DateTime.MinValue, TimeSpan.Zero);

    public static bool GetBanTags(this Account account, out DateTime banTime, out TimeSpan banDuration)
    {
        var tagTime = account.GetTag("BanTime");
        var tagDuration = account.GetTag("BanDuration");

        banTime = tagTime != null ? XmlConverter.GetXmlDateTime(tagTime, DateTime.MinValue) : DateTime.MinValue;

        if (tagDuration == "Infinite")
            banDuration = TimeSpan.MaxValue;
        else if (tagDuration != null)
            banDuration = GetTime.ToTimeSpan(tagDuration);
        else
            banDuration = TimeSpan.Zero;

        return banTime != DateTime.MinValue && banDuration != TimeSpan.Zero;
    }
}
