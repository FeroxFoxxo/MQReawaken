using Server.Base.Accounts.Extensions;
using Server.Base.Accounts.Modals;
using Server.Base.Logging;
using Server.Base.Network;

namespace Server.Base.Accounts.Helpers;

public class AccountAttackLimiter
{
    private readonly List<InvalidAccountAccessLog> _invalidAccessors;
    private readonly NetworkLogger _networkLogger;

    public AccountAttackLimiter(NetworkLogger networkLogger)
    {
        _networkLogger = networkLogger;
        _invalidAccessors = new List<InvalidAccountAccessLog>();
    }

    public InvalidAccountAccessLog FindAccessLog(NetState netState)
    {
        if (netState == null)
            return null;

        var ipAddress = netState.Address;

        for (var i = 0; i < _invalidAccessors.Count; ++i)
        {
            var accessLog = _invalidAccessors[i];

            if (accessLog.HasExpired)
                _invalidAccessors.RemoveAt(i--);
            else if (accessLog.Address.Equals(ipAddress))
                return accessLog;
        }

        return null;
    }

    public bool ThrottleCallback(NetState ns)
    {
        var accessLog = FindAccessLog(ns);

        if (accessLog == null)
            return true;

        return DateTime.UtcNow >= accessLog.LastAccessTime + accessLog.ComputeThrottle();
    }

    public void RegisterInvalidAccess(NetState netState)
    {
        if (netState == null)
            return;

        var accessLog = FindAccessLog(netState);

        if (accessLog == null)
            _invalidAccessors.Add(accessLog = new InvalidAccountAccessLog(netState.Address));

        accessLog.Counts += 1;
        accessLog.LastAccessTime = DateTime.UtcNow;

        netState.Throttler = ThrottleCallback;

        if (accessLog.Counts < 3)
            return;

        _networkLogger.ThrottledError(netState, accessLog);
    }
}
