// Copyright (c) RaaLabs. All rights reserved.
// Licensed under the GPLv2 License. See LICENSE file in the project root for full license information.

using System;
using System.Collections.Generic;
using System.Threading;
using Machine.Specifications;
using Moq;
using Opc.Ua;
using It = Machine.Specifications.It;

namespace RaaLabs.Edge.Connectors.OPCUA.for_Reader.when_reading_nodes_forever;

public class and_exception_thrown_from_handlevalue : given.a_reader
{
    static IEnumerable<(NodeId node, TimeSpan readInterval)> nodes;
    static CancellationToken ct;
    
    Establish context = () =>
    {
        nodes = [
            (new NodeId(321), TimeSpan.FromSeconds(1)),
            (new NodeId(111), TimeSpan.FromSeconds(1))
        ];

        connection
            .Setup(_ => _.ReadAsync(Moq.It.IsAny<RequestHeader>(), Moq.It.IsAny<double>(), Moq.It.IsAny<TimestampsToReturn>(), Moq.It.Is<ReadValueIdCollection>(c => c.Count == 1 && c[0].NodeId == new NodeId(321)), Moq.It.IsAny<CancellationToken>()))
            .Callback<RequestHeader, double, TimestampsToReturn, ReadValueIdCollection, CancellationToken>((_, __, ___, ____, cancellation_token) => ct = cancellation_token)
            .ReturnsAsync(new ReadResponse { Results = new DataValueCollection { new DataValue() } });

        handler = _ => throw new Exception("this is an exception");
    };

    static Exception exception;
    Because of = async () => exception = await Catch.ExceptionAsync(() => reader.ReadNodesForever(connection.Object, nodes, handler, cancellation_token_source.Token));
    
    It should_have_thrown_an_exception = () => exception.ShouldBeOfExactType<Exception>();
    It should_have_set_the_cancellation_token = () => ct.IsCancellationRequested.ShouldBeTrue();
}