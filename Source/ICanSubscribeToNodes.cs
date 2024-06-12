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

// var subscription = new Subscription()
// {
//     PublishingInterval = 1000,
//     PublishingEnabled = true,
//     TimestampsToReturn = TimestampsToReturn.Both
// };

// var nodeId = new NodeId("ns=2;s=ismclient.MEI037.MonCh");

// var monitored = new MonitoredItem()
// {
//     StartNodeId = nodeId,
//     SamplingInterval = 10000,
// };
// monitored.Notification += (sender, e) =>
// {
//     if (e.NotificationValue is MonitoredItemNotification not)
//     {
//         Console.WriteLine($"Notification: {not.Value.Value} - {not.Value.ServerTimestamp} and {not.Value.SourceTimestamp}");
//     }
// };
// var other = new MonitoredItem()
// {
//     StartNodeId = new NodeId("ns=2;s=ismclient.MEI038.MonCh"),
//     SamplingInterval = 5000,
// };
// other.Notification += (sender, e) =>
// {
//     if (e.NotificationValue is MonitoredItemNotification not)
//     {
//         Console.WriteLine($"Notification other: {not.Value.Value} - {not.Value.ServerTimestamp} and {not.Value.SourceTimestamp}");
//     }
// };
// subscription.AddItem(monitored);
// subscription.AddItem(other);

// var added = session.AddSubscription(subscription);
// await subscription.CreateAsync();
