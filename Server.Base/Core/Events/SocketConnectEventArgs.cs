using System.Net.Sockets;

namespace Server.Base.Core.Events;

public class SocketConnectEventArgs : EventArgs
{
    public Socket Socket { get; }
    public bool AllowConnection { get; set; }

    public SocketConnectEventArgs(Socket s)
    {
        Socket = s;
        AllowConnection = true;
    }
}
