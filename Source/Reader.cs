// Copyright (c) RaaLabs. All rights reserved.
// Licensed under the GPLv2 License. See LICENSE file in the project root for full license information.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Opc.Ua;
using Opc.Ua.Client;
using Serilog;

namespace RaaLabs.Edge.Connectors.OPCUA;

public class Reader : ICanReadNodes
{
    readonly ILogger _logger;
    readonly IMetricsHandler _metrics;

    public Reader(ILogger logger, IMetricsHandler metrics)
    {
        _logger = logger;
        _metrics = metrics;
    }

    public async Task ReadNodesForever(ISession connection, IEnumerable<(NodeId node, TimeSpan readInterval)> nodes, Func<NodeValue, Task> handleValue, CancellationToken cancellationToken)
    {
        _logger.Information("Start reading nodes...");
        _metrics.NumberOfReadingsStarted(1);
        using var cts = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);

        var tasks = nodes
            .Select(_ => Task.Run(() =>
                ReadNodeForever(connection, _.node, _.readInterval, handleValue, cts.Token)
            , cts.Token))
            .ToList();

        await Task.WhenAny(tasks).ConfigureAwait(false);
        cts.Cancel();

        try
        {
            await Task.WhenAll(tasks).ConfigureAwait(false);
        }
        catch (OperationCanceledException)
        {
            _logger.Information("Read operation was cancelled");
            _metrics.NumberOfReadingsCancelled(1);
        }
        _logger.Debug("Read operation completed...");
    }

    static async Task ReadNodeForever(ISession connection, NodeId node, TimeSpan readInterval, Func<NodeValue, Task> handleValue, CancellationToken cancellationToken)
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
