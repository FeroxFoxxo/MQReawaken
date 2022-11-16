using System.Net;
using Server.Base.Accounts.Enums;
using Server.Base.Accounts.Extensions;
using Server.Base.Accounts.Helpers;
using Server.Base.Accounts.Modals;
using Server.Base.Core.Helpers;
using Server.Base.Core.Models;
using Server.Base.Logging;
using Server.Base.Network;
using Server.Base.Network.Helpers;

namespace Server.Base.Accounts.Services;

public class AccountHandler : DataHandler<Account>
{
    private readonly AccountAttackLimiter _attackLimiter;
    private readonly ServerConfig _config;
    private readonly PasswordHasher _hasher;
    private readonly IpLimiter _ipLimiter;
    private readonly Logger _logger;
    private readonly NetworkLogger _networkLogger;
    private readonly ServerConfig _serverConfig;

    public Dictionary<IPAddress, int> IpTable;

    public AccountHandler(EventSink sink, Logger logger, ServerConfig serverConfig,
        PasswordHasher hasher, AccountAttackLimiter attackLimiter, IpLimiter ipLimiter,
        NetworkLogger networkLogger, ServerConfig config) : base(sink, logger)
    {
        _logger = logger;
        _serverConfig = serverConfig;
        _hasher = hasher;
        _attackLimiter = attackLimiter;
        _ipLimiter = ipLimiter;
        _networkLogger = networkLogger;
        _config = config;
        IpTable = new Dictionary<IPAddress, int>();
    }

    public override void OnAfterLoad()
    {
        if (Data.Count <= 0)
            CreateDefaultAccount();

        CreateIpTables();
    }

    public void CreateDefaultAccount()
    {
        if (Data.Count != 0)
            return;

        _logger.WriteLine(ConsoleColor.Yellow, "This server has no accounts.");
        _logger.Write(ConsoleColor.Yellow, "Do you want to create the owner account now? ( Y / N ) ");

        var key = Console.ReadKey();

        Console.WriteLine();

        if (key.KeyChar.ToString().ToUpper() == "Y")
        {
            _logger.WriteLine(ConsoleColor.Cyan, "Username: ");
            var username = Console.ReadLine();

            _logger.WriteLine(ConsoleColor.Cyan, "Password: ");
            var password = Console.ReadLine();

            if (username == null)
            {
                _logger.WriteLine(ConsoleColor.Green, "Username for account is null!");
                return;
            }

            Data.Add(username, new Account(username, password, Data.Count, _hasher)
            {
                AccessLevel = AccessLevel.Owner
            });

            _logger.WriteLine(ConsoleColor.Green, "Account created.");
        }
        else
        {
            _logger.WriteLine(ConsoleColor.Red, "Account not created.");
        }
    }

    public AlrReason GetAccount(string username, string password, NetState netState)
    {
        AlrReason rejectReason;

        if (!_serverConfig.SocketBlock && !_ipLimiter.Verify(netState.Address))
        {
            _networkLogger.IpLimitedError(netState);
            rejectReason = AlrReason.InUse;
        }
        else
        {
            var account = Get(username);

            if (account == null)
            {
                if (username.Trim().Length > 0)
                {
                    netState.Account = account = CreateAccount(netState, username, password);

                    if (account == null || !account.CheckAccess(netState, this, _config))
                    {
                        rejectReason = AlrReason.BadComm;
                    }
                    else
                    {
                        rejectReason = AlrReason.Accepted;
                        account.LogAccess(netState, this, _config);
                    }
                }
                else
                {
                    rejectReason = AlrReason.Invalid;
                }
            }
            else if (!account.HasAccess(netState, this, _config))
            {
                rejectReason = _serverConfig.LockDownLevel > AccessLevel.Vip ? AlrReason.BadComm : AlrReason.BadPass;
            }
            else if (!_hasher.CheckPassword(account, password))
            {
                rejectReason = AlrReason.BadPass;
            }
            else if (account.IsBanned())
            {
                rejectReason = AlrReason.Blocked;
            }
            else
            {
                netState.Account = account;
                rejectReason = AlrReason.Accepted;

                account.LogAccess(netState, this, _config);
            }
        }

        var errorReason = rejectReason switch
        {
            AlrReason.Accepted => "Valid credentials",
            AlrReason.BadComm => "Access denied",
            AlrReason.BadPass => "Invalid password",
            AlrReason.Blocked => "Banned account",
            AlrReason.InUse => "Past IP limit threshold",
            AlrReason.Invalid => "Invalid username",
            _ => throw new ArgumentOutOfRangeException()
        };

        _logger.WriteLine(ConsoleColor.Red, $"Login: {netState}: {errorReason} for '{username}'");

        if (rejectReason != AlrReason.Accepted && rejectReason != AlrReason.InUse)
            _attackLimiter.RegisterInvalidAccess(netState);

        return rejectReason;
    }

    public void CreateIpTables()
    {
        IpTable = new Dictionary<IPAddress, int>();

        foreach (var account in Data.Values.Where(account => account.LoginIPs.Length > 0))
        {
            if (IPAddress.TryParse(account.LoginIPs[0], out var ipAddress))
            {
                if (IpTable.ContainsKey(ipAddress))
                    IpTable[ipAddress]++;
                else
                    IpTable[ipAddress] = 1;
            }
            else
            {
                _logger.WriteLine(ConsoleColor.Red,
                    $"Unable to parse IPAddress {account.LoginIPs[0]} for {account.Username}");
            }
        }
    }

    public bool CanCreate(IPAddress ipAddress)
    {
        if (!IpTable.ContainsKey(ipAddress))
            return true;

        return IpTable[ipAddress] < 1;
    }

    private Account CreateAccount(NetState netState, string username, string password)
    {
        if (username.Length == 0)
            return null;

        var isSafe = !(username.StartsWith(" ") || username.EndsWith(" ") || username.EndsWith("."));

        for (var i = 0; isSafe && i < username.Length; ++i)
        {
            isSafe = username[i] >= 0x20 && username[i] < 0x7F &&
                     _serverConfig.ForbiddenChars.All(t => username[i] != t);
        }

        for (var i = 0; isSafe && i < password.Length; ++i)
            isSafe = password[i] >= 0x20 && password[i] < 0x7F;

        if (!isSafe)
            return null;

        if (!CanCreate(netState.Address))
        {
            _logger.WriteLine(ConsoleColor.DarkYellow,
                $"Login: {netState}: Account '{username}' not created, ip already has {_serverConfig.MaxAccountsPerIp} " +
                $"account{(_serverConfig.MaxAccountsPerIp == 1 ? "" : "s")}.");
            return null;
        }

        _logger.WriteLine(ConsoleColor.Green, $"Login: {netState}: Creating new account '{username}'");

        return new Account(username, password, Data.Count, _hasher);
    }
}
