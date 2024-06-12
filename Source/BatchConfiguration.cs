// Copyright (c) RaaLabs. All rights reserved.
// Licensed under the GPLv2 License. See LICENSE file in the project root for full license information.

using System;
using System.Globalization;
using RaaLabs.Edge.Modules.EdgeHub;

namespace RaaLabs.Edge.Connectors.OPCUA;

public class BatchConfiguration : IEdgeHubOutgoingEventBatchConfiguration
{
    public int BatchSize { get; set; } = int.Parse(Environment.GetEnvironmentVariable("EDGEHUB_BATCH_SIZE") ?? "250", CultureInfo.CurrentCulture);
    public int Interval { get; set; } = int.Parse(Environment.GetEnvironmentVariable("EDGEHUB_BATCH_INTERVAL") ?? "5000", CultureInfo.CurrentCulture);
}
