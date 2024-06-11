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
    private readonly ILogger _logger;

    public Connector(ICreateSessions sessions, IRetrieveData retriever, ILogger logger)
    {
        _sessions = sessions;
        _retriever = retriever;
        _logger = logger;
    }

    public event EventEmitter<OpcuaDatapointOutput>? SendDatapoint;

    public async Task Run()
    {
        while (true)
        {
            try
            {
                _logger.Information("Initiating connection to server");
                var connection = await _sessions.ConnectToServer(CancellationToken.None).ConfigureAwait(false);

                _logger.Information("Starting data reader");
                await _retriever.ReadDataForever(connection, SendDatapoint!, CancellationToken.None).ConfigureAwait(false);

                _logger.Warning("Data reader stopped");
            }
            catch (Exception error)
            {
                _logger.Error(error, "Failure occured while connecting or reading data");
                await Task.Delay(TimeSpan.FromSeconds(1)).ConfigureAwait(false);
            }
        }
    }
}
