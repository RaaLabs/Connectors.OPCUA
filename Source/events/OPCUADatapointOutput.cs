// Copyright (c) RaaLabs. All rights reserved.
// Licensed under the GPLv2 License. See LICENSE file in the project root for full license information.

using System;
using RaaLabs.Edge.Modules.EdgeHub;


namespace RaaLabs.Edge.Connectors.OPCUA.Events
{
    /// <summary>
    /// The data point on the format it should be sent to EdgeHub.
    /// </summary>
    [OutputName("output")]
    public class OPCUADatapointOutput : IEdgeHubOutgoingEvent
    {
        /// <summary>
        /// Represents the Source system.
        /// </summary>
        public string Source { get; set; }

        /// <summary>
        /// Gets or sets the tag. Represens the sensor name from the source syste, OPC UA node id, consisting of namespace index and identifier, e.g. "ns=3;i=1002".
        /// </summary>
        public string Tag { get; set; }

        /// <summary>
        /// The value of the value.
        /// </summary>
        public dynamic Value { get; set; }

        /// <summary>
        /// Gets or sets the timestamp in the form of EPOCH milliseconds granularity.
        /// </summary>
        public long Timestamp { get; set; }
    }
}