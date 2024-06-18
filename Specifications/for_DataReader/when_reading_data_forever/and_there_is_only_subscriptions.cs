// Copyright (c) RaaLabs. All rights reserved.
// Licensed under the GPLv2 License. See LICENSE file in the project root for full license information.

using System;
using System.Collections.Generic;
using System.Threading;
using Machine.Specifications;
using Moq;
using Opc.Ua;
using It = Machine.Specifications.It;

namespace RaaLabs.Edge.Connectors.OPCUA.for_DataReader.when_reading_data_forever;

public class and_there_is_only_subscriptions : given.all_the_reader_dependencies
{
    static DataReader data_reader;
    Establish context = () =>
    {
        data_reader = EstablishReaderWithConfiguration(new()
        {
            ServerUrl = "opc.tcp://localhost:4840",
            PublishIntervalSeconds = 1,
            Nodes =
            [
                new() { NodeId = "ns=1;s=Channel3.Device4.Tag6", SubscribeIntervalSeconds = 1 },
                new() { NodeId = "ns=2;s=Channel2.Device5.Tag5", SubscribeIntervalSeconds = 2 },
            ]
        });

        subscriber_running.SetResult();
        reader_running.SetResult();
    };

    Because of = async () => await data_reader.ReadDataForever(connection, handler, Moq.It.IsAny<CancellationToken>());

    It should_have_invoked_the_subscriber = () => subscriber.Verify(_ => _.SubscribeToChangesFor(connection, TimeSpan.FromSeconds(1), Moq.It.IsAny<IEnumerable<(NodeId, TimeSpan)>>(), handler, Moq.It.IsAny<CancellationToken>()), Times.Once);
    It should_not_have_invoked_the_reader = () => reader.VerifyNoOtherCalls();
    It should_have_subscribed_to_the_correct_nodes = () => subscribed_nodes.ShouldContainOnly(
        new(new NodeId("ns=1;s=Channel3.Device4.Tag6"), TimeSpan.FromSeconds(1)),
        new(new NodeId("ns=2;s=Channel2.Device5.Tag5"), TimeSpan.FromSeconds(2))
    );
}