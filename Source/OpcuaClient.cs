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

namespace RaaLabs.Edge.Connectors.OPCUA;

/// <summary>
/// OPC UA Client with only read functionality.
/// </summary>
class OpcuaClient
{
    private readonly OpcuaConfiguration _opcuaConfiguration;
    private readonly ApplicationConfiguration _applicationConfiguration;
    public Session Session { get; set; }
    private readonly ILogger _logger;
    private readonly Action<IList, IList> _validateResponse;

    /// <summary>
    /// Initializes a new instance of <see cref="OpcuaClient".
    /// </summary>
    public OpcuaClient(ApplicationConfiguration applicationConfiguration, OpcuaConfiguration opcuaConfiguration, ILogger logger, Action<IList, IList> validateResponse)
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
            if (Session != null && Session.Connected)
            {
                _logger.Information("Session already connected!");
            }
            else
            {
                _logger.Information("Connecting...");

                EndpointDescription endpointDescription = CoreClientUtils.SelectEndpoint(discoveryUrl:_opcuaConfiguration.ServerUrl, useSecurity:true);
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
                ).ConfigureAwait(false);

                if (opcuaSession != null && opcuaSession.Connected)
                {
                    this.Session = opcuaSession;
                }

                _logger.Information($"New Session Created with SessionName = {this.Session.SessionName}");
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
            if (Session != null)
            {
                _logger.Information("Disconnecting...");

                Session.Close();
                Session.Dispose();
                Session = null;

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
    public List<Events.OpcuaDatapointOutput> ReadNodes(ReadValueIdCollection nodes)
    {
        _logger.Information("Reading nodes...");

        Session.Read(
            null,
            0,
            TimestampsToReturn.Both,
            nodes,
            out DataValueCollection resultsValues, // DataValueCollection is ordered
            out DiagnosticInfoCollection diagnosticInfos
        );

        _validateResponse(resultsValues, nodes);

        var resultsValuesGroups = resultsValues.Batch(2);
        List<Events.OpcuaDatapointOutput> outputs = FormatOutput(resultsValuesGroups);

        return outputs;
    }

    /// <summary>
    /// Handles the certificate validation event.
    /// This event is triggered every time an untrusted certificate is received from the server.
    /// </summary>
    private void CertificateValidation(CertificateValidator sender, CertificateValidationEventArgs e)
    {
        ServiceResult error = e.Error;
        while (error != null)
        {
            _logger.Information(error.ToString());
            error = error.InnerResult;
        }

        bool certificateAccepted = false;
        bool subjectMatch = false;
        bool issuerMatch = false;
        bool certificateIsNotExpired = false;

        if (e.Certificate.Subject == _opcuaConfiguration.OpcUaServerCertificateSubject)
        {
            subjectMatch = true;
        }
        else
        {
            _logger.Information("Subject from server certificate does not match. Expected={0}, Actual={1}", _opcuaConfiguration.OpcUaServerCertificateSubject, e.Certificate.Subject);
        }

        if (e.Certificate.Issuer == _opcuaConfiguration.OpcUaServerCertificateIssuer)
        {
            issuerMatch = true;
        }
        else
        {
            _logger.Information("Issuer from server certificate does not match. Expected={0}, Actual={1}", _opcuaConfiguration.OpcUaServerCertificateIssuer, e.Certificate.Issuer);
        }

        DateTimeOffset dateTimeOffsetNow = DateTimeOffset.Now;
        if (dateTimeOffsetNow >= e.Certificate.NotBefore && dateTimeOffsetNow <= e.Certificate.NotAfter)
        {
            certificateIsNotExpired = true;
        }

        if (subjectMatch && issuerMatch && certificateIsNotExpired)
        {
            certificateAccepted = true;
        }

        if (certificateAccepted)
        {
            _logger.Information("Untrusted Certificate accepted. Subject={0}, Issuer={1}", e.Certificate.Subject, e.Certificate.Issuer);
        }

        else
        {
            _logger.Information("Untrusted Certificate rejected. Subject={0}, Issuer={1}", e.Certificate.Subject, e.Certificate.Issuer);
        }

        e.AcceptAll = certificateAccepted;
    }

    /// <summary>
    /// Creates a List<Events.OPCUADatapointOutput> based on two ReadValueIds, one with the value and one with the nodeId.
    /// </summary>
    /// <param name="resultsValuesGroups"></param>
    /// <returns></returns>
    private static List<Events.OpcuaDatapointOutput> FormatOutput(IEnumerable<IEnumerable<DataValue>> resultsValuesGroups)
    {
        List<Events.OpcuaDatapointOutput> datapoints = new List<Events.OpcuaDatapointOutput>();


        foreach (var resultValueGroup in resultsValuesGroups)
        {
            var opcuaDatapointOutput = new Events.OpcuaDatapointOutput
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

