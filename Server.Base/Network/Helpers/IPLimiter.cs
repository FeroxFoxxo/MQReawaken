using System.Net;
using Server.Base.Core.Models;
using Server.Base.Network.Services;

namespace Server.Base.Network.Helpers;

public class IpLimiter
{
    private readonly NetStateHandler _handler;
    private readonly InternalServerConfig _internalServerConfig;
    private readonly Dictionary<IPAddress, IPAddress> _ipAddressTable;

    public IpLimiter(InternalServerConfig internalServerConfig, NetStateHandler handler)
    {
        _internalServerConfig = internalServerConfig;
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

            if (count >= _internalServerConfig.MaxAddresses)
                return false;
        }

        return true;
    }
}