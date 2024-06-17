// Copyright (c) RaaLabs. All rights reserved.
// Licensed under the GPLv2 License. See LICENSE file in the project root for full license information.

using System.Collections.Generic;
using System.Threading;
using Machine.Specifications;
using Moq;
using Opc.Ua;
using Opc.Ua.Client;
using Serilog;
using It = Machine.Specifications.It;

namespace RaaLabs.Edge.Connectors.OPCUA.for_Client.when_connecting;

public class anonymously_to_any_server
{
    static ISession created_session;
    static Mock<ISessionFactory> session_factory;
    static Client client;

    Establish context = () =>
    {
        created_session = Mock.Of<ISession>();

        session_factory = new();
        session_factory
            .Setup(_ => _.CreateAsync(
                Moq.It.IsAny<ApplicationConfiguration>(),
                Moq.It.IsAny<ConfiguredEndpoint>(),
                Moq.It.IsAny<bool>(),
                Moq.It.IsAny<bool>(),
                Moq.It.IsAny<string>(),
                Moq.It.IsAny<uint>(),
                Moq.It.IsAny<IUserIdentity>(),
                Moq.It.IsAny<IList<string>>(),
                Moq.It.IsAny<CancellationToken>()
            ))
            .ReturnsAsync(created_session);

        client = new(
            new ConnectorConfiguration()
            {
                ServerUrl = "opc.tcp://some.server:4840/some/path"
            },
            session_factory.Object,
            Mock.Of<IMetricsHandler>(),
            Mock.Of<ILogger>());
    };

    static ISession session;
    Because of = async () => session = await client.ConnectToServer(CancellationToken.None);

    It should_use_an_endpoint_with_the_correct_url = () => session_factory.Verify(_ => _.CreateAsync(
        Moq.It.IsAny<ApplicationConfiguration>(),
        Moq.It.Is<ConfiguredEndpoint>(_ => _.EndpointUrl.ToString() == "opc.tcp://some.server:4840/some/path"),
        Moq.It.IsAny<bool>(),
        Moq.It.IsAny<bool>(),
        Moq.It.IsAny<string>(),
        Moq.It.IsAny<uint>(),
        Moq.It.IsAny<IUserIdentity>(),
        Moq.It.IsAny<IList<string>>(),
        Moq.It.IsAny<CancellationToken>()
    ));
    It should_use_an_endpoint_without_server_applicationuri_set = () => session_factory.Verify(_ => _.CreateAsync(
        Moq.It.IsAny<ApplicationConfiguration>(),
        Moq.It.Is<ConfiguredEndpoint>(_ => _.Description.Server.ApplicationUri == null),
        Moq.It.IsAny<bool>(),
        Moq.It.IsAny<bool>(),
        Moq.It.IsAny<string>(),
        Moq.It.IsAny<uint>(),
        Moq.It.IsAny<IUserIdentity>(),
        Moq.It.IsAny<IList<string>>(),
        Moq.It.IsAny<CancellationToken>()
    ));
    It should_use_an_endpoint_with_anonymous_policy = () => session_factory.Verify(_ => _.CreateAsync(
        Moq.It.IsAny<ApplicationConfiguration>(),
        Moq.It.Is<ConfiguredEndpoint>(_ => _.Description.UserIdentityTokens.Exists(_ => _.TokenType == UserTokenType.Anonymous)),
        Moq.It.IsAny<bool>(),
        Moq.It.IsAny<bool>(),
        Moq.It.IsAny<string>(),
        Moq.It.IsAny<uint>(),
        Moq.It.IsAny<IUserIdentity>(),
        Moq.It.IsAny<IList<string>>(),
        Moq.It.IsAny<CancellationToken>()
    ));
    It should_use_an_anonymous_identity = () => session_factory.Verify(_ => _.CreateAsync(
        Moq.It.IsAny<ApplicationConfiguration>(),
        Moq.It.IsAny<ConfiguredEndpoint>(),
        Moq.It.IsAny<bool>(),
        Moq.It.IsAny<bool>(),
        Moq.It.IsAny<string>(),
        Moq.It.IsAny<uint>(),
        Moq.It.Is<IUserIdentity>(u => u.TokenType == UserTokenType.Anonymous),
        Moq.It.IsAny<IList<string>>(),
        Moq.It.IsAny<CancellationToken>()
    ));
    It should_return_the_session_from_the_factory = () => session.ShouldEqual(created_session);
}
