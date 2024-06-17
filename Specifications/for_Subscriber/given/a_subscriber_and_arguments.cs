using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Machine.Specifications;
using Moq;
using Opc.Ua;
using Opc.Ua.Client;
using Serilog;

namespace RaaLabs.Edge.Connectors.OPCUA.for_Subscriber.given;

public class a_subscriber_and_arguments
{
    protected static Mock<ILogger> logger;
    protected static Subscriber subscriber;

    protected static Mock<ISession> session;
    protected static Func<NodeValue, Task> handler;

    Establish context = () =>
    {
        logger = new();
        subscriber = new(Mock.Of<IMetricsHandler>(), logger.Object);

        session = new();
        session
            .Setup(_ => _.AddSubscription(Moq.It.IsAny<Subscription>()))
            .Callback<Subscription>(_ =>
            {
                typeof(Subscription).GetProperty(nameof(Subscription.Session)).SetValue(_, session.Object);
                last_added_subscription = _;
            })
            .Returns(true);
        session
            .Setup(_ => _.CreateSubscriptionAsync(
                Moq.It.IsAny<RequestHeader>(),
                Moq.It.IsAny<double>(),
                Moq.It.IsAny<uint>(),
                Moq.It.IsAny<uint>(),
                Moq.It.IsAny<uint>(),
                Moq.It.IsAny<bool>(),
                Moq.It.IsAny<byte>(),
                Moq.It.IsAny<CancellationToken>()
            ))
            .ReturnsAsync(new CreateSubscriptionResponse()
            {
                SubscriptionId = 1337,
                RevisedPublishingInterval = 13.37,
                RevisedLifetimeCount = 42,
                RevisedMaxKeepAliveCount = 43,
            });
        session
            .Setup(_ => _.CreateMonitoredItemsAsync(
                Moq.It.IsAny<RequestHeader>(),
                1337,
                Moq.It.IsAny<TimestampsToReturn>(),
                Moq.It.IsAny<MonitoredItemCreateRequestCollection>(),
                Moq.It.IsAny<CancellationToken>()
            ))
            .ReturnsAsync(new CreateMonitoredItemsResponse()
            {
                Results =
                [
                    new(),
                    new(),
                ],
            });

        var values = handled_values = [];
        handler = _ => 
        {
            handled_values.Add(_);
            return Task.CompletedTask;
        };
    };

    protected static Subscription last_added_subscription;
    protected static List<NodeValue> handled_values;
}
