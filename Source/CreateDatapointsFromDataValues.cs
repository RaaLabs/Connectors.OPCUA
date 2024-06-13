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

        if (TimestampIsInvalid(nodeValue, nodeValue.Value.ServerTimestamp, "Server") && TimestampIsInvalid(nodeValue, nodeValue.Value.SourceTimestamp, "Source"))
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

    private bool TimestampIsInvalid(NodeValue nodeValue, DateTime datetime, string timestampOrigin)
    {
        var timestamp = ((DateTimeOffset)datetime).ToUnixTimeMilliseconds();
        var currentUnixTimestamp = _clock.GetUtcNow().ToUnixTimeMilliseconds();

        if (timestamp <= currentUnixTimestamp + 60_000 && timestamp >= currentUnixTimestamp - 60_000) return false;

        _logger.Warning("Timestamp from {TimestampOrigin} is invalid for node {NodeId} - {DateTime}", timestampOrigin, nodeValue.Node.Identifier, datetime);
        _metrics.InvalidTimestampReceived(1);
        return true;

    }
}
