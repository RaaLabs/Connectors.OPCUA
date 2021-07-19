// Copyright (c) RaaLabs. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System;
using System.Collections;
using System.Threading.Tasks;
using Serilog;
using Opc.Ua;
using Opc.Ua.Client;

namespace RaaLabs.Edge.Connectors.OPCUA
{
    /// <summary>
    /// OPC UA Client with only read functionality.
    /// </summary>
    class OPCUAClient
    {
        /// <summary>
        /// Gets the client session.
        /// </summary>
        public Session Session => _session;

        /// <summary>
        /// Gets the server URL.
        /// </summary>
        public string ServerUrl { get; } = "opc.tcp://Rafaels-MacBook-Pro.local:53530/OPCUA/SimulationServer";

        private ApplicationConfiguration _configuration;
        private Session _session;
        private readonly ILogger _logger;
        private readonly Action<IList, IList> _validateResponse;

        /// <summary>
        /// Initializes a new instance of <see cref="OPCUAClient".
        /// </summary>
        public OPCUAClient(ApplicationConfiguration configuration, ILogger logger, Action<IList, IList> validateResponse)
        {
            _validateResponse = validateResponse;
            _logger = logger;
            _configuration = configuration;
            _configuration.CertificateValidator.CertificateValidation += CertificateValidation;
        }

        /// <summary>
        /// Creates a session with the UA server
        /// </summary>
        public async Task<bool> ConnectAsync()
        {
            try
            {
                if (_session != null && _session.Connected == true)
                {
                    _logger.Information("Session already connected!");
                }
                else
                {
                    _logger.Information("Connecting...");

                    EndpointDescription endpointDescription = CoreClientUtils.SelectEndpoint(ServerUrl, false);
                    EndpointConfiguration endpointConfiguration = EndpointConfiguration.Create(_configuration);
                    ConfiguredEndpoint endpoint = new ConfiguredEndpoint(null, endpointDescription, endpointConfiguration);

                    Session session = await Session.Create(
                        _configuration,
                        endpoint,
                        false,
                        false,
                        _configuration.ApplicationName,
                        30 * 60 * 1000,
                        new UserIdentity(),
                        null
                    );

                    if (session != null && session.Connected)
                    {
                        this._session = session;
                    }

                    _logger.Information($"New Session Created with SessionName = {this._session.SessionName}");
                }

                return true;
            }
            catch (Exception ex)
            {
                // Log Error
                _logger.Error($"Create Session Error : {ex.Message}");
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
                if (_session != null)
                {
                    _logger.Information("Disconnecting...");

                    _session.Close();
                    _session.Dispose();
                    _session = null;

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
        public void ReadNodes()
        {
            if (_session == null || _session.Connected == false)
            {
                _logger.Information("Session not connected!");
                return;
            }

            try
            {
                ReadValueIdCollection nodesToRead = new ReadValueIdCollection()
                {
                    new ReadValueId() { NodeId = "ns=3;i=1002", AttributeId = Attributes.Value },
                    new ReadValueId() { NodeId = "ns=3;i=1001", AttributeId = Attributes.Value }
                };

                _logger.Information("Reading nodes...");

                _session.Read(
                    null,
                    0,
                    TimestampsToReturn.Both,
                    nodesToRead,
                    out DataValueCollection resultsValues,
                    out DiagnosticInfoCollection diagnosticInfos
                );

                _validateResponse(resultsValues, nodesToRead);

                foreach (DataValue result in resultsValues)
                {
                    _logger.Information("Read Value = {0} , StatusCode = {1}", result.Value, result.StatusCode);
                }
            }
            catch (Exception ex)
            {
                _logger.Information($"Read Nodes Error : {ex.Message}.");
            }
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
    }
}