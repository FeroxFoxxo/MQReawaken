namespace Web.Launcher.Helpers;

public class LauncherSink
{
    public void InvokeGameLaunch() => GameLaunching?.Invoke();

    public event GameLaunchEventHandler GameLaunching;

    public delegate void GameLaunchEventHandler();
}
