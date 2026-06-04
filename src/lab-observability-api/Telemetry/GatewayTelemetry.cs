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

    public static readonly Counter<long> CacheHits =
        Meter.CreateCounter<long>(
            "ai.provider.cache.hits",
            description: "Number of requests that read from the prompt cache");

    public static readonly Counter<long> CacheMisses =
        Meter.CreateCounter<long>(
            "ai.provider.cache.misses",
            description: "Number of requests that populated the prompt cache");

    public static readonly Counter<long> BatchJobsSubmitted =
        Meter.CreateCounter<long>(
            "ai.provider.batch.submitted",
            description: "Number of batch jobs successfully submitted");

    public static readonly Counter<long> BatchJobsCompleted =
        Meter.CreateCounter<long>(
            "ai.provider.batch.completed",
            description: "Number of batch jobs whose results were retrieved");

    public static readonly Histogram<long> BatchResultCount =
        Meter.CreateHistogram<long>(
            "ai.provider.batch.result_count",
            description: "Number of individual results returned per batch retrieval");
}