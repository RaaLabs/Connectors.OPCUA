// Copyright (c) RaaLabs. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System;
using System.Collections;
using System.Collections.Generic;
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

        public OPCUAConfiguration _opcuaConfiguration;
        private ApplicationConfiguration _applicationConfiguration;
        private Session _session;
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
                if (_session != null && _session.Connected == true)
                {
                    _logger.Information("Session already connected!");
                }
                else
                {
                    _logger.Information("Connecting...");

                    EndpointDescription endpointDescription = CoreClientUtils.SelectEndpoint(_opcuaConfiguration.ServerUrl, false);
                    EndpointConfiguration endpointConfiguration = EndpointConfiguration.Create(_applicationConfiguration);
                    ConfiguredEndpoint endpoint = new ConfiguredEndpoint(null, endpointDescription, endpointConfiguration);

                    Session session = await Session.Create(
                        _applicationConfiguration,
                        endpoint,
                        false,
                        false,
                        _applicationConfiguration.ApplicationName,
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
        public IEnumerable<DataValue> ReadNodes()
        {
            ReadValueIdCollection nodesToRead = new ReadValueIdCollection()
            {
                new ReadValueId() { NodeId = "ns=3;i=1002", AttributeId = Attributes.NodeId },
                new ReadValueId() { NodeId = "ns=3;i=1002", AttributeId = Attributes.Value },
                new ReadValueId() { NodeId = "ns=3;i=1001", AttributeId = Attributes.NodeId },
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
                _logger.Information("Read Value = {0} , StatusCode = {1}, Timestamp = {2}", result.Value, result.StatusCode, result.ServerTimestamp);
            }

            return resultsValues;
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