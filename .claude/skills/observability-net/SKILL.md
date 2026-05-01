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

## The Three Pillars (architect note)
1. **Logs** — discrete events, structured (Serilog → App Insights)
2. **Metrics** — aggregated numbers (latency, throughput, error rate)
3. **Traces** — causal chains across components (W3C Trace Context)

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
<PackageReference Include="Microsoft.Extensions.Http.Resilience" Version="8.*" />
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

In `ClaudeChatModelProvider.SendAsync`:

```csharp
using var activity = ActivitySource.StartActivity("claude.chat");
activity?.SetTag("llm.provider", "anthropic");
activity?.SetTag("llm.model", _options.Model);

var sw = Stopwatch.StartNew();
try
{
    var response = await _httpClient.PostAsync(...);
    activity?.SetTag("llm.tokens.input", response.Usage.InputTokens);
    activity?.SetTag("llm.tokens.output", response.Usage.OutputTokens);
    activity?.SetTag("llm.latency_ms", sw.ElapsedMilliseconds);
    return Map(response);
}
catch (Exception ex)
{
    activity?.SetStatus(ActivityStatusCode.Error, ex.Message);
    throw;
}
```

## Resilience (Polly via Microsoft.Extensions.Http.Resilience)

```csharp
builder.Services.AddHttpClient<ClaudeChatModelProvider>()
    .AddStandardResilienceHandler(opts =>
    {
        opts.Retry.MaxRetryAttempts = 3;
        opts.Retry.UseJitter = true;
        opts.AttemptTimeout.Timeout = TimeSpan.FromSeconds(30);
        opts.CircuitBreaker.FailureRatio = 0.5;
        opts.CircuitBreaker.SamplingDuration = TimeSpan.FromSeconds(30);
    });
```

**Rules:**
- Retry only on transient (5xx, 408, 429 with backoff)
- Never retry on 4xx auth errors (401, 403) — wastes budget
- Circuit breaker prevents cascade failures
- Jitter prevents thundering herd

## KQL Starter Queries (App Insights → Logs)

```kql
// Top 10 slowest /api/ai/chat requests in last hour
requests
| where timestamp > ago(1h)
| where url contains "/api/ai/chat"
| top 10 by duration desc
| project timestamp, duration, resultCode, customDimensions.CorrelationId
```

```kql
// Token usage per hour
traces
| where timestamp > ago(24h)
| where customDimensions.["llm.tokens.output"] != ""
| extend tokens = toint(customDimensions.["llm.tokens.output"])
| summarize total_tokens = sum(tokens) by bin(timestamp, 1h)
| render timechart
```
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