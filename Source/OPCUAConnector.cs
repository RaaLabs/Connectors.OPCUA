// Copyright (c) RaaLabs. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System;
using Serilog;
using System.Threading.Tasks;
using System.Collections;
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
    public class OPCUAConnector : IRunAsync, IProduceEvent<Events.OPCUADatapointOutput>
    {
        public event EventEmitter<Events.OPCUADatapointOutput> SendDatapoint;
        private OPCUAClient _opcuaClient;
        private readonly ApplicationInstance _opcuaAppInstance;
        private readonly ILogger _logger;
        private readonly OPCUAConfiguration _opcuaConfiguration;


        /// <summary>
        /// Initializes a new instance of <see cref="OPCUAConnector"/>
        /// </summary>
        /// <param name="logger"><see cref="ILogger"/> for logging</param>
        public OPCUAConnector(ILogger logger, OPCUAConfiguration opcuaConfiguration)
        {
            _logger = logger;
            _opcuaConfiguration = opcuaConfiguration;

            var securityConfig = new SecurityConfiguration()
            {
                AutoAcceptUntrustedCertificates = true // ONLY for debugging/early dev
            };

            var config = new ApplicationConfiguration()
            {
                ApplicationName = "Raa Labs OPC UA connector",
                ApplicationUri = Utils.Format(@"urn:{0}:" + "Raa Labs OPC UA connector" + "", "Rafaels-MacBook-Pro.local"),
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
        }

        public async Task Run()
        {
            _logger.Information("Raa Labs OPC UA connector");
            while (true)
            {
                var policy = Policy
                    .Handle<Exception>()
                    .WaitAndRetryForeverAsync(retryAttempt => TimeSpan.FromSeconds(Math.Min(Math.Pow(2, retryAttempt),3600)),
                    (exception, timeSpan, context) =>
                    {
                        _logger.Error(exception, $"OPC UA connector threw an exception during connect - retrying");
                    });

                await policy.ExecuteAsync(async () =>
                {
                    await ConnectOPCUA();
                });
                
                _logger.Information("Waiting 1 sec ...");
                await Task.Delay(1000);
            }
        }

        private async Task ConnectOPCUA()
        {
            _opcuaClient = new OPCUAClient(_opcuaAppInstance.ApplicationConfiguration, _opcuaConfiguration, _logger, ClientBase.ValidateResponse);

            try
            {
                bool connected = await _opcuaClient.ConnectAsync();
                if (connected)
                {
                    IEnumerable<DataValue> opcuaDataValues = _opcuaClient.ReadNodes();
                    _opcuaClient.Disconnect();

                    foreach (var opcuaDataValue in opcuaDataValues)
                    {
                        var opcuaReceived = new Events.OPCUADatapointOutput
                        {
                            Source = "OPCUA",
                            Tag = "",
                            Timestamp = new long(),
                            Value = ""
                        };

                        SendDatapoint(opcuaReceived);
                    }
                }
                else
                {
                    _logger.Information("Could not connect to server!");
                }
            }
            catch (Exception ex)
            {
                _logger.Information(ex.Message);
            }
        }
    }
}