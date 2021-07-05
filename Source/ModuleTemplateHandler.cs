// Copyright (c) RaaLabs. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using Serilog;
using RaaLabs.Edge.Modules.EventHandling;


namespace RaaLabs.Edge.ModuleTemplate
{
    public class ModuleTemplateHandler : IConsumeEvent<Events.ModuleTemplateDatapointInput>, IProduceEvent<Events.ModuleTemplateDatapointOutput>
    {
        public event EventEmitter<Events.ModuleTemplateDatapointOutput> SendDatapoint;
        private readonly ILogger _logger;
        private readonly ModuleTemplateConfiguration _configuration;

        public ModuleTemplateHandler(ILogger logger, ModuleTemplateConfiguration configuration)
        {
            _logger = logger;
            _configuration = configuration;
        }

        public void Handle(Events.ModuleTemplateDatapointInput @event)
        {
            var moduleTemplateDatapointOutput = new Events.ModuleTemplateDatapointOutput
            {
                TimeSeries = @event.TimeSeries,
                Value = @event.Value,
                Timestamp = @event.Timestamp
            };
            if(@event.Value < _configuration.SampleConfigValue) SendDatapoint(moduleTemplateDatapointOutput);
        }
    }
}
