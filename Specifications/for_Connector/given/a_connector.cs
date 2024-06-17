using System.Collections.Generic;
using System.Threading.Tasks;
using Machine.Specifications;
using Moq;
using RaaLabs.Edge.Connectors.OPCUA.Events;
using Serilog;

namespace RaaLabs.Edge.Connectors.OPCUA.for_Connector.given;

public class a_connector
{
    protected static Mock<ICreateSessions> sessions;
    protected static Mock<IRetrieveData> retriever;
    protected static Mock<ICreateDatapointsFromDataValues> datapoints;

    protected static Connector connector;

    Establish context = () =>
    {
        sessions = new();

        retriever = new();

        datapoints = new();

        connector = new(sessions.Object, retriever.Object, datapoints.Object, Mock.Of<ILogger>());

        var collected = sent_datapoints = [];
        connector.SendDatapoint += _ =>
        {
            collected.Add(_);
            return Task.CompletedTask;
        };
    };

    protected static List<OpcuaDatapointOutput> sent_datapoints;
}