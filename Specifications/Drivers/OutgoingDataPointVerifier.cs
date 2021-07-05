// Copyright (c) RaaLabs. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System;
using TechTalk.SpecFlow;
using FluentAssertions;
using System.Globalization;
using RaaLabs.Edge.Connectors.OPCUA.Events;

namespace RaaLabs.Edge.Connectors.OPCUA.Specs.Drivers
{
    class OutgoingDataPointVerifier : IProducedEventVerifier<OPCUADatapointOutput>
    {
        public void VerifyFromTableRow(OPCUADatapointOutput @event, TableRow row)
        {
            double actualValue = @event.Value;
            var expectedValue = float.Parse(row["Value"], CultureInfo.InvariantCulture.NumberFormat);
            @event.TimeSeries.Should().Be(Guid.Parse(row["TimeSeries"]));
            @event.Timestamp.Should().Be(long.Parse(row["Timestamp"]));
            actualValue.Should().BeApproximately(expectedValue, 0.0001f);
        }
    }
}

