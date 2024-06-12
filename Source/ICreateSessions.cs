using System.Threading;
using System.Threading.Tasks;
using Opc.Ua.Client;

namespace RaaLabs.Edge.Connectors.OPCUA;

public interface ICreateSessions
{
    Task<Session> ConnectToServer(CancellationToken cancellationToken);
}

// var application = new ApplicationConfiguration()
// {
//     ApplicationName = "RaaEDGE OPC UA Connector",
//     ApplicationType = ApplicationType.Client,
//     ApplicationUri = "urn:raaedge:client",
//     ProductUri = "urn:raaedge.github.io:opcua-client",

//     ClientConfiguration = new()
//     {
//         DefaultSessionTimeout = 30*60*1000,
//     },
// };

// var descriptor = new EndpointDescription("opc.tcp://localhost:4840/opcserver/");
// descriptor.UserIdentityTokens.Add(new UserTokenPolicy(UserTokenType.Anonymous));
// descriptor.Server.ApplicationUri = null;

// var endpoint = new ConfiguredEndpoint(null, descriptor);
// var identity = new UserIdentity(new AnonymousIdentityToken());

// var session = await Session.Create(
//     application,
//     endpoint,
//     updateBeforeConnect: false,
//     checkDomain: false,
//     "session name 1",
//     30*60*1000,
//     identity,
//     [],
//     CancellationToken.None
// );
