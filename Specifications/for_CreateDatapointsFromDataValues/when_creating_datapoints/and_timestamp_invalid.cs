using System;
using Machine.Specifications;
using Moq;
using Opc.Ua;
using RaaLabs.Edge.Connectors.OPCUA.Events;
using Serilog;
using It = Machine.Specifications.It;

namespace RaaLabs.Edge.Connectors.OPCUA.for_CreateDatapointsFromDataValues.when_creating_datapoints;

public class and_timestamp_invalid : given.a_creator
{
    static NodeValue nodevalue;

    Establish context = () =>
    {
        clock
            .Setup(_ => _.GetUtcNow())
            .Returns(new DateTimeOffset(new DateTime(2024, 1, 1, 1, 1, 1, DateTimeKind.Utc)));
        
        nodevalue = new NodeValue(new NodeId("ns=1;i=1"), new (){Value = 2, StatusCode = StatusCodes.Good, SourceTimestamp = new DateTime(2023, 1, 1, 1, 1, 1, DateTimeKind.Utc), ServerTimestamp = new DateTime(2024, 1, 1, 1, 1, 1, DateTimeKind.Utc)});
    };

    static OpcuaDatapointOutput result;
    Because of = () => result = parser.CreateDatapointFrom(nodevalue);

    It should_log_warning = () => logger.Verify(_ => _.Warning("Timestamp from Source is invalid for node {NodeId} - {DateTime}", new NodeId("ns=1;i=1"), new DateTime(2023, 1, 1, 1, 1, 1, DateTimeKind.Utc)));
    It should_have_correct_source = () => result.Source.ShouldEqual("OPCUA");
    It should_have_correct_tag = () => result.Tag.ShouldEqual("ns=1;i=1");
    It should_have_correct_value = () => result.Value.Equals(2);
    It should_have_correct_timestamp = () => result.Timestamp.ShouldEqual(1704070861000);
}