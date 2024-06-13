// Copyright (c) RaaLabs. All rights reserved.
// Licensed under the GPLv2 License. See LICENSE file in the project root for full license information.

using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using RaaLabs.Edge.Modules.Configuration;

namespace RaaLabs.Edge.Connectors.OPCUA;

[Name("configuration.json"), RestartOnChange, ExcludeFromCodeCoverage]
public class ConnectorConfiguration : IConfiguration
{
    public required string ServerUrl { get; init; }
    public double PublishIntervalSeconds { get; init; } = 1.0;
    public IList<NodeConfiguration> Nodes { get; init; } = [];
    public string? Source { get; set; }
}

public class NodeConfiguration
{
    public required string NodeId { get; init; }
    public double? SubscribeIntervalSeconds { get; init; }
    public double? ReadIntervalSeconds { get; init; }
}
