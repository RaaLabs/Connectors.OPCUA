// Copyright (c) RaaLabs. All rights reserved.
// Licensed under the GPLv2 License. See LICENSE file in the project root for full license information.

using System;
using System.Threading;
using System.Threading.Tasks;
using RaaLabs.Edge.Connectors.OPCUA.Events;
using RaaLabs.Edge.Modules.EventHandling;
using Serilog;

namespace RaaLabs.Edge.Connectors.OPCUA;

public class Connector : IRunAsync, IProduceEvent<OpcuaDatapointOutput>
{
    private readonly ICreateSessions _sessions;
    private readonly IRetrieveData _retriever;
    private readonly ICreateDatapointsFromDataValues _datapoints;
    private readonly ILogger _logger;

    public Connector(ICreateSessions sessions, IRetrieveData retriever, ICreateDatapointsFromDataValues datapoints, ILogger logger)
    {
        _sessions = sessions;
        _retriever = retriever;
        _datapoints = datapoints;
        _logger = logger;
    }

    public event AsyncEventEmitter<OpcuaDatapointOutput>? SendDatapoint;

    public async Task Run()
    {
        while (true)
        {
            try
            {
                _logger.Information("Initiating connection to server");
                var connection = await _sessions.ConnectToServer(CancellationToken.None).ConfigureAwait(false);

                _logger.Information("Starting data reader");
                await _retriever.ReadDataForever(connection, ConvertAndSendDataValue, CancellationToken.None).ConfigureAwait(false);

                _logger.Warning("Data reader stopped");
            }
            catch (Exception error)
            {
                _logger.Error(error, "Failure occured while connecting or reading data");
                await Task.Delay(TimeSpan.FromSeconds(1)).ConfigureAwait(false);
            }
        }
    }

    private Task ConvertAndSendDataValue(NodeValue value) =>
        SendDatapoint!(_datapoints.CreateDatapointFrom(value));
}
