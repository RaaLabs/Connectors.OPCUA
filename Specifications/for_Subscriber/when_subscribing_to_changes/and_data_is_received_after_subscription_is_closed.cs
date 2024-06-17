// Copyright (c) RaaLabs. All rights reserved.
// Licensed under the GPLv2 License. See LICENSE file in the project root for full license information.

using System;
using System.Linq;
using System.Threading;
using Machine.Specifications;
using Opc.Ua;

namespace RaaLabs.Edge.Connectors.OPCUA.for_Subscriber.when_subscribing_to_changes;

public class and_data_is_received_after_subscription_is_closed : given.a_subscriber_and_arguments
{
    Establish context = async () =>
    {
        await subscriber.SubscribeToChangesFor(
            session.Object,
            TimeSpan.FromMilliseconds(1),
            [
                (new NodeId(13), TimeSpan.FromSeconds(2)),
                (new NodeId(14), TimeSpan.FromSeconds(3))
            ],
            handler,
            CancellationToken.None
        );
    };

    Because of = () =>
    {
        last_added_subscription.MonitoredItems.Single(_ => _.StartNodeId == new NodeId(13)).SaveValueInCache(new MonitoredItemNotification() { Value = new(new Variant("hello there")) });
    };

    It should_not_have_received_anything = () => handled_values.ShouldBeEmpty();
    It should_log_an_error_because_it_should_not_happen = () => logger.Verify(_ => _.Error(Moq.It.Is<string>(_ => _.Contains("Will drop the received value"))));
}
