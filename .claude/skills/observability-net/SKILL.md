---
name: observability-net
description: Add structured logging, distributed tracing, correlation IDs, and metrics to Lab.Observability.Api using Serilog and Azure Application Insights. Use during Day 6 (observability & resilience) and any time the user asks about logging, tracing, telemetry, monitoring, or App Insights.
allowed-tools: Read, Write
---

# Observability for .NET 8 AI Gateway

## When to use

- Day 6 work (observability & resilience)
- Adding logging, tracing, or metrics
- Wiring up Application Insights
- Implementing correlation IDs
- KQL query questions
- Before advising on Azure Monitor / Application Insights / OpenTelemetry package versions or wiring, verify against Microsoft Learn via the MCP. Package version transitivity is a known failure class (see graveyard, Day 6 OTel 1.14.0). MS Learn covers the Azure exporter side; it does NOT cover Anthropic SDK behavior.

## The Three Pillars

1. **Logs** — discrete events, structured (Serilog → App Insights)
2. **Metrics** — aggregated numbers (latency, throughput, error rate)
3. **Traces** — causal chains across components (W3C Trace Context)

**AI Engineer:** knows what to log (token counts, model ID, latency, errors) and how to wire the SDK
**Forward-Deployed Engineer:** knows what to *show* the customer — dashboards that prove the AI is working and what it's costing
**LLM Architect:** knows how to *govern* it — retention policies, cost attribution by workload class, alert rules that page before the bill arrives

For an LLM gateway, you ALSO care about:

- **Tokens in / out** per request (cost telemetry)
- **Provider latency** vs **gateway latency** (where time is spent)
- **Cache hit rate** (when prompt caching lands)
- **Failure classification** (timeout vs 4xx vs 5xx vs throttle)

Elite AI architects treat token cost as a first-class metric, not an afterthought.

## NuGet Packages (Day 6+)

Per ADR-006, the observability stack is OpenTelemetry-first with Serilog
used only as a logging library (NOT as a telemetry export sink).

```xml
<PackageReference Include="Serilog.AspNetCore" Version="8.0.*" />
<PackageReference Include="Microsoft.Extensions.Http.Resilience" Version="10.*" />
```

## DO NOT INSTALL these packages

Per ADR-006, these would create a parallel telemetry pipeline and
duplicate every signal:

- `Microsoft.ApplicationInsights.AspNetCore`
- `Serilog.Sinks.ApplicationInsights`

## Program.cs Wiring (OTel-first, Serilog as logger)

```csharp
using Serilog;

// Bootstrap logger — captures startup errors before host is built
Log.Logger = new LoggerConfiguration()
    .WriteTo.Console()
    .CreateBootstrapLogger();

try
{
    var builder = WebApplication.CreateBuilder(args);

    // Replace default logging with Serilog; pipe to OpenTelemetry
    builder.Host.UseSerilog((ctx, services, cfg) => cfg
        .ReadFrom.Configuration(ctx.Configuration)
        .ReadFrom.Services(services)
        .Enrich.FromLogContext()
        .Enrich.WithProperty("Service", "lab-observability-api")
        .WriteTo.Console()
        // No WriteTo.ApplicationInsights here — OTel handles the export.
    );

    // OpenTelemetry remains the single export pipeline.
    var aiConn = builder.Configuration["APPLICATIONINSIGHTS_CONNECTION_STRING"];
    var otel = builder.Services.AddOpenTelemetry();
    if (!string.IsNullOrWhiteSpace(aiConn))
    {
        otel.UseAzureMonitor();
    }

    // ... custom sources, meters, etc. (preserved from Day 5)

    var app = builder.Build();

    app.UseSerilogRequestLogging(opts =>
    {
        opts.MessageTemplate =
            "HTTP {RequestMethod} {RequestPath} {StatusCode} in {Elapsed:0}ms";
        opts.EnrichDiagnosticContext = (diag, http) =>
        {
            diag.Set("CorrelationId", http.Items["x-correlation-id"]?.ToString());
            diag.Set("UserAgent", http.Request.Headers.UserAgent.ToString());
        };
    });

    // ... rest of pipeline
    app.Run();
}
catch (Exception ex)
{
    Log.Fatal(ex, "Application terminated unexpectedly during startup");
}
finally
{
    Log.CloseAndFlush();
}
```

**Why this works as one pipeline:**

- Serilog writes to its own sinks (Console).
- The default ASP.NET Core `ILoggerFactory` is replaced by Serilog via `UseSerilog`.
- `AddOpenTelemetry()` registers OTel's `ILoggerProvider`, which captures
  the host's `ILogger` output and exports it to Azure Monitor.
- Net effect: one log line emitted by code → Serilog formats it for Console
  AND the OTel logger provider exports it to Application Insights.

## Correlation ID Pattern

ASP.NET Core gives you `HttpContext.TraceIdentifier` for free. Promote it:

```csharp
public class CorrelationIdMiddleware
{
    private const string Header = "X-Correlation-Id";
    public async Task InvokeAsync(HttpContext ctx, RequestDelegate next)
    {
        var id = ctx.Request.Headers[Header].FirstOrDefault()
                 ?? ctx.TraceIdentifier;
        ctx.TraceIdentifier = id;
        ctx.Response.Headers[Header] = id;
        using (LogContext.PushProperty("CorrelationId", id))
        {
            await next(ctx);
        }
    }
}
```

