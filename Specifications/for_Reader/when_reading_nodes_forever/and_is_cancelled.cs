using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Machine.Specifications;
using Opc.Ua;

namespace RaaLabs.Edge.Connectors.OPCUA.for_Reader.when_reading_nodes_forever;

public class and_is_cancelled : given.a_reader
{
    static Task forever_reading_task;
    static CancellationTokenSource cts;
    static List<NodeValue> handled_values;
    Establish context = () =>
    {
        cts = new();
        forever_reading_task = reader.ReadNodesForever(
            connection.Object, 
            new []{(new NodeId(123), TimeSpan.FromSeconds(1))}, 
            handler, 
            cts.Token);
        connection
            .Setup(_ => _.ReadValueAsync(new NodeId(123), cts.Token))
            .Returns( Task.FromResult(new DataValue(new Variant("data value"))));
        
        var values = handled_values = [];
        
        handler = _ => 
        {
            handled_values.Add(_);
            return Task.CompletedTask;
        };
    };

    static Exception error;
    Because of = async () => error = await Catch.ExceptionAsync(async () =>
    {
        cts.CancelAfter(123);
        await forever_reading_task;
    });
    
    It should_not_emit_anything = () => handled_values.ShouldBeEmpty();
    It should_peacfully_cancel = () => error.ShouldBeOfExactType<OperationCanceledException>();
    
    Cleanup after = () => cts.Dispose();
}