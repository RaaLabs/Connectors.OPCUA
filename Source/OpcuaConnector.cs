// Copyright (c) RaaLabs. All rights reserved.
// Licensed under the GPLv2 License. See LICENSE file in the project root for full license information.

using System;
using Serilog;
using System.Threading.Tasks;
using System.Collections.Generic;
using Polly;
using Opc.Ua;
using Opc.Ua.Configuration;
using RaaLabs.Edge.Modules.EventHandling;


namespace RaaLabs.Edge.Connectors.OPCUA
{
    /// <summary>
    /// Represents an implementation for <see cref="IProduceEvent"/>
    /// </summary>
    public class OpcuaConnector : IRunAsync, IProduceEvent<Events.OpcuaDatapointOutput>
    {
        /// <summary>
        /// 
        /// </summary>
        public event EventEmitter<Events.OpcuaDatapointOutput> SendDatapoint;
        private OpcuaClient _opcuaClient;
        private readonly ReadValueIdCollection _nodesToRead;
        private readonly ApplicationInstance _opcuaAppInstance;
        private readonly ILogger _logger;
        private readonly OpcuaConfiguration _opcuaConfiguration;
        private readonly IMetricsHandler _metricsHandler;


        /// <summary>
        /// Initializes a new instance of <see cref="OpcuaConnector"/>
        /// </summary>
        /// <param name="logger"><see cref="ILogger"/> for logging</param>
        public OpcuaConnector(ILogger logger, OpcuaConfiguration opcuaConfiguration, IMetricsHandler metricsHandler)
        {
            _logger = logger;
            _opcuaConfiguration = opcuaConfiguration;
            _metricsHandler = metricsHandler;

            var securityConfig = new SecurityConfiguration()
            {
                AutoAcceptUntrustedCertificates = true // ONLY for debugging/early dev
            };

            var config = new ApplicationConfiguration()
            {
                ApplicationName = "Raa Labs OPC UA connector",
                ApplicationUri = "Raa Labs OPC UA connector",
                ApplicationType = ApplicationType.Client,
                TransportConfigurations = new TransportConfigurationCollection(),
                TransportQuotas = new TransportQuotas { OperationTimeout = 15000 },
                ClientConfiguration = new ClientConfiguration { DefaultSessionTimeout = 60000 },
                TraceConfiguration = new TraceConfiguration(),
                SecurityConfiguration = securityConfig
            };

            _opcuaAppInstance = new ApplicationInstance()
            {
                ApplicationName = "Raa Labs OPC UA connector",
                ApplicationType = ApplicationType.Client,
                ApplicationConfiguration = config
            };

            _nodesToRead = InitializeReadValueIdCollection();
        }

        private ReadValueIdCollection InitializeReadValueIdCollection()
        {
            ReadValueIdCollection nodesToRead = new ReadValueIdCollection(){};
            foreach (var nodeId in _opcuaConfiguration.NodeIds)
            {
                // Because nodeId and value cannot be read using the same ReadValueId, but nodeId and value are required 
                nodesToRead.Add(new ReadValueId() { NodeId = nodeId, AttributeId = Attributes.NodeId });
                nodesToRead.Add(new ReadValueId() { NodeId = nodeId, AttributeId = Attributes.Value });
            }

            return nodesToRead;
        }

        public async Task Run()
        {
            _logger.Information("Raa Labs OPC UA connector");
            _opcuaClient = new OpcuaClient(_opcuaAppInstance.ApplicationConfiguration, _opcuaConfiguration, _logger, ClientBase.ValidateResponse);
            await _opcuaClient.ConnectAsync();

            while (true)
            {
                var policy = Policy
                    .Handle<Exception>()
                    .WaitAndRetryForeverAsync(retryAttempt => TimeSpan.FromSeconds(Math.Min(Math.Pow(2, retryAttempt), 3600)),
                    (exception, timeSpan, context) =>
                    {
                        _logger.Error(exception, $"OPC UA connector threw an exception during connect - retrying");
                    });

                await policy.ExecuteAsync(async () =>
                {
                    await ConnectOpcua();
                });

                await Task.Delay(1000);
            }
        }

        private async Task ConnectOpcua()
        {
            try
            {
                if (!_opcuaClient.Session.Connected)
                {
                    await _opcuaClient.ConnectAsync();
                }

                List<Events.OpcuaDatapointOutput> opcuaDatapoints = _opcuaClient.ReadNodes(_nodesToRead);

                foreach (var opcuaDatapoint in opcuaDatapoints)
                {
                    SendDatapoint(opcuaDatapoint);
                    _metricsHandler.NumberOfMessagesSent(1);
                }
            }
            catch (Exception ex)
            {
                _logger.Information(ex.ToString());
            }
        }
    }
}