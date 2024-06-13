using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Opc.Ua;
using Opc.Ua.Client;

namespace RaaLabs.Edge.Connectors.OPCUA;

public interface ICanSubscribeToNodes
{
    Task SubscribeToChangesFor(ISession connection, TimeSpan publishInterval, IEnumerable<(NodeId node, TimeSpan samplingInterval)> nodes, Func<NodeValue,Task> handleValue, CancellationToken cancellationToken);
}
