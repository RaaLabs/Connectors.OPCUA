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

namespace RaaLabs.Edge.Connectors.OPCUA
{
    /// <summary>
    /// OPC UA Client with only read functionality.
    /// </summary>
    class OPCUAClient
    {
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
        public List<Events.OPCUADatapointOutput> ReadNodes()
        {
            ReadValueIdCollection nodesToRead = new ReadValueIdCollection(){};
            foreach (var nodeId in _opcuaConfiguration.NodeIds)
            {
                // Because nodeId and value cannot be read using the same ReadValueId, but nodeId and value are required 
                nodesToRead.Add(new ReadValueId() { NodeId = nodeId, AttributeId = Attributes.NodeId });
                nodesToRead.Add(new ReadValueId() { NodeId = nodeId, AttributeId = Attributes.Value });
            }

            _logger.Information("Reading nodes...");

            _session.Read(
                null,
                0,
                TimestampsToReturn.Both,
                nodesToRead,
                out DataValueCollection resultsValues, // DataValueCollection is ordered
                out DiagnosticInfoCollection diagnosticInfos
            );

            _validateResponse(resultsValues, nodesToRead);

            var resultsValuesGroups = Split(resultsValues);
            List<Events.OPCUADatapointOutput> outputs = FormatOutput(resultsValuesGroups);

            foreach (var output in outputs)
            {
                _logger.Information("OPC UA data read: Source: {0} , Tag (NodeId): {1}, Value: {2} Timestamp: {3}", output.Source, output.Tag, output.Value, output.Timestamp);
            }

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
        private List<Events.OPCUADatapointOutput> FormatOutput(List<List<DataValue>> resultsValuesGroups)
        {
            List<Events.OPCUADatapointOutput> datapoints = new List<Events.OPCUADatapointOutput>();

            foreach (var resultValueGroup in resultsValuesGroups)
            {
                var opcuaDatapointOutput = new Events.OPCUADatapointOutput
                {
                    Source = "OPCUA",
                    Tag = resultValueGroup[0].Value.ToString(), // this is the node id
                    Timestamp = ((DateTimeOffset)resultValueGroup[1].ServerTimestamp).ToUnixTimeMilliseconds(),
                    Value = resultValueGroup[1].Value.ToString() // this is the node value
                };

                datapoints.Add(opcuaDatapointOutput);
            }
            
            return datapoints;
        }


        /// <summary>
        /// Groups a List<DataValue> into a List<List<DataValue>> each with two DataValues, one for the nodeId and one for the value.
        /// </summary>
        private static List<List<DataValue>> Split<DataValue>(List<DataValue> source)
        {
            return source
                .Select((x, i) => new { Index = i, Value = x })
                .GroupBy(x => x.Index / 2)
                .Select(x => x.Select(v => v.Value).ToList())
                .ToList();
        }
    }
}