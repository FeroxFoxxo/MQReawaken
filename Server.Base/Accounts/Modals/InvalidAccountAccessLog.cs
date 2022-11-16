using System.Net;

namespace Server.Base.Accounts.Modals;

public class InvalidAccountAccessLog
{
    public IPAddress Address { get; set; }

    public int Counts { get; set; }

    public DateTime LastAccessTime { get; set; }

    public bool HasExpired => DateTime.UtcNow >= LastAccessTime + TimeSpan.FromHours(1.0);

    public InvalidAccountAccessLog(IPAddress address)
    {
        Address = address;
        LastAccessTime = DateTime.UtcNow;
    }
}
