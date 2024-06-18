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

public class and_datavalue_received_then_cancelled : given.a_reader
{
    static IEnumerable<(NodeId node, TimeSpan readInterval)> nodes;
    static List<NodeValue> handled_values;
    
    Establish context = async () =>
    {
        nodes = [(new NodeId(321), TimeSpan.FromSeconds(1))];

        connection
            .Setup(_ => _.ReadValueAsync(new NodeId(321), Moq.It.IsAny<CancellationToken>()))
            .Callback(() => cancellation_token_source.Cancel())
            .ReturnsAsync(new DataValue("reading value"));
        
        handled_values = [];
        
        handler = _ => 
        {
            handled_values.Add(_);
            return Task.CompletedTask;
        };
    };

    Because of = async () => await reader.ReadNodesForever(connection.Object, nodes, handler, cancellation_token_source.Token);

    It should_have_read_the_values = () => handled_values.ShouldContainOnly(new NodeValue(new (321), new ("reading value")));
}