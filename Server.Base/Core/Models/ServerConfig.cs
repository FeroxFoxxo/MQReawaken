using Server.Base.Accounts.Enums;

namespace Server.Base.Core.Models;

public class ServerConfig
{
    public int MaxAccountsPerIp { get; set; }
    public char[] ForbiddenChars { get; set; }
    public AccessLevel LockDownLevel { get; set; }
    public int MaxAddresses { get; set; }
    public bool SocketBlock { get; set; }
    public int BreakCount { get; set; }
    public double[] Delays { get; set; }
    public int GlobalUpdateRange { get; set; }
    public int RandomKeyLength { get; set; }
    public int BufferSize { get; set; }
    public int PlayerCap { get; set; }
    public int BackupCapacity { get; set; }
    public int RestartWarningSeconds { get; set; }
    public int RestartDelaySeconds { get; set; }
    public int RestartAutomaticallyHours { get; set; }
    public int SaveWarningMinutes { get; set; }
    public int SaveAutomaticallyMinutes { get; set; }
    public string[] Backups { get; set; }

    public ServerConfig()
    {
        MaxAccountsPerIp = 1;
        ForbiddenChars = new[]
        {
            '<', '>', ':', '"', '/', '\\', '|', '?', '*', ' ', '%'
        };
        LockDownLevel = AccessLevel.Player;
        MaxAddresses = 10;
        SocketBlock = true;
        BreakCount = 20000;
        Delays = new double[] { 0, 10, 25, 50, 250, 1000, 5000, 60000 };
        GlobalUpdateRange = 18;
        RandomKeyLength = 16;
        BufferSize = 65535;
        PlayerCap = 100;
        BackupCapacity = 64;
        RestartWarningSeconds = 60;
        RestartDelaySeconds = 10;
        RestartAutomaticallyHours = 24;
        SaveWarningMinutes = 1;
        SaveAutomaticallyMinutes = 5;
        Backups = new[]
        {
            "Third Backup",
            "Second Backup",
            "Most Recent"
        };
    }
}
