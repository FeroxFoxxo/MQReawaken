using System.Net;
using Server.Base.Core.Models;
using Server.Base.Network.Services;

namespace Server.Base.Network.Helpers;

public class IpLimiter
{
    private readonly NetStateHandler _handler;
    private readonly Dictionary<IPAddress, IPAddress> _ipAddressTable;
    private readonly ServerConfig _serverConfig;

    public IpLimiter(ServerConfig serverConfig, NetStateHandler handler)
    {
        _serverConfig = serverConfig;
        _handler = handler;
        _ipAddressTable = new Dictionary<IPAddress, IPAddress>();
    }

    public IPAddress Intern(IPAddress ipAddress)
    {
        if (_ipAddressTable.TryGetValue(ipAddress, out var interned))
            return interned;

        interned = ipAddress;
        _ipAddressTable[ipAddress] = interned;

        return interned;
    }

    public bool Verify(IPAddress ourAddress)
    {
        var netStates = _handler.Instances;

        var count = 0;

        foreach (var unused in netStates.Where(compState => ourAddress.Equals(compState.Address)))
        {
            ++count;

            if (count >= _serverConfig.MaxAddresses)
                return false;
        }

        return true;
    }
}
