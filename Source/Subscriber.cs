using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Channels;
using System.Threading.Tasks;
using Opc.Ua;
using Opc.Ua.Client;
using Serilog;

namespace RaaLabs.Edge.Connectors.OPCUA;

public class Subscriber : ICanSubscribeToNodes
{
    private readonly ILogger _logger;

    public Subscriber(ILogger logger)
    {
        _logger = logger;
    }

    public async Task SubscribeToChangesFor(ISession connection, TimeSpan publishInterval, IEnumerable<(NodeId node, TimeSpan samplingInterval)> nodes, Func<NodeValue, Task> handleValue, CancellationToken cancellationToken)
    {
        using var subscription = CreateEmptySubscription(publishInterval);
        var channel = Channel.CreateUnbounded<NodeValue>(new(){ SingleReader = true });

        foreach (var (node, samplingInterval) in nodes)
        {
            subscription.AddItem(MonitoringFor(node, samplingInterval, channel));
        }

        connection.AddSubscription(subscription);
        await subscription.CreateAsync(cancellationToken).ConfigureAwait(false);

        DeleteSubscriptionWhenCancelled(subscription, cancellationToken);
        CompleteChannelWhenSubscriptionCompletes(subscription, channel);

        await foreach (var value in channel.Reader.ReadAllAsync(CancellationToken.None))
        {
            await handleValue(value).ConfigureAwait(false);
        }
    }

    private MonitoredItem MonitoringFor(NodeId nodeId, TimeSpan samplingInterval, ChannelWriter<NodeValue> writer)
    {
        var monitored = new MonitoredItem()
        {
            StartNodeId = nodeId,
            SamplingInterval = (int)samplingInterval.TotalMilliseconds,
        };

        monitored.Notification += (_, notification) =>
        {
            if (notification.NotificationValue is not MonitoredItemNotification monitored)
            {
                return;
            }

            if (!writer.TryWrite(new(nodeId, monitored.Value)))
            {

            }
        };

        return monitored;
    }

    private void DeleteSubscriptionWhenCancelled(Subscription subscription, CancellationToken cancellationToken) =>
        cancellationToken.Register(() =>
        {
            subscription.Delete(true);
        });

    private void CompleteChannelWhenSubscriptionCompletes(Subscription subscription, ChannelWriter<NodeValue> writer)
    {
        subscription.PublishStatusChanged += (_, changed) =>
        {
            if ((changed.Status & PublishStateChangedMask.Stopped) != 0)
            {
                writer.Complete();
            }
        };
        subscription.StateChanged += (_, changed) =>
        {
            if ((changed.Status & SubscriptionChangeMask.Deleted) != 0)
            {
                writer.Complete();
            }
        };
    }

    private static Subscription CreateEmptySubscription(TimeSpan publishInterval) =>
        new()
        {
            PublishingInterval = (int)publishInterval.TotalMilliseconds,
            PublishingEnabled = true,
            TimestampsToReturn = TimestampsToReturn.Both,
        };
}
