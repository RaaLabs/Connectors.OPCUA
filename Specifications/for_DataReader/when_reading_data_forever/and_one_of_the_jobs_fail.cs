// Copyright (c) RaaLabs. All rights reserved.
// Licensed under the GPLv2 License. See LICENSE file in the project root for full license information.

using System;
using System.Threading;
using Machine.Specifications;

namespace RaaLabs.Edge.Connectors.OPCUA.for_DataReader.when_reading_data_forever;

public class and_one_of_the_jobs_fail : given.all_the_reader_dependencies
{
    static DataReader data_reader;
    static Exception thrown_error;

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
        thrown_error = new Exception("oh no :(");
    };

    static Exception caught_error;

    class subscriber_first
    {
        Establish context = () => subscriber_running.SetException(thrown_error);
        Because of = async () => caught_error = await Catch.ExceptionAsync(() => data_reader.ReadDataForever(connection, handler, CancellationToken.None));
        It should_have_cancelled_the_reader = () => reader_cancellation.IsCancellationRequested.ShouldBeTrue();
        It should_throw_the_error = () => caught_error.ShouldBeTheSameAs(thrown_error);
    }

    class reader_first
    {
        Establish context = () => reader_running.SetException(thrown_error);
        Because of = async () => caught_error = await Catch.ExceptionAsync(() => data_reader.ReadDataForever(connection, handler, CancellationToken.None));
        It should_have_cancelled_the_subscriber = () => subscriber_cancellation.IsCancellationRequested.ShouldBeTrue();
        It should_throw_the_error = () => caught_error.ShouldBeTheSameAs(thrown_error);
    }
}