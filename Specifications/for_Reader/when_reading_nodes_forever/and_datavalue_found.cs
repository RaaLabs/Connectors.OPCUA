// Copyright (c) RaaLabs. All rights reserved.
// Licensed under the GPLv2 License. See LICENSE file in the project root for full license information.

using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Machine.Specifications;
using Opc.Ua;

namespace RaaLabs.Edge.Connectors.OPCUA.for_Reader.when_reading_nodes_forever;

public class and_datavalue_found : given.a_reader
{
    static IEnumerable<(NodeId node, TimeSpan readInterval)> nodes;
    static List<NodeValue> handled_values;
    
    Establish context = () =>
    {
        nodes = new[]
        {
            (new NodeId(1), TimeSpan.FromSeconds(1))
        };
        
        connection
            .Setup(_ => _.ReadValueAsync(new NodeId(1), Moq.It.IsAny<CancellationToken>()))
            .Returns(Task.FromResult(new DataValue("reading value")));
        
        var values = handled_values = [];
        
        handler = _ => 
        {
            handled_values.Add(_);
            return Task.CompletedTask;
        };
    };

    Because of = () =>
    {
        _ = reader.ReadNodesForever(connection.Object, nodes, handler, Moq.It.IsAny<CancellationToken>());
    };

    It should_have_read_the_values = () => handled_values.ShouldContainOnly(new NodeValue(new (1), new ("reading value")));
}