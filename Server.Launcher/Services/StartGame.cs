using Server.Base.Core.Abstractions;
using Server.Base.Core.Helpers;
using Server.Launcher.Internal;
using Server.Reawakened.Data.Modals;
using System.Runtime.InteropServices;

namespace Server.Launcher.Services;

public class StartGame : IService
{
    private readonly ServerConfig _config;
    private readonly EventSink _sink;

    public StartGame(EventSink sink, ServerConfig config)
    {
        _sink = sink;
        _config = config;
    }

    public void Initialize() => _sink.WorldLoad += GetGameInformation;

    private void GetGameInformation()
    {
        _config.BaseDirectory = SetIfNotNull(_config.BaseDirectory, "Get Base Directory",
            "Settings File (*.txt)\0*.txt\0");

        _config.CacheDirectory = SetIfNotNull(_config.CacheDirectory, "Get Cache Directory",
            "Root Info File (__info)\0__info\0");
    }

    private static string? SetIfNotNull(string? setting, string title, string filter)
    {
        var isWindows = RuntimeInformation.IsOSPlatform(OSPlatform.Windows);

        return string.IsNullOrEmpty(setting)
            ? isWindows
                ? FileDialog.GetFile(title, filter)
                : Console.ReadLine()
            : setting;
    }
}
