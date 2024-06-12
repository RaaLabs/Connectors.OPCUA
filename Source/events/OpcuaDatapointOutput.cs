// Copyright (c) RaaLabs. All rights reserved.
// Licensed under the GPLv2 License. See LICENSE file in the project root for full license information.

using RaaLabs.Edge.Modules.EdgeHub;


namespace RaaLabs.Edge.Connectors.OPCUA.Events;

/// <summary>
/// The data point on the format it should be sent to EdgeHub.
/// </summary>
[OutputName("output")]
public class OpcuaDatapointOutput : IEdgeHubOutgoingEvent
{
    /// <summary>
    /// Represents the Source system.
    /// </summary>
    public required string Source { get; init; }

    /// <summary>
    /// Gets or sets the tag. Represents the sensor name from the source system, OPC UA node id, consisting of namespace index and identifier, e.g. "ns=3;i=1002".
    /// </summary>
    public required string Tag { get; init; }

    /// <summary>
    /// The value of the sensor reading.
    /// </summary>
    public required dynamic Value { get; init; }

    /// <summary>
    /// Gets or sets the timestamp in the form of EPOCH milliseconds granularity.
    /// </summary>
    public long Timestamp { get; init; }
}
