using Microsoft.Extensions.Logging;
using Server.Base.Network;
using Server.Base.Network.Services;

namespace Server.Base.Core.Models;

public interface INetStateData
{
    public void RemovedState(NetState state, NetStateHandler handler, ILogger logger);
}
