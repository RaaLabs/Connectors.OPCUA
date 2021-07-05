// Copyright (c) RaaLabs. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System;
using TechTalk.SpecFlow;
using RaaLabs.Edge.Connectors.OPCUA.Events;

namespace RaaLabs.Edge.Connectors.OPCUA.Specs.Drivers
{
    class IncomingDataPointInstanceFactory : IEventInstanceFactory<OPCUADatapointInput>
    {
        public OPCUADatapointInput FromTableRow(TableRow row)
        {
            var dataPoint = new OPCUADatapointInput
            {
                TimeSeries = Guid.Parse(row["TimeSeries"]),
                Value = float.Parse(row["Value"]),
                Timestamp = long.Parse(row["Timestamp"])
            };

            return dataPoint;
        }
    }
}
