// Copyright (c) RaaLabs. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using Serilog;
using RaaLabs.Edge.Modules.EventHandling;


namespace RaaLabs.Edge.Connectors.OPCUA
{
    public class OPCUAHandler : IConsumeEvent<Events.OPCUADatapointInput>, IProduceEvent<Events.OPCUADatapointOutput>
    {
        public event EventEmitter<Events.OPCUADatapointOutput> SendDatapoint;
        private readonly ILogger _logger;
        private readonly OPCUAConfiguration _configuration;

        public OPCUAHandler(ILogger logger, OPCUAConfiguration configuration)
        {
            _logger = logger;
            _configuration = configuration;
        }

        public void Handle(Events.OPCUADatapointInput @event)
        {
            var OPCUADatapointOutput = new Events.OPCUADatapointOutput
            {
                TimeSeries = @event.TimeSeries,
                Value = @event.Value,
                Timestamp = @event.Timestamp
            };
            if(@event.Value < _configuration.SampleConfigValue) SendDatapoint(OPCUADatapointOutput);
        }
    }
}
