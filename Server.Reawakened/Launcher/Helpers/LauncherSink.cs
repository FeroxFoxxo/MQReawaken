namespace Server.Reawakened.Launcher.Helpers;

public class LauncherSink
{
    public delegate void GameLaunchEventHandler();

    public void InvokeGameLaunch() => GameLaunching?.Invoke();

    public event GameLaunchEventHandler GameLaunching;
}
