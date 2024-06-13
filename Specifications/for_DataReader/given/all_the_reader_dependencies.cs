using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Machine.Specifications;
using Moq;
using Opc.Ua;
using Opc.Ua.Client;
using Serilog;

namespace RaaLabs.Edge.Connectors.OPCUA.for_DataReader.given;

public class all_the_reader_dependencies
{
    protected static Mock<ICanSubscribeToNodes> subscriber;
    protected static TaskCompletionSource subscriber_running;
    protected static Mock<ICanReadNodes> reader;
    protected static TaskCompletionSource reader_running;

    protected static ISession connection;
    protected static Func<NodeValue, Task> handler;

    Establish context = () =>
    {
        connection = Mock.Of<ISession>();
        handler = _ => Task.CompletedTask;

        subscriber = new();
        subscriber_running = new();
        subscriber
            .Setup(_ => _.SubscribeToChangesFor(
                connection,
                Moq.It.IsAny<TimeSpan>(),
                Moq.It.IsAny<IEnumerable<(NodeId, TimeSpan)>>(),
                handler,
                Moq.It.IsAny<CancellationToken>()))
            .Callback<ISession, TimeSpan, IEnumerable<(NodeId, TimeSpan)>, Func<NodeValue, Task>, CancellationToken>(
                (_, _, nodes, _, token) =>
                {
                    subscribed_nodes = nodes.ToList();
                    subscriber_cancellation = token;
                    token.Register(() => subscriber_running.TrySetResult());
                })
            .Returns(subscriber_running.Task);

        reader = new();
        reader_running = new();
        reader
            .Setup(_ => _.ReadNodesForever(
                connection,
                Moq.It.IsAny<IEnumerable<(NodeId, TimeSpan)>>(),
                handler,
                Moq.It.IsAny<CancellationToken>()))
            .Callback<ISession, IEnumerable<(NodeId, TimeSpan)>, Func<NodeValue, Task>, CancellationToken>(
                (_, nodes, _, token) =>
                {
                    read_nodes = nodes.ToList();
                    reader_cancellation = token;
                    token.Register(() => reader_running.TrySetResult());
                })
            .Returns(reader_running.Task);
    };

    protected static DataReader EstablishReaderWithConfiguration(ConnectorConfiguration configuration) =>
        new(configuration, subscriber.Object, reader.Object, Mock.Of<ILogger>());

    protected static List<(NodeId, TimeSpan)> subscribed_nodes;
    protected static CancellationToken subscriber_cancellation;
    protected static List<(NodeId, TimeSpan)> read_nodes;
    protected static CancellationToken reader_cancellation;

    Cleanup after = () =>
    {
        subscriber_running.TrySetCanceled();
        reader_running.TrySetCanceled();
    };
}
