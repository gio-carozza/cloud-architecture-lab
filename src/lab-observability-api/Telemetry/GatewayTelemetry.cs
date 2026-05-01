using System.Diagnostics;
using System.Diagnostics.Metrics;

namespace Lab.Observability.Api.Telemetry;

public static class GatewayTelemetry
{
    public const string ServiceName = "Lab.Observability.Api";

    public static readonly ActivitySource ActivitySource = new(ServiceName);
    public static readonly Meter Meter = new(ServiceName);

    public static readonly Histogram<double> ProviderLatencyMs =
        Meter.CreateHistogram<double>("ai.provider.latency.ms");

    public static readonly Counter<long> ProviderRequestCount =
        Meter.CreateCounter<long>("ai.provider.requests");

    public static readonly Counter<long> ProviderFailureCount =
        Meter.CreateCounter<long>("ai.provider.failures");
}