Pass it through to provider calls so Anthropic logs can be correlated to your gateway logs.

## LLM-Specific Telemetry

Use two nested spans — tag where the data naturally lives:

**Outer span** (`ai.chat.complete`) in `ClaudeChatModelProvider` — orchestration data only:

```csharp
using var activity = GatewayTelemetry.ActivitySource.StartActivity("ai.chat.complete");
activity?.SetTag("llm.provider", "anthropic");
activity?.SetTag("llm.model", _options.Model);
try
{
    var result = await _claudeApiClient.SendChatAsync(payload, ct);
    return result;
}
catch (Exception ex)
{
    activity?.SetStatus(ActivityStatusCode.Error, ex.Message);
    throw;
}
```

**Inner span** (`claude.chat.api`) in `ClaudeApiClient` — transport data (token counts, latency, endpoint):

```csharp
using var activity = GatewayTelemetry.ActivitySource.StartActivity("claude.chat.api");
activity?.SetTag("llm.provider", "anthropic");
activity?.SetTag("llm.model", _options.Model);
activity?.SetTag("llm.endpoint", _options.BaseUrl.TrimEnd('/') + "/messages");
var sw = Stopwatch.StartNew();
try
{
    // ... HTTP call, parse response ...
    var (inputTokens, outputTokens, cacheReadTokens, cacheCreationTokens) = TryExtractUsage(responseBody);
    if (inputTokens.HasValue) activity?.SetTag("llm.tokens.input", inputTokens.Value);
    if (outputTokens.HasValue) activity?.SetTag("llm.tokens.output", outputTokens.Value);
    if (cacheReadTokens.HasValue) activity?.SetTag("llm.cache.read_tokens", cacheReadTokens.Value);
    if (cacheCreationTokens.HasValue) activity?.SetTag("llm.cache.creation_tokens", cacheCreationTokens.Value);
    activity?.SetTag("llm.latency_ms", sw.Elapsed.TotalMilliseconds);
}
catch (Exception ex)
{
    activity?.SetStatus(ActivityStatusCode.Error, ex.Message);
    activity?.SetTag("llm.latency_ms", sw.Elapsed.TotalMilliseconds);
    throw;
}
```

**Why two spans:** token counts come from the Anthropic response payload, which only `ClaudeApiClient` sees. Surfacing them on the outer span would require coupling layers. Tag where the data lives; nested spans give the full operational story.

**Where spans land in App Insights:** both appear in the `dependencies` table, NOT `requests` or `traces`. The outer span is the parent; the inner span is a child dependency within it.

## Resilience (Polly via Microsoft.Extensions.Http.Resilience v10)

```csharp
builder.Services.AddHttpClient<ClaudeApiClient>()
    .AddStandardResilienceHandler(opts =>
    {
        // Per-attempt timeout — single HTTP call cannot exceed this
        opts.AttemptTimeout.Timeout = TimeSpan.FromSeconds(45);

        // Total request timeout — must be >= AttemptTimeout
        opts.TotalRequestTimeout.Timeout = TimeSpan.FromSeconds(60);

        // Circuit breaker — SamplingDuration MUST be >= 2x AttemptTimeout (v10 invariant)
        opts.CircuitBreaker.SamplingDuration = TimeSpan.FromSeconds(120);
        opts.CircuitBreaker.FailureRatio = 0.20;
        opts.CircuitBreaker.MinimumThroughput = 5;
        opts.CircuitBreaker.BreakDuration = TimeSpan.FromSeconds(15);

        // No retries on chat POST — non-idempotent, paid, confusing UX if silently retried.
        // v10 rejects MaxRetryAttempts=0; express intent as ShouldHandle=false instead.
        // Day 7: replace with classification-based predicate (retry 429/503/504, not 401/403).
        opts.Retry.MaxRetryAttempts = 1;
        opts.Retry.ShouldHandle = _ => ValueTask.FromResult(false);
    });
```

**v10 validator rules (startup will throw if violated):**

- `SamplingDuration >= 2 × AttemptTimeout` — mathematical invariant enforced at startup
- `MaxRetryAttempts >= 1` — use `ShouldHandle = _ => false` to express "no retries"

**Design rules:**

- Never retry on chat POST — non-idempotent, paid call; caller may have already shown an error
- Never retry 401/403 — auth failures are not transient; retrying burns quota
- Timeout + circuit breaker are the right resilience shape for LLM calls without retry classification

## KQL Starter Queries

See `docs/standards/kql-cookbook.md` — all gateway queries live there.
Key note: Activity spans land in the `dependencies` table, NOT `requests` or `traces`.

## Logging Field Conventions

   Standard structured fields used across the gateway:

- `provider` — name of the LLM provider (e.g., "anthropic")
- `model` — model identifier
- `endpoint` — relative API path
- `statusCode` — HTTP status code
- `durationMs` — operation duration in milliseconds
- `requestId` / `correlationId` — request correlation identifier
- `environment` — deployment environment (dev/test/prod)

## What NOT to log

- Raw API keys or secrets
- Full prompt bodies (PII risk, cost)
- Full response bodies in production

## Common mistakes (avoid)

- Logging full prompt bodies (PII, secrets, cost)
- Retrying 401s (waste, alarms)
- Forgetting jitter on retries (synchronized retry storms)
- Treating App Insights as "logs only" — use metrics & traces too
- Not propagating correlation IDs to downstream calls
