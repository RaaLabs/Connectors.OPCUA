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

public class two_times
{
    static Mock<ISessionFactory> session_factory;
    static Client client;

    Establish context = () =>
    {
        session_factory = new();

        var names = session_names = [];
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
            .Callback<ApplicationConfiguration, ConfiguredEndpoint, bool, bool, string, uint, IUserIdentity, IList<string>, CancellationToken>((_, _, _, _, name, _, _, _, _) => names.Add(name))
            .ReturnsAsync(Mock.Of<ISession>());

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

    static List<string> session_names;

    Because of = async () =>
    {
        await client.ConnectToServer(CancellationToken.None);
        await client.ConnectToServer(CancellationToken.None);
    };

    It should_create_two_sessions = () => session_names.Count.ShouldEqual(2);
    It should_use_a_different_name_each_time = () => session_names[0].ShouldNotEqual(session_names[1]);
}