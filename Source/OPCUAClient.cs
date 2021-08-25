// Copyright (c) RaaLabs. All rights reserved.
// Licensed under the GPLv2 License. See LICENSE file in the project root for full license information.

using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using Serilog;
using System.Linq;
using Opc.Ua;
using Opc.Ua.Client;
using MoreLinq;

namespace RaaLabs.Edge.Connectors.OPCUA
{
    /// <summary>
    /// OPC UA Client with only read functionality.
    /// </summary>
    class OPCUAClient
    {
        public OPCUAConfiguration _opcuaConfiguration;
        private ApplicationConfiguration _applicationConfiguration;
        public Session session;
        private readonly ILogger _logger;
        private readonly Action<IList, IList> _validateResponse;

        /// <summary>
        /// Initializes a new instance of <see cref="OPCUAClient".
        /// </summary>
        public OPCUAClient(ApplicationConfiguration applicationConfiguration, OPCUAConfiguration opcuaConfiguration, ILogger logger, Action<IList, IList> validateResponse)
        {
            _validateResponse = validateResponse;
            _logger = logger;
            _opcuaConfiguration = opcuaConfiguration;
            _applicationConfiguration = applicationConfiguration;
            _applicationConfiguration.CertificateValidator.CertificateValidation += CertificateValidation;
        }

        /// <summary>
        /// Creates a session with the UA server
        /// </summary>
        public async Task<bool> ConnectAsync()
        {
            try
            {
                if (session != null && session.Connected == true)
                {
                    _logger.Information("Session already connected!");
                }
                else
                {
                    _logger.Information("Connecting...");

                    EndpointDescription endpointDescription = CoreClientUtils.SelectEndpoint(_opcuaConfiguration.ServerUrl, false);
                    EndpointConfiguration endpointConfiguration = EndpointConfiguration.Create(_applicationConfiguration);
                    ConfiguredEndpoint endpoint = new ConfiguredEndpoint(null, endpointDescription, endpointConfiguration);

                    Session opcuaSession = await Session.Create(
                        _applicationConfiguration,
                        endpoint,
                        false,
                        false,
                        _applicationConfiguration.ApplicationName,
                        30 * 60 * 1000,
                        new UserIdentity(),
                        null
                    );

                    if (opcuaSession != null && opcuaSession.Connected)
                    {
                        this.session = opcuaSession;
                    }

                    _logger.Information($"New Session Created with SessionName = {this.session.SessionName}");
                }

                return true;
            }
            catch (Exception ex)
            {
                _logger.Error($"Create Session Error : {ex.ToString()}");
                return false;
            }
        }


        /// <summary>
        /// Disconnects the session.
        /// </summary>
        public void Disconnect()
        {
            try
            {
                if (session != null)
                {
                    _logger.Information("Disconnecting...");

                    session.Close();
                    session.Dispose();
                    session = null;

                    _logger.Information("Session Disconnected.");
                }
                else
                {
                    _logger.Information("Session not created!");
                }
            }
            catch (Exception ex)
            {
                _logger.Information($"Disconnect Error : {ex.Message}");
            }
        }


        /// <summary>
        /// Read a list of nodes from Server
        /// </summary>
        public List<Events.OPCUADatapointOutput> ReadNodes(ReadValueIdCollection nodes)
        {
            _logger.Information("Reading nodes...");

            session.Read(
                null,
                0,
                TimestampsToReturn.Both,
                nodes,
                out DataValueCollection resultsValues, // DataValueCollection is ordered
                out DiagnosticInfoCollection diagnosticInfos
            );

            _validateResponse(resultsValues, nodes);

            var resultsValuesGroups = resultsValues.Batch(2);
            List<Events.OPCUADatapointOutput> outputs = FormatOutput(resultsValuesGroups);

            return outputs;
        }

        /// <summary>
        /// Handles the certificate validation event.
        /// This event is triggered every time an untrusted certificate is received from the server.
        /// </summary>
        private void CertificateValidation(CertificateValidator sender, CertificateValidationEventArgs e)
        {
            bool certificateAccepted = true;

            // ****
            // Implement a custom logic to decide if the certificate should be
            // accepted or not and set certificateAccepted flag accordingly.
            // The certificate can be retrieved from the e.Certificate field
            // ***

            ServiceResult error = e.Error;
            while (error != null)
            {
                _logger.Information(error.ToString());
                error = error.InnerResult;
            }

            if (certificateAccepted)
            {
                _logger.Information("Untrusted Certificate accepted. SubjectName = {0}", e.Certificate.SubjectName);
            }

            e.AcceptAll = certificateAccepted;
        }

        /// <summary>
        /// Creates a List<Events.OPCUADatapointOutput> based on two ReadValueIds, one with the value and one with the nodeId.
        /// </summary>
        /// <param name="resultsValuesGroups"></param>
        /// <returns></returns>
        private List<Events.OPCUADatapointOutput> FormatOutput(IEnumerable<IEnumerable<DataValue>> resultsValuesGroups)
        {
            List<Events.OPCUADatapointOutput> datapoints = new List<Events.OPCUADatapointOutput>();


            foreach (var resultValueGroup in resultsValuesGroups)
            {
                var opcuaDatapointOutput = new Events.OPCUADatapointOutput
                {
                    Source = "OPCUA",
                    Tag = resultValueGroup.ElementAt(0).Value.ToString(), // this is the node id
                    Timestamp = ((DateTimeOffset)resultValueGroup.ElementAt(1).ServerTimestamp).ToUnixTimeMilliseconds(),
                    Value = resultValueGroup.ElementAt(1).Value.ToString() // this is the node value
                };

                datapoints.Add(opcuaDatapointOutput);
            }
            
            return datapoints;
        }
    }
}
