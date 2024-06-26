// Copyright (c) RaaLabs. All rights reserved.
// Licensed under the GPLv2 License. See LICENSE file in the project root for full license information.

using System;
using Machine.Specifications;
using Moq;
using Opc.Ua;
using RaaLabs.Edge.Connectors.OPCUA.Events;
using It = Machine.Specifications.It;

namespace RaaLabs.Edge.Connectors.OPCUA.for_DataPointParser.when_creating_datapoints;

public class and_parsing_successful : given.a_creator
{
    static NodeValue nodevalue;

    Establish context = () =>
    {
        clock
            .Setup(_ => _.GetUtcNow())
            .Returns(new DateTimeOffset(new DateTime(2024, 1, 1, 1, 1, 1, DateTimeKind.Utc)));
        
        nodevalue = new NodeValue("ns=1;i=1", new (){Value = 2, StatusCode = StatusCodes.Good, SourceTimestamp = new DateTime(2024, 1, 1, 1, 1, 1, DateTimeKind.Utc), ServerTimestamp = new DateTime(2024, 1, 1, 1, 1, 1, DateTimeKind.Utc)});
    };

    static OpcuaDatapointOutput result;
    Because of = () => result = parser.CreateDatapointFrom(nodevalue);

    It should_not_log_anything = () => logger.Verify(_ => _.Warning(Moq.It.IsAny<string>()), Times.Never);
    It should_have_correct_source = () => result.Source.ShouldEqual("OPCUA");
    It should_have_correct_tag = () => result.Tag.ShouldEqual("ns=1;i=1");
    It should_have_correct_value = () => result.Value.Equals(2);
    It should_have_correct_timestamp = () => result.Timestamp.ShouldEqual(1704070861000);
}