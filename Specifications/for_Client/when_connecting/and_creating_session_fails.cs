using System;
using System.Collections.Generic;
using System.Threading;
using Machine.Specifications;
using Moq;
using Opc.Ua;
using Opc.Ua.Client;
using Serilog;
using It = Machine.Specifications.It;

namespace RaaLabs.Edge.Connectors.OPCUA.for_Client.when_connecting;

public class and_creating_session_fails
{
    static Exception thrown_exception;
    static Mock<ISessionFactory> session_factory;
    static Client client;

    Establish context = () =>
    {
        thrown_exception = new();

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
            .ThrowsAsync(thrown_exception);

        client = new(
            new ConnectorConfiguration()
            {
                ServerUrl = "opc.tcp://some.server:4840/some/path"
            },
            session_factory.Object,
            Mock.Of<IMetricsHandler>(),
            Mock.Of<ILogger>()
        );
    };

    static Exception error;
    Because of = async () => error = await Catch.ExceptionAsync(() => client.ConnectToServer(CancellationToken.None));

    It should_fail = () => error.ShouldEqual(thrown_exception);
}