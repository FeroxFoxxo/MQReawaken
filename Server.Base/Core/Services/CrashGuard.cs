using System.Diagnostics;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Server.Base.Core.Abstractions;
using Server.Base.Core.Events;
using Server.Base.Core.Extensions;
using Server.Base.Core.Helpers;
using Server.Base.Network.Services;
using Server.Base.Worlds;

namespace Server.Base.Core.Services;

public class CrashGuard : IService
{
    private readonly NetStateHandler _handler;
    private readonly ILogger<CrashGuard> _logger;
    private readonly Module[] _modules;
    private readonly EventSink _sink;
    private readonly World _world;

    public CrashGuard(NetStateHandler handler, ILogger<CrashGuard> logger, EventSink sink, World world,
        IServiceProvider serviceProvider)
    {
        _handler = handler;
        _logger = logger;
        _sink = sink;
        _world = world;

        _modules = serviceProvider.GetServices<Module>().ToArray();
    }

    public void Initialize() => _sink.Crashed += OnCrash;

    public void OnCrash(CrashedEventArgs e)
    {
        GenerateCrashReport(e);

        _world.WaitForWriteCompletion();

        Backup();

        Restart(e);
    }

    private void Restart(CrashedEventArgs e)
    {
        _logger.LogInformation("Restarting...");

        try
        {
            Process.Start(GetExePath.Path());
            _logger.LogDebug("Successfully restarted!");

            e.Close = true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to restart server");
        }
    }

    private void Backup()
    {
        _logger.LogInformation("Backing up...");

        try
        {
            var timeStamp = GetTime.GetTimeStamp();

            var root = GetRoot();
            var rootBackup = InternalDirectory.Combine(root, $"Backups/Crashed/{timeStamp}/");
            var rootOrigin = InternalDirectory.Combine(root, "Saves/");

            InternalDirectory.CreateDirectory(rootBackup);
            InternalDirectory.CreateDirectory(rootBackup, "Accounts/");

            CopyFile(rootOrigin, rootBackup, "Accounts/Accounts.xml");

            _logger.LogDebug("Backed up!");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unable to back up server.");
        }
    }

    private void CopyFile(string rootOrigin, string rootBackup, string path)
    {
        var originPath = InternalDirectory.Combine(rootOrigin, path);
        var backupPath = InternalDirectory.Combine(rootBackup, path);

        try
        {
            if (File.Exists(originPath))
                File.Copy(originPath, backupPath);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unable to copy file.");
        }
    }

    private string GetRoot()
    {
        try
        {
            return Path.GetDirectoryName(Environment.GetCommandLineArgs()[0]);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unable to get root.");
            return "";
        }
    }

    private void GenerateCrashReport(CrashedEventArgs crashedEventArgs)
    {
        _logger.LogInformation("Generating report...");

        try
        {
            var timeStamp = GetTime.GetTimeStamp();
            var fileName = $"Crash {timeStamp}.log";

            var root = GetRoot();
            var filePath = InternalDirectory.Combine(root, fileName);

            using (var streamWriter = new StreamWriter(filePath))
            {
                streamWriter.WriteLine("Server Crash Report");
                streamWriter.WriteLine("===================");
                streamWriter.WriteLine();

                foreach (var module in _modules)
                    streamWriter.WriteLine(module.GetModuleInformation());
                streamWriter.WriteLine("Operating System: {0}", Environment.OSVersion);
                streamWriter.WriteLine(".NET Framework: {0}", Environment.Version);
                streamWriter.WriteLine("Time: {0}", DateTime.UtcNow);

                streamWriter.WriteLine();
                streamWriter.WriteLine("Exception:");
                streamWriter.WriteLine(crashedEventArgs.Exception);
                streamWriter.WriteLine();

                streamWriter.WriteLine("Clients:");

                try
                {
                    var netStates = _handler.Instances;

                    streamWriter.WriteLine("- Count: {0}", netStates.Count);

                    foreach (var netState in netStates)
                    {
                        streamWriter.Write("+ {0}:", netState);

                        var account = netState.Account;

                        if (account != null)
                            streamWriter.Write(" (Account = {0})", account.Username);

                        streamWriter.WriteLine();
                    }
                }
                catch
                {
                    streamWriter.WriteLine("- Failed");
                }
            }


            _logger.LogDebug("Logged error!");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unable to log error.");
        }
    }
}
