// Copyright (c) RaaLabs. All rights reserved.
// Licensed under the GPLv2 License. See LICENSE file in the project root for full license information.

using System.Threading;
using System.Threading.Tasks;
using Opc.Ua.Client;

namespace RaaLabs.Edge.Connectors.OPCUA;

public interface ICreateSessions
{
    Task<ISession> ConnectToServer(CancellationToken cancellationToken);
}
