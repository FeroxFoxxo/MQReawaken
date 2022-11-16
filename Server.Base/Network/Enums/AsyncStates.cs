namespace Server.Base.Network.Enums;

[Flags]
public enum AsyncStates
{
    Pending = 0x01,
    Paused = 0x02
}
