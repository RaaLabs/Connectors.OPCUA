using System.Threading;
using System.Threading.Tasks;
using Opc.Ua.Client;
using RaaLabs.Edge.Connectors.OPCUA.Events;
using RaaLabs.Edge.Modules.EventHandling;

namespace RaaLabs.Edge.Connectors.OPCUA;

public interface IRetrieveData
{
    Task ReadDataForever(Session connection, EventEmitter<OpcuaDatapointOutput> sendDatapoint, CancellationToken cancellationToken);
    // 1. Create a subscription with monitored item fora ll nodes with subscribeinterval set
    // 2. Create one task per node with readinterval set (that reads in loop with timerasync)
}
