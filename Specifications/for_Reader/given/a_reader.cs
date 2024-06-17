using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Machine.Specifications;
using Moq;
using Serilog;
using ISession = Opc.Ua.Client.ISession;

namespace RaaLabs.Edge.Connectors.OPCUA.for_Reader.given;

public class a_reader
{
    protected static Reader reader;
    protected static Mock<ISession> connection;
    protected static CancellationTokenSource cts;
    protected static List<NodeValue> handled_values;
    protected static Func<NodeValue, Task> handler;
    
    Establish context = () =>
    {
        connection = new();
        reader = new(Mock.Of<ILogger>());
        cts = new();
        
        var values = handled_values = [];
        
        handler = _ => 
        {
            handled_values.Add(_);
            return Task.CompletedTask;
        };
    };

    Cleanup after = () =>
    {
        cts.Dispose();
    };
}