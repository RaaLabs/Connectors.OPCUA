// Copyright (c) RaaLabs. All rights reserved.
// Licensed under the GPLv2 License. See LICENSE file in the project root for full license information.

using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Machine.Specifications;
using Opc.Ua;

namespace RaaLabs.Edge.Connectors.OPCUA.for_Reader.when_reading_nodes_forever;

public class and_datavalue_found : given.a_reader
{
    static IEnumerable<(NodeId node, TimeSpan readInterval)> nodes;
    static Task forever_reading_task;
    
    Establish context = () =>
    {
        nodes = new[]
        {
            (new NodeId(1), TimeSpan.FromSeconds(1))
        };
        connection
            .Setup(_ => _.ReadValueAsync(new NodeId(1), cts.Token))
            .Returns(Task.FromResult(new DataValue(("reading value"))));
    };

    Because of = () =>
    {
        forever_reading_task = reader.ReadNodesForever(connection.Object, nodes, handler, cts.Token);
    };

    It should_have_read_the_values = () => handled_values.ShouldContainOnly(new NodeValue(new (1), new ("reading value")));
}