using System;
using System.Threading;
using System.Threading.Tasks;
using Opc.Ua.Client;

namespace RaaLabs.Edge.Connectors.OPCUA;

public interface IRetrieveData
{
    Task ReadDataForever(ISession connection, Func<NodeValue,Task> handleValue, CancellationToken cancellationToken);
}
