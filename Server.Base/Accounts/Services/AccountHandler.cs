using Microsoft.Extensions.Logging;
using Server.Base.Accounts.Enums;
using Server.Base.Accounts.Extensions;
using Server.Base.Accounts.Helpers;
using Server.Base.Accounts.Modals;
using Server.Base.Core.Helpers;
using Server.Base.Core.Models;
using Server.Base.Core.Services;
using Server.Base.Logging;
using Server.Base.Network;
using Server.Base.Network.Helpers;
using System.Net;

namespace Server.Base.Accounts.Services;

public class AccountHandler : DataHandler<Account>
{
    private readonly AccountAttackLimiter _attackLimiter;
    private readonly InternalServerConfig _config;
    private readonly PasswordHasher _hasher;
    private readonly InternalServerConfig _internalServerConfig;
    private readonly IpLimiter _ipLimiter;
    private readonly NetworkLogger _networkLogger;

    public Dictionary<IPAddress, int> IpTable;

    public AccountHandler(EventSink sink, ILogger<Account> logger, InternalServerConfig internalServerConfig,
        PasswordHasher hasher, AccountAttackLimiter attackLimiter, IpLimiter ipLimiter,
        NetworkLogger networkLogger, InternalServerConfig config) : base(sink, logger)
    {
        _internalServerConfig = internalServerConfig;
        _hasher = hasher;
        _attackLimiter = attackLimiter;
        _ipLimiter = ipLimiter;
        _networkLogger = networkLogger;
        _config = config;
        IpTable = new Dictionary<IPAddress, int>();
    }

    public override void Initialize()
    {
        base.Initialize();
        Sink.ServerStarted += OnAfterLoad;
    }

    public void OnAfterLoad()
    {
        if (Data.Count <= 0)
            CreateDefaultAccount();

        CreateIpTables();
    }

    public void CreateDefaultAccount()
    {
        if (Data.Count != 0)
            return;

        Logger.LogInformation("This server has no accounts.");
        Logger.LogInformation("Please create an owner account now");

        Logger.LogDebug("Username: ");
        var username = Console.ReadLine();

        Logger.LogDebug("Password: ");
        var password = Console.ReadLine();

        if (username == null)
        {
            Logger.LogError("Username for account is null!");
            return;
        }

        Data.Add(username, new Account(username, password, Data.Count, _hasher)
        {
            AccessLevel = AccessLevel.Owner
        });

        Logger.LogInformation("Account created.");
    }

    public AlrReason GetAccount(string username, string password, NetState netState)
    {
        AlrReason rejectReason;

        if (!_internalServerConfig.SocketBlock && !_ipLimiter.Verify(netState.Address))
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
                rejectReason = _internalServerConfig.LockDownLevel > AccessLevel.Vip
                    ? AlrReason.BadComm
                    : AlrReason.BadPass;
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
            _ => throw new ArgumentOutOfRangeException(rejectReason.ToString())
        };

        if (rejectReason == AlrReason.Accepted)
            Logger.LogInformation("Login: {NetState}: {Reason} for '{Username}'", netState, errorReason, username);
        else
            Logger.LogError("Login: {NetState}: {Reason} for '{Username}'", netState, errorReason, username);

        if (rejectReason is not AlrReason.Accepted and not AlrReason.InUse)
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
                Logger.LogError("Unable to parse IPAddress {IP} for {Username}",
                    account.LoginIPs[0], account.Username);
            }
        }
    }

    public bool CanCreate(IPAddress ipAddress) => !IpTable.ContainsKey(ipAddress) || IpTable[ipAddress] < 1;

    private Account CreateAccount(NetState netState, string username, string password)
    {
        if (username.Length == 0)
            return null;

        var isSafe = !(username.StartsWith(" ") || username.EndsWith(" ") || username.EndsWith("."));

        for (var i = 0; isSafe && i < username.Length; ++i)
        {
            isSafe = username[i] >= 0x20 && username[i] < 0x7F &&
                     _internalServerConfig.ForbiddenChars.All(t => username[i] != t);
        }

        for (var i = 0; isSafe && i < password.Length; ++i)
            isSafe = password[i] is >= (char)0x20 and < (char)0x7F;

        if (!isSafe)
            return null;

        if (!CanCreate(netState.Address))
        {
            Logger.LogWarning(
                "Login: {NetState}: Account '{Username}' not created, ip already has {Accounts} account{Plural}.",
                netState, username, _internalServerConfig.MaxAccountsPerIp,
                _internalServerConfig.MaxAccountsPerIp == 1 ? "" : "s");
            return null;
        }

        Logger.LogInformation("Login: {NetState}: Creating new account '{Username}'",
            netState, username);

        return new Account(username, password, Data.Count, _hasher);
    }
}
