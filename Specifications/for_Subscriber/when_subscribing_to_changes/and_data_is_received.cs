using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Machine.Specifications;
using Opc.Ua;

namespace RaaLabs.Edge.Connectors.OPCUA.for_Subscriber.when_subscribing_to_changes;

public class and_data_is_received : given.a_subscriber_and_arguments
{
    static Task running_subscriber;
    Establish context = () => running_subscriber = subscriber.SubscribeToChangesFor(
        session.Object,
        TimeSpan.FromMilliseconds(20),
        [
            (new NodeId(13), TimeSpan.FromSeconds(2)),
            (new NodeId(14), TimeSpan.FromSeconds(3))
        ],
        handler,
        CancellationToken.None
    );

    Because of = async () => 
    {
        last_added_subscription.MonitoredItems.Single(_ => _.StartNodeId == new NodeId(13)).SaveValueInCache(new MonitoredItemNotification() { Value = new(new Variant("hello there")) });
        last_added_subscription.MonitoredItems.Single(_ => _.StartNodeId == new NodeId(14)).SaveValueInCache(new MonitoredItemNotification() { Value = new(new Variant("what is going")) });
        last_added_subscription.MonitoredItems.Single(_ => _.StartNodeId == new NodeId(13)).SaveValueInCache(new MonitoredItemNotification() { Value = new(new Variant("on here")) });
        await running_subscriber;
    };

    It should_have_received_the_value = () => handled_values.ShouldContainOnly(
        new NodeValue(new(13), new(new Variant("hello there"))),
        new NodeValue(new(14), new(new Variant("what is going"))),
        new NodeValue(new(13), new(new Variant("on here")))
    );
}
