// Copyright (c) RaaLabs. All rights reserved.
// Licensed under the GPLv2 License. See LICENSE file in the project root for full license information.

using System;
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;
using Opc.Ua;
using Opc.Ua.Client;
using Serilog;

namespace RaaLabs.Edge.Connectors.OPCUA;

public class Client : ICreateSessions
{
    readonly ApplicationConfiguration _application;
    readonly ConfiguredEndpoint _endpoint;
    readonly UserIdentity _identity;
    readonly ISessionFactory _factory;
    readonly IMetricsHandler _metrics;
    readonly ILogger _logger;

    public Client(ConnectorConfiguration config, ISessionFactory factory, IMetricsHandler metrics, ILogger logger)
    {
        _application = ConnectorDescription();
        _application.ClientConfiguration = new()
        {
            DefaultSessionTimeout = (int)TimeSpan.FromHours(1).TotalMilliseconds,
        };

        (_endpoint, _identity) = ConnectAnonymouslyToAnyServerOn(config.ServerUrl);
        _factory = factory;
        _metrics = metrics;
        _logger = logger;
    }

    public async Task<ISession> ConnectToServer(CancellationToken cancellationToken)
    {
        try
        {
            var sessionName = $"RaaEdge OPCUA Connector {Guid.NewGuid()}";
            _logger.Information("Creating session '{SessionName}' with OPCUA server on '{EndpointUrl}'", sessionName, _endpoint.EndpointUrl);
            _metrics.NumberOfSessionConnectionAttempts(1);
            var timer = Stopwatch.StartNew();

            var session = await _factory.CreateAsync(
                _application,
                _endpoint,
                updateBeforeConnect: false,
                checkDomain: false,
                sessionName,
                (uint)_application.ClientConfiguration.DefaultSessionTimeout,
                _identity,
                [],
                cancellationToken
            ).ConfigureAwait(false);

            _metrics.NumberOfSessionConnections(1);
            _metrics.SessionConnectionTime(timer.Elapsed.TotalSeconds);
            return session;
        }
        catch (Exception error)
        {
            _logger.Error(error, "Failed to connect to server");
            throw;
        }
    }

    static (ConfiguredEndpoint, UserIdentity) ConnectAnonymouslyToAnyServerOn(string url)
    {
        var descriptor = new EndpointDescription(url);
        descriptor.UserIdentityTokens.Add(new UserTokenPolicy(UserTokenType.Anonymous));
        descriptor.Server.ApplicationUri = null;
        var endpoint = new ConfiguredEndpoint(null, descriptor);

        var identity = new UserIdentity(new AnonymousIdentityToken());
        return (endpoint, identity);
    }

    static ApplicationConfiguration ConnectorDescription() => new()
    {
        ApplicationType = ApplicationType.Client,

        ApplicationName = "RaaEDGE OPC UA Connector",
        ApplicationUri = "https://github.com/RaaLabs/Connectors.OPCUA",
        ProductUri = "https://github.com/RaaLabs",
    };
}
