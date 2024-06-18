// Copyright (c) RaaLabs. All rights reserved.
// Licensed under the GPLv2 License. See LICENSE file in the project root for full license information.

using System;
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
    protected static Func<NodeValue, Task> handler;

    protected static CancellationTokenSource cancellation_token_source;
    
    Establish context = () =>
    {
        connection = new();
        reader = new(Mock.Of<ILogger>(), Mock.Of<IMetricsHandler>());
        cancellation_token_source = new();
    };

    Cleanup after = () =>
    {
        cancellation_token_source.Dispose();
    };
}