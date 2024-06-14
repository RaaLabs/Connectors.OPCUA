using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Opc.Ua;
using Opc.Ua.Client;
using Serilog;

namespace RaaLabs.Edge.Connectors.OPCUA;

public class Reader : ICanReadNodes
{
    private readonly ILogger _logger;

    public Reader(ILogger logger)
    {
        _logger = logger;
    }

    public async Task ReadNodesForever(ISession connection, IEnumerable<(NodeId node, TimeSpan readInterval)> nodes, Func<NodeValue, Task> handleValue, CancellationToken cancellationToken)
    {
        _logger.Information("Starting reading nodes...");
        var tasks = new List<Task>();
        foreach (var (node, readInterval) in nodes)
        {
            var task = Task.Run(() => ReadNodeForever(connection, node, readInterval, handleValue, cancellationToken), cancellationToken);
            tasks.Add(task);
        }
        await Task.WhenAll(tasks).ConfigureAwait(false);
    }

    private async Task ReadNodeForever(ISession connection, NodeId node, TimeSpan readInterval, Func<NodeValue, Task> handleValue, CancellationToken cancellationToken)
    {
        using var timer = new PeriodicTimer(readInterval);
        while (!cancellationToken.IsCancellationRequested)
        {
            var dataValue = await connection.ReadValueAsync(node, cancellationToken).ConfigureAwait(false);
            await handleValue(new (node, dataValue)).ConfigureAwait(false);
            await timer.WaitForNextTickAsync(cancellationToken).ConfigureAwait(false);
        }
    }
}
