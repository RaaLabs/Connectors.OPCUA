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

public class DataReader : IRetrieveData
{
    private readonly TimeSpan _publishInterval;
    private readonly List<(NodeId, TimeSpan)> _subscribeNodes;
    private readonly List<(NodeId, TimeSpan)> _readNodes;
    private readonly ICanSubscribeToNodes _subscriber;
    private readonly ICanReadNodes _reader;
    private readonly ILogger _logger;

    public DataReader(ConnectorConfiguration config, ICanSubscribeToNodes subscriber, ICanReadNodes reader, ILogger logger)
    {
        _publishInterval = TimeSpan.FromSeconds(config.PublishIntervalSeconds);
        _subscribeNodes = config.Nodes
            .Where(_ => _.SubscribeIntervalSeconds is not null)
            .Select(_ => (new NodeId(_.NodeId), TimeSpan.FromSeconds(_.SubscribeIntervalSeconds!.Value)))
            .ToList();
        _readNodes = config.Nodes
            .Where(_ => _.ReadIntervalSeconds is not null)
            .Select(_ => (new NodeId(_.NodeId), TimeSpan.FromSeconds(_.ReadIntervalSeconds!.Value)))
            .ToList();
        _subscriber = subscriber;
        _reader = reader;
        _logger = logger;
    }

    public async Task ReadDataForever(ISession connection, Func<NodeValue, Task> handleValue, CancellationToken cancellationToken)
    {
        using var cts = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);

        var (subscription, reader) = (Task.CompletedTask, Task.CompletedTask);
        try
        {
            _logger.Information("Starting data reader for {SubscriptionCount} subscriptions and {ReadCount} reads", _subscribeNodes.Count, _readNodes.Count);

            subscription = SubscribeOrSleep(connection, handleValue, cts.Token);
            reader = ReadOrSleep(connection, handleValue, cts.Token);

            await Task.WhenAny(subscription, reader).ConfigureAwait(false);

            ThrowIfFailedWithError(subscription);
            ThrowIfFailedWithError(reader);

            _logger.Warning("Reading data completed, it should not...");
        }
        catch (Exception error) when (error is not OperationCanceledException)
        {
            _logger.Error(error, "Failure occured while reading data");
        }

        cts.Cancel();
        try
        {
            _logger.Information("Waiting for subscription and reader to complete");
            await subscription.ConfigureAwait(false);
            await reader.ConfigureAwait(false);
        }
        catch (OperationCanceledException)
        {
            // Ignore
        }
    }

    private Task SubscribeOrSleep(ISession connection, Func<NodeValue, Task> handleValue, CancellationToken cancellationToken) =>
        _subscribeNodes.Count switch
        {
            0 => Task.Delay(Timeout.InfiniteTimeSpan, cancellationToken),
            _ => Task.Run(() => _subscriber.SubscribeToChangesFor(connection, _publishInterval, _subscribeNodes, handleValue, cancellationToken), cancellationToken)
        };

    private Task ReadOrSleep(ISession connection, Func<NodeValue, Task> handleValue, CancellationToken cancellationToken) =>
        _readNodes.Count switch
        {
            0 => Task.Delay(Timeout.InfiniteTimeSpan, cancellationToken),
            _ => Task.Run(() => _reader.ReadNodesForever(connection, _readNodes, handleValue, cancellationToken), cancellationToken)
        };

    private static void ThrowIfFailedWithError(Task task)
    {
        if (task.Exception?.GetBaseException() is {} error and not OperationCanceledException)
        {
            throw error;
        }
    }
}
