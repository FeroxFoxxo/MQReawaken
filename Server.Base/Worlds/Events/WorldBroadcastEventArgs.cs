namespace Server.Base.Worlds.Events;

public class WorldBroadcastEventArgs
{
    public string Message { get; }

    public WorldBroadcastEventArgs(string msg) => Message = msg;
}
