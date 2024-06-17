// Copyright (c) RaaLabs. All rights reserved.
// Licensed under the GPLv2 License. See LICENSE file in the project root for full license information.

using System;
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
    
    Establish context = () =>
    {
        connection = new();
        reader = new(Mock.Of<ILogger>());
    };
}