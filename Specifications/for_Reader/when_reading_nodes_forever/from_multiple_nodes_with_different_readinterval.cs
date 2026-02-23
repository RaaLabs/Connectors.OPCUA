// Copyright (c) RaaLabs. All rights reserved.
// Licensed under the GPLv2 License. See LICENSE file in the project root for full license information.

using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Machine.Specifications;
using Opc.Ua;
using Moq;
using It = Machine.Specifications.It;

namespace RaaLabs.Edge.Connectors.OPCUA.for_Reader.when_reading_nodes_forever;

public class from_multiple_nodes_with_different_readinterval : given.a_reader
{
    static IEnumerable<(NodeId node, TimeSpan readInterval)> nodes;
    static List<NodeValue> handled_values;
    
    Establish context = () =>
    {
        nodes = [
            (new NodeId(321), TimeSpan.FromSeconds(1)),
            (new NodeId(231), TimeSpan.FromMilliseconds(500)),
            (new NodeId(111), TimeSpan.FromMilliseconds(200))
        ];

        connection
            .Setup(_ => _.ReadAsync(Moq.It.IsAny<RequestHeader>(), Moq.It.IsAny<double>(), Moq.It.IsAny<TimestampsToReturn>(), Moq.It.Is<ReadValueIdCollection>(c => c.Count == 1 && c[0].NodeId == new NodeId(321)), Moq.It.IsAny<CancellationToken>()))
            .ReturnsAsync(new ReadResponse { Results = new DataValueCollection { new DataValue("value 1") } });
        connection
            .Setup(_ => _.ReadAsync(Moq.It.IsAny<RequestHeader>(), Moq.It.IsAny<double>(), Moq.It.IsAny<TimestampsToReturn>(), Moq.It.Is<ReadValueIdCollection>(c => c.Count == 1 && c[0].NodeId == new NodeId(231)), Moq.It.IsAny<CancellationToken>()))
            .ReturnsAsync(new ReadResponse { Results = new DataValueCollection { new DataValue("value 2") } });
        connection
            .Setup(_ => _.ReadAsync(Moq.It.IsAny<RequestHeader>(), Moq.It.IsAny<double>(), Moq.It.IsAny<TimestampsToReturn>(), Moq.It.Is<ReadValueIdCollection>(c => c.Count == 1 && c[0].NodeId == new NodeId(111)), Moq.It.IsAny<CancellationToken>()))
            .Callback<RequestHeader, double, TimestampsToReturn, ReadValueIdCollection, CancellationToken>((_, __, ___, ____, _____) => cancellation_token_source.Cancel())
            .ReturnsAsync(new ReadResponse { Results = new DataValueCollection { new DataValue("value 3") } });
        
        handled_values = [];
        
        handler = _ => 
        {
            handled_values.Add(_);
            return Task.CompletedTask;
        };
    };

    Because of = async () => await reader.ReadNodesForever(connection.Object, nodes, handler, cancellation_token_source.Token);

    It should_have_read_all_values = () => handled_values.ShouldContainOnly(
        new NodeValue(new (321), new ("value 1")),
        new NodeValue(new (231), new ("value 2")),
        new NodeValue(new (111), new ("value 3"))
    );
}