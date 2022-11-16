using System.Net;
using Server.Base.Accounts.Enums;
using Server.Base.Accounts.Modals;
using Server.Base.Accounts.Services;
using Server.Base.Core.Extensions;
using Server.Base.Core.Models;
using Server.Base.Network;

namespace Server.Base.Accounts.Extensions;

public static class CheckAccessRights
{
    public static bool CheckAccess(this Account account, NetState netState, AccountHandler handler,
        ServerConfig config) =>
        netState != null && account.CheckAccess(netState.Address, handler, config);

    public static bool HasAccess(this Account account, NetState netState, AccountHandler handler,
        ServerConfig config) =>
        netState != null && account.HasAccess(netState.Address, handler, config);

    public static bool CheckAccess(this Account account, IPAddress ipAddress, AccountHandler handler,
        ServerConfig config)
    {
        var hasAccess = account.HasAccess(ipAddress, handler, config);

        if (hasAccess)
            account.LogAccess(ipAddress, handler);

        return hasAccess;
    }

    public static bool HasAccess(this Account account, IPAddress ipAddress, AccountHandler handler, ServerConfig config)
    {
        var accessLevel = config.LockDownLevel;

        if (accessLevel >= AccessLevel.Moderator)
        {
            var hasAccess = account.AccessLevel >= accessLevel;

            if (!hasAccess)
                return false;
        }

        var accessAllowed = account.IpRestrictions.Length == 0;

        for (var i = 0; !accessAllowed && i < account.IpRestrictions.Length; ++i)
            accessAllowed = ipAddress.IpMatch(account.IpRestrictions[i]);

        return accessAllowed;
    }

    public static void LogAccess(this Account account, NetState netState, AccountHandler handler, ServerConfig config)
    {
        if (netState != null)
            account.LogAccess(netState.Address, handler);
    }

    public static void LogAccess(this Account account, IPAddress ipAddress, AccountHandler handler)
    {
        if (account.LoginIPs.Length == 0)
            if (handler.IpTable.ContainsKey(ipAddress))
                handler.IpTable[ipAddress]++;
            else
                handler.IpTable[ipAddress] = 1;

        var contains = false;

        for (var i = 0; !contains && i < account.LoginIPs.Length; ++i)
            contains = account.LoginIPs[i].Equals(ipAddress.ToString());

        if (contains)
            return;

        var oldIPs = account.LoginIPs;

        account.LoginIPs = new string[oldIPs.Length + 1];

        for (var i = 0; i < oldIPs.Length; ++i)
            account.LoginIPs[i] = oldIPs[i];

        account.LoginIPs[oldIPs.Length] = ipAddress.ToString();
    }
}
