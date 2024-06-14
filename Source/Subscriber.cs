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
    private readonly IMetricsHandler _metrics;
    private readonly ILogger _logger;

    public Subscriber(IMetricsHandler metrics, ILogger logger)
    {
        _metrics = metrics;
        _logger = logger;
    }

    public async Task SubscribeToChangesFor(ISession connection, TimeSpan publishInterval, IEnumerable<(NodeId node, TimeSpan samplingInterval)> nodes, Func<NodeValue, Task> handleValue, CancellationToken cancellationToken)
    {
        _logger.Debug("Creating subscription with publish interval {PublishInterval}", publishInterval);

        using var subscription = CreateEmptySubscription(publishInterval);
        var channel = Channel.CreateUnbounded<NodeValue>(new(){ SingleReader = true });

        foreach (var (node, samplingInterval) in nodes)
        {
            _logger.Debug("Adding monitored item for {Node} with sampling interval {SamplingInterval}", node, samplingInterval);
            subscription.AddItem(MonitoringFor(node, samplingInterval, channel));
        }

        _logger.Debug("Adding and creating subscription");
        connection.AddSubscription(subscription);
        await subscription.CreateAsync(cancellationToken).ConfigureAwait(false);

        DeleteSubscriptionWhenCancelled(subscription, cancellationToken);
        CompleteChannelWhenSubscriptionCompletes(subscription, channel);

        _logger.Debug("Starting to read values from subscription");
        await foreach (var value in channel.Reader.ReadAllAsync(CancellationToken.None))
        {
            _logger.Verbose("Received value {Value} from subscription", value);
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
                _logger.Verbose("Received notification without monitored item value in subscription");
                return;
            }

            if (!writer.TryWrite(new(nodeId, monitored.Value)))
            {
                _logger.Error("Failed to write value from subscription to channel. Will drop the received value");
            }
        };

        return monitored;
    }

    private void DeleteSubscriptionWhenCancelled(Subscription subscription, CancellationToken cancellationToken) =>
        cancellationToken.Register(() =>
        {
            _logger.Debug("CancellationToken cancelled, deleting subscription");
            subscription.Delete(true);
        });

    private void CompleteChannelWhenSubscriptionCompletes(Subscription subscription, ChannelWriter<NodeValue> writer)
    {
        subscription.PublishStatusChanged += (_, changed) =>
        {
            _logger.Debug("Subscription publish status changed to {Status}", changed.Status);
            if ((changed.Status & PublishStateChangedMask.Stopped) != 0)
            {
                _logger.Debug("Subscription stopped, completing channel");
                writer.Complete();
            }
        };
        subscription.StateChanged += (_, changed) =>
        {
            _logger.Debug("Subscription state changed to {Status}", changed.Status);
            if ((changed.Status & SubscriptionChangeMask.Deleted) != 0)
            {
                _logger.Debug("Subscription deleted, completing channel");
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
