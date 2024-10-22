using System;
using System.Globalization;
using RaaLabs.Edge.Connectors.OPCUA.Events;
using RaaLabs.Edge.Modules.Diagnostics.Health;
using RaaLabs.Edge.Modules.EventHandling;
using Serilog;

namespace RaaLabs.Edge.Connectors.OPCUA;

public class HealthCheck : IConsumeEvent<OpcuaDatapointOutput>, IExposeHealthStatus
{
    readonly TimeSpan _maxTimeBetweenDatapoints;
    readonly TimeProvider _clock;
    readonly ILogger _logger;
    DateTimeOffset _lastTimestampReceived;

    public HealthCheck(TimeProvider clock, ILogger logger)
    {
        _maxTimeBetweenDatapoints = TimeSpan.FromSeconds(double.Parse(Environment.GetEnvironmentVariable("HEALTHCHECK_MAX_SECONDS_BETWEEN_DATAPOINTS") ?? "120", CultureInfo.CurrentCulture));
        _clock = clock;
        _logger = logger;
        _lastTimestampReceived = _clock.GetUtcNow();
    }

    public void Handle(OpcuaDatapointOutput @event)
    {
        _lastTimestampReceived = _clock.GetUtcNow();
        _logger.Debug("Sent data, updating last timestamp received in healthcheck with {LastTimestampReceived}", _lastTimestampReceived);
    }

    public bool IsReady => true;
    public bool IsHealthy
    {
        get
        {
            var timeSinceLastDatapoint = _clock.GetUtcNow() - _lastTimestampReceived;
            if (timeSinceLastDatapoint > _maxTimeBetweenDatapoints)
            {
                _logger.Warning("Time since data was sent is {Since}, returning unhealthy", timeSinceLastDatapoint);
                return false;
            }
            _logger.Debug("Time since data was sent is {Since}, returning healthy", timeSinceLastDatapoint);
            return true;
        }
    }
}
