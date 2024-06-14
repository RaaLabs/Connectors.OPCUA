using System;
using Machine.Specifications;
using Opc.Ua;
using RaaLabs.Edge.Connectors.OPCUA.Events;

namespace RaaLabs.Edge.Connectors.OPCUA.for_CreateDatapointsFromDataValues.when_creating_datapoints;

public class and_statuscode_not_good : given.a_creator
{
    static NodeValue nodevalue;

    Establish context = () =>
    {
        clock
            .Setup(_ => _.GetUtcNow())
            .Returns(new DateTimeOffset(new DateTime(2024, 1, 1, 1, 1, 1, DateTimeKind.Utc)));
        
        nodevalue = new NodeValue(new NodeId("ns=1;i=1"), new (){Value = 2, StatusCode = StatusCodes.BadNodeIdInvalid, SourceTimestamp = new DateTime(2024, 1, 1, 1, 1, 1, DateTimeKind.Utc), ServerTimestamp = new DateTime(2024, 1, 1, 1, 1, 1, DateTimeKind.Utc)});
    };

    static OpcuaDatapointOutput result;
    Because of = () => result = parser.CreateDatapointFrom(nodevalue);

    It should_log_warning = () => logger.Verify(_ => _.Warning("Bad status code for node {NodeId} - {StatusCode}", new NodeId("ns=1;i=1"), nodevalue.Value.StatusCode));
    It should_have_correct_source = () => result.Source.ShouldEqual("OPCUA");
    It should_have_correct_tag = () => result.Tag.ShouldEqual("ns=1;i=1");
    It should_have_correct_value = () => result.Value.Equals(2);
    It should_have_correct_timestamp = () => result.Timestamp.ShouldEqual(1704070861000);
}