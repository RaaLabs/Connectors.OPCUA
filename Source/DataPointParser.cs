// Copyright (c) RaaLabs. All rights reserved.
// Licensed under the GPLv2 License. See LICENSE file in the project root for full license information.

using System;
using RaaLabs.Edge.Connectors.OPCUA.Events;
using Serilog;
using StatusCode = Opc.Ua.StatusCode;

namespace RaaLabs.Edge.Connectors.OPCUA;

public class DataPointParser : ICreateDatapointsFromDataValues
{
    private readonly ILogger _logger;
    private readonly IMetricsHandler _metrics;

    private readonly ConnectorConfiguration _configuration;
    private readonly TimeProvider _clock;

    public DataPointParser(TimeProvider clock, ILogger logger, IMetricsHandler metrics, ConnectorConfiguration configuration)
    {
        _clock = clock;
        _logger = logger;
        _metrics = metrics;
        _configuration = configuration;
    }

    public OpcuaDatapointOutput CreateDatapointFrom(NodeValue nodeValue)
    {
        ValidateStatusCodeFor(nodeValue);
        ValidateTimestampFrom(nodeValue);

        return new()
        {
            Source = _configuration.Source ?? "OPCUA",
            Tag = nodeValue.Node.ToString(),
            Value = nodeValue.Value.Value,
            Timestamp = _clock.GetUtcNow().ToUnixTimeMilliseconds()
        };
    }

    private void ValidateStatusCodeFor(NodeValue nodeValue)
    {
        if (StatusCode.IsGood(nodeValue.Value.StatusCode) && StatusCode.IsNotBad(nodeValue.Value.StatusCode)) return;

        _logger.Warning("Bad status code for node {NodeId} - {StatusCode}", nodeValue.Node, nodeValue.Value.StatusCode);
        _metrics.NumberOfBadStatusCodesFor(1, nodeValue.Node.ToString()!);
    }

    private void ValidateTimestampFrom(NodeValue nodeValue)
    {
        var serverTimestamp = ((DateTimeOffset)nodeValue.Value.ServerTimestamp).ToUnixTimeMilliseconds();
        var sourceTimestamp = ((DateTimeOffset)nodeValue.Value.SourceTimestamp).ToUnixTimeMilliseconds();

        var currentUnixTimestamp = _clock.GetUtcNow().ToUnixTimeMilliseconds();

        var diffServerTime = Math.Abs(serverTimestamp - currentUnixTimestamp);
        if (diffServerTime > 600_000)
        {
            _logger.Warning("Timestamp from Server is invalid for node {NodeId} - {DateTime}", nodeValue.Node, nodeValue.Value.ServerTimestamp);
            _metrics.InvalidTimestampReceived(1);
        }

        var diffSourceTime = Math.Abs(sourceTimestamp - currentUnixTimestamp);
        if (diffSourceTime > 600_000)
        {
            _logger.Warning("Timestamp from Source is invalid for node {NodeId} - {DateTime}", nodeValue.Node, nodeValue.Value.SourceTimestamp);
            _metrics.InvalidTimestampReceived(1);
        }
    }
}
