// Copyright (c) RaaLabs. All rights reserved.
// Licensed under the GPLv2 License. See LICENSE file in the project root for full license information.

using System;
using Machine.Specifications;
using Moq;
using Serilog;

namespace RaaLabs.Edge.Connectors.OPCUA.for_DataPointParser.given;

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
        
        parser = new DataPointParser(configuration, logger.Object, metrics.Object, clock.Object);
    };
}