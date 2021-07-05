// Copyright (c) RaaLabs. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System;
using TechTalk.SpecFlow;

namespace RaaLabs.Edge.ModuleTemplate.Specs.Drivers
{
    class IncomingDataPointInstanceFactory : IEventInstanceFactory<Events.ModuleTemplateDatapointInput>
    {
        public Events.ModuleTemplateDatapointInput FromTableRow(TableRow row)
        {
            var dataPoint = new Events.ModuleTemplateDatapointInput
            {
                TimeSeries = Guid.Parse(row["TimeSeries"]),
                Value = float.Parse(row["Value"]),
                Timestamp = long.Parse(row["Timestamp"])
            };

            return dataPoint;
        }
    }
}
