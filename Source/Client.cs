using System.Threading;
using System.Threading.Tasks;
using Opc.Ua.Client;
using Serilog;

namespace RaaLabs.Edge.Connectors.OPCUA;

public class Client : ICreateSessions
{
    private readonly ILogger _logger;

    public Client(ConnectorConfiguration config, ILogger logger)
    {
        _logger = logger;
    }

    public async Task<Session> ConnectToServer(CancellationToken cancellationToken)
    {
    }
}
