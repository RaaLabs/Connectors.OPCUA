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

public class and_there_is_only_polling : given.all_the_reader_dependencies
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
                new() { NodeId = "ns=1;s=Channel3.Device4.Tag6", ReadIntervalSeconds = 1 },
                new() { NodeId = "ns=3;s=Channel1.Device6.Tag4", ReadIntervalSeconds = 3 },
            ]
        });

        subscriber_running.SetResult();
        reader_running.SetResult();
    };

    Because of = async () => await data_reader.ReadDataForever(connection, handler, Moq.It.IsAny<CancellationToken>());

    It should_not_have_invoked_the_subscriber = () => subscriber.VerifyNoOtherCalls();
    It should_have_invoked_the_reader = () => reader.Verify(_ => _.ReadNodesForever(connection, Moq.It.IsAny<IEnumerable<(NodeId, TimeSpan)>>(), handler, Moq.It.IsAny<CancellationToken>()), Times.Once);
    It should_have_read_the_correct_nodes = () => read_nodes.ShouldContainOnly(
        new(new NodeId("ns=1;s=Channel3.Device4.Tag6"), TimeSpan.FromSeconds(1)),
        new(new NodeId("ns=3;s=Channel1.Device6.Tag4"), TimeSpan.FromSeconds(3))
    );
}