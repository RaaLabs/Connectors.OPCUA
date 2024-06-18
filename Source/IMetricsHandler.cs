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

    [Counter(Name = "messages_sent_total", Unit = "count", Description = "The total number of messages sent", Exported = true)]
    public void NumberOfMessagesSent(int value);

    #region DataPointParser.cs
    [Counter(Name = "opcua_bad_statuscode_received", Unit = "count", Description = "The total number of bad status code received from OPCUA server", Exported = true)]
    public void NumberOfBadStatusCodesFor(int value, string nodeId);

    [Counter(Name = "opcua_future_timestamp_received", Unit = "count", Description = "The total number of timestamps from the future received from OPCUA server or source", Exported = true)]
    public void NumberOfFutureTimestampsFor(int value, string type);

    [Counter(Name = "opcua_past_timestamp_received", Unit = "count", Description = "The total number of timestamps from the past received from OPCUA server or source", Exported = true)]
    public void NumberOfOldTimestampsFor(int value, string type);
    #endregion

    #region Client.cs
    [Counter(Name = "opcua_session_connection_attempts_total", Unit = "count", Description = "The total number of connection attempts to the OPCUA server", Exported = true)]
    public void NumberOfSessionConnectionAttempts(long value);

    [Counter(Name = "opcua_session_connections_successful_total", Unit = "count", Description = "The total number of successful connections to the OPCUA server", Exported = true)]
    public void NumberOfSessionConnections(long value);

    [Counter(Name = "opcua_session_connection_time_seconds_total", Unit = "count", Description = "The total time spent waiting for connections to the OPCUA server to open", Exported = true)]
    public void SessionConnectionTime(double value);
    #endregion

    #region Subscriber.cs
    [Counter(Name = "opcua_subscription_attempts_total", Unit = "count", Description = "The total number of subscription attempts to the OPCUA server", Exported = true)]
    public void NumberOfSubscriptionAttempts(long value);

    [Counter(Name = "opcua_subscriptions_total", Unit = "count", Description = "The total number of successful subscriptions to the OPCUA server", Exported = true)]
    public void NumberOfSubscriptions(long value);

    [Counter(Name = "opcua_subscription_setup_time_seconds_total", Unit = "count", Description = "The total time spent setting up subscriptions to the OPCUA server", Exported = true)]
    public void SubscriptionSetupTime(double value);

    [Counter(Name = "opcua_subscription_notifications_received_total", Unit = "count", Description = "The total number of notifications received from the OPCUA server", Exported = true)]
    public void NumberOfReceivedMonitorNotifications(long value);
    #endregion

    #region Reader.cs
    [Counter(Name = "opcua_reader_operations_cancelled", Unit = "count", Description = "The total number of reading operations cancelled from OPCUA server", Exported = true)]
    public void NumberOfReadingsCancelled(long value);

    [Counter(Name = "opcua_readings_started", Unit = "count", Description = "The total number of reading operations started from OPCUA server", Exported = true)]
    public void NumberOfReadingsStarted(long value);
    #endregion
}

[ExcludeFromCodeCoverage]
public class MetricsHandlerState
{
    public string InstanceNumber { get; set; } = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds().ToString("x", CultureInfo.InvariantCulture);
}
