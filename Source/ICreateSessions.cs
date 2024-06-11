using System.Threading;
using System.Threading.Tasks;
using Opc.Ua.Client;

namespace RaaLabs.Edge.Connectors.OPCUA;

public interface ICreateSessions
{
    Task<Session> ConnectToServer(CancellationToken cancellationToken);
}
