using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Opc.Ua.Client;

namespace RaaLabs.Edge.Connectors.OPCUA;

public interface ICanSubscribeToNodes
{
    Task SubscribeToChangesFor(Session connection, TimeSpan publishInterval, IEnumerable<(string id, TimeSpan samplingInterval)> nodes, Func<NodeValue,Task> handleValue, CancellationToken cancellationToken);
}
