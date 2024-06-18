// Copyright (c) RaaLabs. All rights reserved.
// Licensed under the GPLv2 License. See LICENSE file in the project root for full license information.

using RaaLabs.Edge.Modules.EdgeHub;

namespace RaaLabs.Edge.Connectors.OPCUA.Events;

[OutputName("output")]
public class OpcuaDatapointOutput : IEdgeHubOutgoingEvent
{
    public required string Source { get; init; }
    public required string Tag { get; init; }
    public required dynamic Value { get; init; }
    public long Timestamp { get; init; }
}
