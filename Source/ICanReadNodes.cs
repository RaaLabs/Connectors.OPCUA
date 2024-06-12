using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Opc.Ua.Client;

namespace RaaLabs.Edge.Connectors.OPCUA;

public interface ICanReadNodes
{
    Task ReadNodesForever(Session connection, IEnumerable<(string id, TimeSpan readInterval)> nodes, Func<NodeValue,Task> handleValue, CancellationToken cancellationToken);
}
