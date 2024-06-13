// Copyright (c) RaaLabs. All rights reserved.
// Licensed under the GPLv2 License. See LICENSE file in the project root for full license information.

using System.Diagnostics.CodeAnalysis;
using RaaLabs.Edge.Modules.EventHandling;
using RaaLabs.Edge.Modules.EdgeHub;
using RaaLabs.Edge.Modules.Configuration;
using RaaLabsDiagnostics = RaaLabs.Edge.Modules.Diagnostics.Diagnostics;
using Autofac;
using Opc.Ua.Client;



namespace RaaLabs.Edge.Connectors.OPCUA;

[ExcludeFromCodeCoverage]
static class Program
{
    static void Main(string[] args)
    {
        var application = new ApplicationBuilder()
            .WithModule<EventHandling>()
            .WithModule<Configuration>()
            .WithModule<EdgeHub>()
            .WithModule<RaaLabsDiagnostics>()
            .WithTask<Connector>()
            .WithManualRegistration(_ =>
            {
                _.RegisterInstance(DefaultSessionFactory.Instance).As<ISessionFactory>();
                _.RegisterType<Connector>().As<ICreateSessions>();
                _.RegisterType<DataReader>().As<IRetrieveData>();
            })
            .Build();

        application.Run().Wait();
    }
}
