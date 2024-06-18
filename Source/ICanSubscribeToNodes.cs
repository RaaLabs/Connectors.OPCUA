// Copyright (c) RaaLabs. All rights reserved.
// Licensed under the GPLv2 License. See LICENSE file in the project root for full license information.

using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Opc.Ua;
using Opc.Ua.Client;

namespace RaaLabs.Edge.Connectors.OPCUA;

public interface ICanSubscribeToNodes
{
    Task SubscribeToChangesFor(ISession connection, TimeSpan publishInterval, IEnumerable<(NodeId node, TimeSpan samplingInterval)> nodes, Func<NodeValue,Task> handleValue, CancellationToken cancellationToken);
}
