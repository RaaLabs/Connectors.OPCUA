// Copyright (c) RaaLabs. All rights reserved.
// Licensed under the GPLv2 License. See LICENSE file in the project root for full license information.

using System;
using System.Globalization;
using System.Diagnostics.CodeAnalysis;
using RaaLabs.Edge.Modules.Diagnostics.Metrics;
using RaaLabs.Edge.Modules.EventHandling.RequestHandling;


namespace RaaLabs.Edge.Connectors.OPCUA;

[Metrics(Prefix = "raaedge"), Labels("iothub", "{IOTEDGE_IOTHUBHOSTNAME:env}", "edge_device", "{IOTEDGE_DEVICEID:env}", "edge_module", "{IOTEDGE_MODULEID:env}", "module", "{IOTEDGE_MODULEID:env}", "instance_number", "{InstanceNumber}")]
public interface IMetricsHandler : IMetricsClient, IWithStateFrom<MetricsHandlerState>
{

    [Counter(Name = "messages_received_total", Unit = "count", Description = "The total number of messages received", Exported = true)]
    public void NumberOfMessagesReceived(long value);

    [Counter(Name = "messages_sent_total", Unit = "count", Description = "The total number of messages sent", Exported = true)]
    public void NumberOfMessagesSent(long value);



    [Counter(Name = "opcua_session_connection_attempts_total", Unit = "count", Description = "The total number of connection attempts to the OPCUA server", Exported = true)]
    public void NumberOfSessionConnectionAttempts(long value);

    [Counter(Name = "opcua_session_connections_successful_total", Unit = "count", Description = "The total number of successful connections to the OPCUA server", Exported = true)]
    public void NumberOfSessionConnections(long value);

    [Counter(Name = "opcua_session_connection_time_seconds_total", Unit = "count", Description = "The total time spent waiting for connections to the OPCUA server to open", Exported = true)]
    public void SessionConnectionTime(double value);
}

[ExcludeFromCodeCoverage]
public class MetricsHandlerState
{
    public string InstanceNumber { get; set; } = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds().ToString("x", CultureInfo.InvariantCulture);
}
