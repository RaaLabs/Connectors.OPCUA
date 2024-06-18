// Copyright (c) RaaLabs. All rights reserved.
// Licensed under the GPLv2 License. See LICENSE file in the project root for full license information.

using System;
using RaaLabs.Edge.Connectors.OPCUA.Events;
using Serilog;
using StatusCode = Opc.Ua.StatusCode;

namespace RaaLabs.Edge.Connectors.OPCUA;

public class DataPointParser : ICreateDatapointsFromDataValues
{
    private readonly ConnectorConfiguration _configuration;
    private readonly ILogger _logger;
    private readonly IMetricsHandler _metrics;
    private readonly TimeProvider _clock;

    public DataPointParser(ConnectorConfiguration configuration, ILogger logger, IMetricsHandler metrics, TimeProvider clock)
    {
        _configuration = configuration;
        _logger = logger;
        _metrics = metrics;
        _clock = clock;
    }

    public OpcuaDatapointOutput CreateDatapointFrom(NodeValue nodeValue)
    {
        LogWarningIfStatusCodeIsNotGood(nodeValue);
        LogWarningIfFaultyTimestamp(nodeValue, nodeValue.Value.ServerTimestamp, "Server");
        LogWarningIfFaultyTimestamp(nodeValue, nodeValue.Value.SourceTimestamp, "Source");

        return new()
        {
            Source = _configuration.Source ?? "OPCUA",
            Tag = nodeValue.Node.ToString(),
            Value = nodeValue.Value.Value,
            Timestamp = _clock.GetUtcNow().ToUnixTimeMilliseconds()
        };
    }

    private void LogWarningIfStatusCodeIsNotGood(NodeValue nodeValue)
    {
        if (StatusCode.IsGood(nodeValue.Value.StatusCode) && StatusCode.IsNotBad(nodeValue.Value.StatusCode)) return;

        _logger.Warning("Bad status code for node {NodeId} - {StatusCode}", nodeValue.Node, nodeValue.Value.StatusCode);
        _metrics.NumberOfBadStatusCodesFor(1, nodeValue.Node.ToString()!);
    }

    private void LogWarningIfFaultyTimestamp(NodeValue nodeValue, DateTime timestamp, string timestampType)
    {
        var dateTimeOffset = (DateTimeOffset) timestamp;
        var utcNow = _clock.GetUtcNow();

        if (dateTimeOffset > utcNow + TimeSpan.FromMinutes(15))
        {
            _logger.Warning("Timestamp more than 15 minutes the future for node {NodeValueNode} - {Timestamp}. Timestamp from {Source} is never used as property for OpcuaDatapointOutput", nodeValue.Node, timestamp, timestampType);
            _metrics.NumberOfFutureTimestampsFor(1, nodeValue.Node.ToString());
        }
        else if (dateTimeOffset < utcNow - TimeSpan.FromMinutes(15))
        {
            _logger.Warning("Timestamp older than  15 minutes for node {NodeValueNode} - {Timestamp}. Timestamp from {Source} is never used as property for OpcuaDatapointOutput", nodeValue.Node, timestamp, timestampType);
            _metrics.NumberOfOldTimestampsFor(1, nodeValue.Node.ToString());
        }
    }
}
