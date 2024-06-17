using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Machine.Specifications;
using Opc.Ua;

namespace RaaLabs.Edge.Connectors.OPCUA.for_Subscriber.when_subscribing_to_changes;

public class and_notification_is_recieved_that_is_not_data : given.a_subscriber_and_arguments
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
        last_added_subscription.MonitoredItems.Single(_ => _.StartNodeId == new NodeId(13)).SaveValueInCache(new NotificationData());
        await running_subscriber;
    };

    It should_not_do_anything = () => handled_values.ShouldBeEmpty();
}
