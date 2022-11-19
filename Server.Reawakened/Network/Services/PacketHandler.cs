using System.Reflection;
using System.Xml;
using Microsoft.Extensions.DependencyInjection;
using Server.Base.Core.Abstractions;
using Server.Base.Core.Extensions;
using Server.Base.Core.Helpers;
using Server.Base.Logging;
using Server.Base.Network;
using Server.Base.Network.Services;
using Server.Reawakened.Data.Services;
using Server.Reawakened.Network.Helpers;
using Server.Reawakened.Network.Protocols;
using Module = Server.Base.Core.Abstractions.Module;

namespace Server.Reawakened.Network.Services;

public class PacketHandler : IService
{
    public delegate void ExternalCallback(NetState state, string[] message, IServiceProvider serviceProvider);

    public delegate void SystemCallback(NetState state, XmlDocument document, IServiceProvider serviceProvider);

    private readonly NetStateHandler _handler;

    private readonly NetworkLogger _logger;

    private readonly Dictionary<string, SystemCallback> _protocolsSystem;
    private readonly Dictionary<string, ExternalCallback> _protocolsXt;

    private readonly ReflectionUtils _reflectionUtils;
    private readonly IServiceProvider _services;
    private readonly EventSink _sink;
    private readonly UserHandler _userHandler;

    public PacketHandler(IServiceProvider services, ReflectionUtils reflectionUtils, NetworkLogger logger,
        NetStateHandler handler, UserHandler userHandler, EventSink sink)
    {
        _services = services;
        _reflectionUtils = reflectionUtils;
        _logger = logger;
        _handler = handler;
        _userHandler = userHandler;
        _sink = sink;
        _protocolsXt = new Dictionary<string, ExternalCallback>();
        _protocolsSystem = new Dictionary<string, SystemCallback>();
    }

    public void Initialize() => _sink.ServerStarted += AddProtocols;

    private void AddProtocols()
    {
        foreach (var type in _services.GetRequiredServices<Module>().Select(m => m.GetType().Assembly.GetTypes())
                     .SelectMany(sl => sl).Where(myType => myType.IsClass && !myType.IsAbstract))
        {
            if (type.IsSubclassOf(typeof(SystemProtocol)))
            {
                var createInstance =
                    _reflectionUtils.CreateBuilder<SystemProtocol>(type.GetTypeInfo());

                void Callback(NetState state, XmlDocument document, IServiceProvider services)
                {
                    var instance = createInstance(services);

                    instance.InitializeProtocol(state, _userHandler);
                    instance.Run(document);
                }

                _protocolsSystem.Add(createInstance(_services).ProtocolName, Callback);
            }
            else if (type.IsSubclassOf(typeof(ExternalProtocol)))
            {
                var createInstance =
                    _reflectionUtils.CreateBuilder<ExternalProtocol>(type.GetTypeInfo());

                void Callback(NetState state, string[] msg, IServiceProvider services)
                {
                    var instance = createInstance(services);

                    instance.InitializeProtocol(state, _userHandler);
                    instance.Run(msg);
                }

                _protocolsXt.Add(createInstance(_services).ProtocolName, Callback);
            }
        }

        _handler.Protocols.Add('%', SendXt);
        _handler.Protocols.Add('<', SendSys);
    }

    public void SendXt(NetState netState, string packet)
    {
        var splitPacket = packet.Split('%');
        var actionType = splitPacket[3];

        if (_protocolsXt.ContainsKey(actionType))
            _protocolsXt[actionType](netState, splitPacket, _services);
        else
            _logger.TracePacketError(actionType, packet, netState);
    }

    public void SendSys(NetState netState, string packet)
    {
        XmlDocument xmlDocument = new();
        xmlDocument.LoadXml(packet);
        var actionType = xmlDocument.SelectSingleNode("/msg/body/@action")?.Value;

        if (actionType != null && _protocolsSystem.ContainsKey(actionType))
            _protocolsSystem[actionType](netState, xmlDocument, _services);
        else
            _logger.TracePacketError(actionType, packet, netState);
    }
}
