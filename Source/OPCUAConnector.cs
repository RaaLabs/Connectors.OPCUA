// Copyright (c) RaaLabs. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System;
using Serilog;
using System.Collections.Generic;
using System.Threading.Tasks;
using RaaLabs.Edge.Modules.EventHandling;
using Opc.Ua;
using Opc.Ua.Client;
using Opc.Ua.Configuration;

namespace RaaLabs.Edge.Connectors.OPCUA
{
    /// <summary>
    /// Represents an implementation for <see cref="IProduceEvent"/>
    /// </summary>
    public class OPCUAConnector : IRunAsync, IProduceEvent<Events.OPCUADatapointInput>
    {
        /// <inheritdoc/>

        public event EventEmitter<Events.OPCUADatapointInput> OPCUAReceived;
        private readonly ILogger _logger;
        private Session session;
        private SessionReconnectHandler reconnectHandler;
        private const int ReconnectPeriod = 10;
        OPCUAClient opcuaClient;

        /// <summary>
        /// Initializes a new instance of <see cref="OPCUAConnector"/>
        /// </summary>
        /// <param name="logger"><see cref="ILogger"/> for logging</param>
        public OPCUAConnector(ILogger logger)
        {
            _logger = logger;

        }

        public async Task Run()
        {
            opcuaClient = new OPCUAClient("Rafaels-MacBook-Pro.local", "53530", true, 1, "2");
        }
    }
}