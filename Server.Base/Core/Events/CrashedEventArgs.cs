namespace Server.Base.Core.Events;

public class CrashedEventArgs : EventArgs
{
    public Exception Exception { get; }
    public bool Close { get; set; }

    public CrashedEventArgs(Exception exception) => Exception = exception;
}
