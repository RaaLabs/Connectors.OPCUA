using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Machine.Specifications;
using Opc.Ua;

namespace RaaLabs.Edge.Connectors.OPCUA.for_Subscriber.when_subscribing_to_changes;

public class and_then_cancelled : given.a_subscriber_and_arguments
{
    static CancellationTokenSource cancellation;
    static Task running_subscriber;
    Establish context = () =>
    {
        cancellation = new();
        running_subscriber = subscriber.SubscribeToChangesFor(
            session.Object,
            TimeSpan.FromHours(1),
            [
                (new NodeId(13), TimeSpan.FromSeconds(2)),
                (new NodeId(14), TimeSpan.FromSeconds(3))
            ],
            handler,
            cancellation.Token
        );
    };

    Because of = async () =>
    {
        cancellation.Cancel();
        await running_subscriber;
    };

    It should_delete_the_subscription = () => session.Verify(_ => _.DeleteSubscriptions(
        Moq.It.IsAny<RequestHeader>(),
        Moq.It.Is<UInt32Collection>(_ => _.Count == 1 && _[0] == 1337),
        out Moq.It.Ref<StatusCodeCollection>.IsAny,
        out Moq.It.Ref<DiagnosticInfoCollection>.IsAny
    ));
}
