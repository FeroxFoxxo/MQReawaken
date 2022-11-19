using System.Globalization;
using Server.Base.Accounts.Enums;
using Server.Base.Accounts.Helpers;

namespace Server.Base.Accounts.Modals;

public class Account
{
    public AccessLevel AccessLevel { get; set; }

    public string Created { get; set; }

    public int Flags { get; set; }

    public string[] IpRestrictions { get; set; }

    public string LastLogin { get; set; }

    public string[] LoginIPs { get; set; }

    public string Password { get; set; }

    public List<AccountTag> Tags { get; set; }

    public int UserId { get; set; }

    public string Username { get; set; }

    public Account() { }

    public Account(string username, string password, int userPlayerId, PasswordHasher hasher)
    {
        Username = username;
        Password = hasher.GetPassword(username, password);
        AccessLevel = AccessLevel.Player;
        Created = DateTime.UtcNow.ToString(CultureInfo.CurrentCulture);
        LastLogin = DateTime.UtcNow.ToString(CultureInfo.CurrentCulture);
        IpRestrictions = Array.Empty<string>();
        LoginIPs = Array.Empty<string>();
        Tags = new List<AccountTag>();
        UserId = userPlayerId;
    }
}
