using System.Threading;
using Machine.Specifications;

namespace RaaLabs.Edge.Connectors.OPCUA.for_DataReader.when_reading_data_forever;

public class and_one_of_the_jobs_complete : given.all_the_reader_dependencies
{
    static DataReader data_reader;
    Establish context = () =>
    {
        data_reader = EstablishReaderWithConfiguration(new()
        {
            ServerUrl = "opc.tcp://localhost:4840",
            PublishIntervalSeconds = 1,
            Nodes =
            [
                new() { NodeId = "ns=1;s=Channel3.Device4.Tag6", SubscribeIntervalSeconds = 1, ReadIntervalSeconds = 1 },
                new() { NodeId = "ns=2;s=Channel2.Device5.Tag5", SubscribeIntervalSeconds = 2 },
                new() { NodeId = "ns=3;s=Channel1.Device6.Tag4", ReadIntervalSeconds = 3 },
            ]
        });
    };

    class subscriber_first
    {
        Establish context = () => subscriber_running.SetResult();
        Because of = async () => await data_reader.ReadDataForever(connection, handler, CancellationToken.None);
        It should_have_cancelled_the_reader = () => reader_cancellation.IsCancellationRequested.ShouldBeTrue();
    }

    class reader_first
    {
        Establish context = () => reader_running.SetResult();
        Because of = async () => await data_reader.ReadDataForever(connection, handler, CancellationToken.None);
        It should_have_cancelled_the_subscriber = () => subscriber_cancellation.IsCancellationRequested.ShouldBeTrue();
    }
}