// Copyright (c) RaaLabs. All rights reserved.
// Licensed under the GPLv2 License. See LICENSE file in the project root for full license information.

using System;
using System.Linq;
using System.Threading;
using Machine.Specifications;
using Opc.Ua;

namespace RaaLabs.Edge.Connectors.OPCUA.for_Subscriber.when_subscribing_to_changes;

public class for_some_nodes : given.a_subscriber_and_arguments
{
    Because of = async () => await subscriber.SubscribeToChangesFor(
        session.Object,
        TimeSpan.FromMilliseconds(1),
        [
            (new NodeId(13), TimeSpan.FromSeconds(2)),
            (new NodeId(14), TimeSpan.FromSeconds(3))
        ],
        handler,
        CancellationToken.None
    );

    It should_create_a_subscription_with_publishing_enabled_and_the_correct_interval = () => session.Verify(_ => _.SetPublishingModeAsync(
        Moq.It.IsAny<RequestHeader>(),
        true,
        Moq.It.IsAny<UInt32Collection>(),
        Moq.It.IsAny<CancellationToken>()
    ));
    It should_create_monitored_items_for_the_two_nodes = () => session.Verify(_ => _.CreateMonitoredItemsAsync(
        Moq.It.IsAny<RequestHeader>(),
        1337,
        TimestampsToReturn.Both,
        Moq.It.Is<MonitoredItemCreateRequestCollection>(_ => _.Count == 2
            && _.Exists(_ => _.ItemToMonitor.NodeId == new NodeId(13) && _.ItemToMonitor.AttributeId == Attributes.Value)
            && _.Exists(_ => _.ItemToMonitor.NodeId == new NodeId(14) && _.ItemToMonitor.AttributeId == Attributes.Value)
        ),
        Moq.It.IsAny<CancellationToken>()
    ));
    It should_have_two_monitored_items = () => last_added_subscription.MonitoredItems.Count().ShouldEqual(2);
}
