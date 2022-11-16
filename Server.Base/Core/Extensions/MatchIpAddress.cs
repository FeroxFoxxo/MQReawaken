using System.Net;

namespace Server.Base.Core.Extensions;

public static class MatchIpAddress
{
    public static bool IpMatch(this IPAddress ipAddress, string value) => ipAddress.IpMatchedAndIsValid(value).Item1;

    public static Tuple<bool, bool> IpMatchedAndIsValid(this IPAddress ipAddress, string value) =>
        !IPAddress.TryParse(value, out var ipAddress2)
            ? new Tuple<bool, bool>(false, false)
            : new Tuple<bool, bool>(ipAddress2.Equals(ipAddress), true);
}
