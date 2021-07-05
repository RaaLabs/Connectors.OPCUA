// Copyright (c) RaaLabs. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System;
using RaaLabs.Edge.Modules.EdgeHub;


namespace RaaLabs.Edge.Connectors.OPCUA.Events
{
    /// <summary>
    /// The data point received. Could be a different format than what is specified in this
    /// sample. This is an example for a module that is placed after IdentityMapper
    /// </summary>
    [InputName("events")]
    public class OPCUADatapointInput : IEdgeHubIncomingEvent
    {
        /// <summary>
        /// The time series id
        /// </summary>
        public Guid TimeSeries { get; set; }

        /// <summary>
        /// The value of the data point
        /// </summary>
        public dynamic Value { get; set; }

        /// <summary>
        /// The timestamp, represented as epoc time with milliseconds
        /// </summary>
        public long Timestamp { get; set; }
    }
}