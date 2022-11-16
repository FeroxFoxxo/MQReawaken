namespace Server.Base.Worlds.Events;

public class WorldSaveEventArgs : EventArgs
{
    public bool Message { get; }

    public WorldSaveEventArgs(bool msg) => Message = msg;
}
