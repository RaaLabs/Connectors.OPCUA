// Copyright (c) RaaLabs. All rights reserved.
// Licensed under the GPLv2 License. See LICENSE file in the project root for full license information.

using System;
using System.Threading;
using System.Threading.Tasks;
using Machine.Specifications;
using Moq;
using Opc.Ua.Client;
using RaaLabs.Edge.Connectors.OPCUA.Events;
using It = Machine.Specifications.It;

namespace RaaLabs.Edge.Connectors.OPCUA.for_Connector.when_running;

public class and_data_is_read : given.a_connector
{
    static NodeValue value_to_recieve;
    static OpcuaDatapointOutput output_to_produce;
    static ISession created_session;
    static Func<NodeValue, Task> send_events_callback;

    Establish context = () =>
    {
        value_to_recieve = new(new(15), new("Hello"));
        output_to_produce = new()
        {
            Source = "testing",
            Tag = "moar tests",
            Value = "yeah"
        };
        created_session = Mock.Of<ISession>();
        
        sessions
            .Setup(_ => _.ConnectToServer(Moq.It.IsAny<CancellationToken>()))
            .ReturnsAsync(created_session);

        var reading = new TaskCompletionSource();
        retriever
            .Setup(_ => _.ReadDataForever(created_session, Moq.It.IsAny<Func<NodeValue, Task>>(), Moq.It.IsAny<CancellationToken>()))
            .Callback<ISession, Func<NodeValue, Task>, CancellationToken>((_, callback, _) => send_events_callback = callback)
            .Returns(reading.Task);

        datapoints
            .Setup(_ => _.CreateDatapointFrom(value_to_recieve))
            .Returns(output_to_produce);
    };

    Because of = async () =>
    {
        _ = connector.Run();
        await Task.Delay(20);
        await send_events_callback(value_to_recieve);
    };

    It should_connect_to_the_server = () => sessions.Verify(_ => _.ConnectToServer(CancellationToken.None), Times.Once);
    It should_read_data_from_the_connection = () => retriever.Verify(_ => _.ReadDataForever(created_session, Moq.It.IsAny<Func<NodeValue, Task>>(), CancellationToken.None), Times.Once);
    It should_convert_data_using_datapoints = () => datapoints.Verify(_ => _.CreateDatapointFrom(value_to_recieve), Times.Once);
    It should_emit_the_converted_data = () => sent_datapoints.ShouldContainOnly(output_to_produce);
}
