using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Opc.Ua;
using Opc.Ua.Client;
using Serilog;

namespace RaaLabs.Edge.Connectors.OPCUA;

public class NodeReader : ICanReadNodes
{
    private readonly ILogger _logger;

    public NodeReader(ILogger logger)
    {
        _logger = logger;
    }

    public async Task ReadNodesForever(ISession connection, IEnumerable<(NodeId node, TimeSpan readInterval)> nodes, Func<NodeValue, Task> handleValue, CancellationToken cancellationToken)
    {
        _logger.Information("Starting reading nodes...");
        while (!cancellationToken.IsCancellationRequested)
        {
            foreach (var (node, readInterval) in nodes)
            {
                using var timer = new PeriodicTimer(readInterval);
                var dataValue = await connection.ReadValueAsync(node, CancellationToken.None).ConfigureAwait(false);
                await handleValue(new (node, new () {Value = dataValue.Value})).ConfigureAwait(false);
                await timer.WaitForNextTickAsync(cancellationToken).ConfigureAwait(false);
            }
        }
    }
}
