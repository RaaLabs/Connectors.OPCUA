using System;
using Machine.Specifications;
using Moq;
using Serilog;

namespace RaaLabs.Edge.Connectors.OPCUA.for_CreateDatapointsFromDataValues.given;

public class a_creator
{
    protected static Mock<TimeProvider> clock;
    protected static Mock<ILogger> logger;
    static Mock<IMetricsHandler> metrics;
    
    protected static DataPointParser parser;
    
    Establish context = () =>
    {
        clock = new();
        logger = new();
        metrics = new();
        
        var configuration = new ConnectorConfiguration
        {
            ServerUrl = "server-url"
        };
        
        parser = new DataPointParser(clock.Object, logger.Object, metrics.Object, configuration);
    };
}