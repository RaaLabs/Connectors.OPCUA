using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Opc.Ua.Client;

namespace RaaLabs.Edge.Connectors.OPCUA;

public interface ICanReadNodes
{
    Task ReadNodesForever(ISession connection, IEnumerable<(string id, TimeSpan readInterval)> nodes, Func<NodeValue,Task> handleValue, CancellationToken cancellationToken);
}

// NOT Task.Delay but PeriodicTimer
// var node = await session.ReadValueAsync(nodeId, CancellationToken.None);
