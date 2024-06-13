// Copyright (c) RaaLabs. All rights reserved.
// Licensed under the GPLv2 License. See LICENSE file in the project root for full license information.

using System;
using RaaLabs.Edge.Connectors.OPCUA.Events;
using Serilog;
using StatusCode = Opc.Ua.StatusCode;

namespace RaaLabs.Edge.Connectors.OPCUA;

public class CreateDatapointsFromDataValues : ICreateDatapointsFromDataValues
{
    private readonly ILogger _logger;
    private readonly IMetricsHandler _metrics;

    private readonly ConnectorConfiguration _configuration;
    private readonly TimeProvider _clock;

    public CreateDatapointsFromDataValues(TimeProvider clock, ILogger logger, IMetricsHandler metrics, ConnectorConfiguration configuration)
    {
        _clock = clock;
        _logger = logger;
        _metrics = metrics;
        _configuration = configuration;
    }

    public OpcuaDatapointOutput CreateDatapointFrom(NodeValue nodeValue)
    {
        if (!StatusCode.IsGood(nodeValue.Value.StatusCode) || !StatusCode.IsNotBad(nodeValue.Value.StatusCode))
        {
            _logger.Warning("Bad status code for node {NodeId}", nodeValue.Node.Identifier);
            _metrics.NumberOfBadStatusCodesFor(1, nodeValue.Node.Identifier.ToString()!);
        }

        if (!ValidateTimestamps(nodeValue, nodeValue.Value.ServerTimestamp, "ServerTimestamp") && !ValidateTimestamps(nodeValue, nodeValue.Value.SourceTimestamp, "SourceTimestamp"))
        {
        }

        return new()
        {
            Source = _configuration.Source ?? "OPCUA",
            Tag = nodeValue.Node.Identifier.ToString()!,
            Value = nodeValue.Value.Value,
            Timestamp = _clock.GetUtcNow().ToUnixTimeMilliseconds()
        };
    }

    private bool ValidateTimestamps(NodeValue nodeValue, DateTime timestamp, string timestampType)
    {
        var unixTimestamp = ((DateTimeOffset)timestamp).ToUnixTimeMilliseconds();
        var currentUnixTimestamp = _clock.GetUtcNow().ToUnixTimeMilliseconds();

        if (unixTimestamp > currentUnixTimestamp + 60_000)
        {
            _logger.Warning("Timestamp more than 60 seconds the future for node {NodeId} - {TimestampType}", nodeValue.Node.Identifier, timestampType);
            _metrics.NumberOfFutureTimestampsFor(1);
            return false;
        }

        if (unixTimestamp >= currentUnixTimestamp - 60_000) return true;
        _logger.Warning("Timestamp older than 60 seconds for node {NodeId} - {TimestampType}", nodeValue.Node.Identifier, timestampType);
        _metrics.NumberOfOldTimestampsFor(1);

        return false;
    }
}
