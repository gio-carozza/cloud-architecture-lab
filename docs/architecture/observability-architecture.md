# Observability Architecture

The observability subsystem of the Lab.Observability.Api gateway.
This document describes the current state. For the reasoning trail
behind these choices, read the linked ADRs.

## Purpose

What problem this subsystem solves, in two sentences. Not the tech —
the operational outcome.

Example:
> The gateway must be diagnosable in production. Operators need to
> answer "which request failed, on which provider, with what latency,
> and is it transient or persistent?" — for any request, in under a minute.

## Pillars

The three signal types this subsystem produces, and what each is for.

- **Logs** — discrete events. Structured. Searchable. Used for
  reconstructing a single request's path.
- **Metrics** — aggregated numbers. Used for SLOs, dashboards, alerts.
- **Traces** — causal chains across components. Used for understanding
  *where* time is spent within a request.

Plus the LLM-specific signals this gateway adds:
- **Tokens in / out per request** — first-class cost telemetry
- **Provider latency vs gateway latency** — where time is actually spent
- **Failure classification** — timeout vs 4xx vs 5xx vs throttle

## End-to-End Flow

```text
Client request
    │
    ▼
CorrelationIdMiddleware
    │  Assigns or accepts X-Correlation-Id; pushes into LogContext
    ▼
ASP.NET Core pipeline
    │  Serilog request logging emits structured access log line
    ▼
AiController → IChatModelProvider
    │
    ▼
ClaudeChatModelProvider        [outer span: ai.chat.complete]
    │  Tags: llm.provider, llm.model
    ▼
ClaudeApiClient                [inner span: claude.chat.api]
    │  Tags: llm.provider, llm.model, llm.endpoint,
    │        llm.tokens.input, llm.tokens.output, llm.latency_ms
    │  Resilience pipeline (timeout, circuit breaker) wraps the call
    ▼
Anthropic API
    │
    ▼
Response or exception
    │  On exception: Activity status set to Error
    │  On success: Activity tags populated, metrics emitted
    ▼
Serilog → OpenTelemetry logger provider → Azure Monitor exporter
    │
    ▼
Application Insights (workspace-based)
    │
    ▼
Operator: KQL queries, dashboards, alerts
```

## Components

### CorrelationIdMiddleware
**Location:** `src/lab-observability-api/Middleware/CorrelationIdMiddleware.cs`
**Responsibility:** Ensures every request has a correlation ID. Accepts
client-provided `X-Correlation-Id`, falls back to `HttpContext.TraceIdentifier`.
Pushes the ID into Serilog's `LogContext` so every log line in the request
scope carries it. Adds the same ID to the response header for client-side
correlation.

**Failure mode:** None — middleware is infallible. If header is missing,
falls back. If push fails, the request still proceeds.

### Serilog (logging library)
**Configured in:** `Program.cs`
**Sinks:** Console (local dev) only. **No** ApplicationInsights sink — see ADR-008.
**Enrichers:** `FromLogContext`, static property `Service=lab-observability-api`.
**Request logging:** `UseSerilogRequestLogging` produces one structured access
log per request with method, path, status, elapsed ms, correlation ID, user agent.

**Why Serilog and not raw ILogger:** `LogContext.PushProperty` propagates
correlation IDs cleanly through async boundaries. Raw ILogger requires
manual scope management at every async hop.

### OpenTelemetry (telemetry pipeline)
**Configured in:** `Program.cs` via `AddOpenTelemetry().UseAzureMonitor()`.
**Captures:** Logs (via the OTel ILogger provider), traces (ASP.NET Core +
HttpClient instrumentation built into the Azure Monitor distro), metrics
(custom `Meter` instances + runtime metrics).
**Exports to:** Application Insights via the connection string in app
setting `APPLICATIONINSIGHTS_CONNECTION_STRING`.

**Why a single pipeline:** Two pipelines emitting to one App Insights
resource produces duplicate request telemetry, conflicting Activity
propagation, and doubled ingestion cost. ADR-008 is the full reasoning.

### GatewayTelemetry (custom signals)
**Location:** `src/lab-observability-api/Telemetry/GatewayTelemetry.cs`
**Provides:**
- `ActivitySource("Lab.Observability.Api")` for custom spans
- `Meter("Lab.Observability.Api")` for custom metrics:
  - `ai.provider.requests` (counter)
  - `ai.provider.failures` (counter)
  - `ai.provider.latency.ms` (histogram)
  - `ai.tokens.input` (counter, future)
  - `ai.tokens.output` (counter, future)

**Why custom signals:** The standard ASP.NET + HttpClient instrumentation
captures the gateway's request lifecycle. It does NOT understand LLM
semantics — token counts, model names, provider identity. Those are
gateway-specific business metrics that need explicit emission.

### Resilience pipeline (Polly via Microsoft.Extensions.Http.Resilience)
**Configured in:** `Program.cs` via `AddStandardResilienceHandler` on
the `ClaudeApiClient` HttpClient.
**Behaviors:**
- Per-attempt timeout: 45 seconds
- Total request timeout: 60 seconds
- Circuit breaker: 20% failure ratio over 120s sampling window; minimum 5 requests; breaks for 15s
- **No retries on chat generation** — see ADR-006 alternatives section
  for why (POST operations to a paid external provider; retries duplicate
  cost and create confusing user-facing behavior). Expressed as
  `MaxRetryAttempts=1` + `ShouldHandle=false` to satisfy v10 validator.

**Failure mode:** When the circuit is open, requests fail fast with
`BrokenCircuitException`. The global exception middleware translates this
to HTTP 503 with a stable `ApiError` shape and the correlation ID.

### Global exception handling
**Location:** `Program.cs` — `UseExceptionHandler`
**Responsibility:** Catches any unhandled exception, logs it with the
correlation ID, returns a stable `ApiError` JSON shape. Never returns
stack traces to clients.

## Configuration

| App setting | Purpose | Where set |
|---|---|---|
| `APPLICATIONINSIGHTS_CONNECTION_STRING` | Tells the Azure Monitor exporter where to send telemetry | App Service env var |
| `Serilog:MinimumLevel:Default` | Minimum log level | `appsettings.json` |
| `Serilog:MinimumLevel:Override:Microsoft.AspNetCore` | Quiet noisy framework logs | `appsettings.json` |

When `APPLICATIONINSIGHTS_CONNECTION_STRING` is empty (local dev without
Azure), `UseAzureMonitor()` is skipped and telemetry stays in-process.
Console logging still works via Serilog.

## Operating This Subsystem

### Where to look first when something is wrong

**Latency spike on `/api/ai/chat`:**
Application Insights → Performance → filter by `cloud_RoleName == "lab-observability-api"`.
Sort by p95. Drill into a slow request, follow the trace to the `claude.chat.api`
span (inner transport span), check `llm.latency_ms` tag — distinguishes provider
latency from gateway overhead captured on the outer `ai.chat.complete` span.

**Error rate climbing:**
Application Insights → Failures → group by `customDimensions.failureType`.
401 = config issue (Anthropic key). 5xx from provider = provider degradation.
`BrokenCircuitException` = circuit open, gateway is shedding load
intentionally.

**Cost surprise:**
Run the token-usage KQL query (see below). If output tokens spike, look
for prompt regressions. If input tokens spike, look for context bloat.

### Starter KQL queries

**Top 10 slowest chat requests in the last hour:**
```kql
requests
| where timestamp > ago(1h)
| where url contains "/api/ai/chat"
| top 10 by duration desc
| project timestamp, duration, resultCode, customDimensions.CorrelationId
```

**Token usage per hour:**
```kql
// Activity spans land in dependencies, NOT traces
dependencies
| where timestamp > ago(24h)
| where name == "claude.chat.api"
| extend inputTokens  = toint(customDimensions["llm.tokens.input"])
| extend outputTokens = toint(customDimensions["llm.tokens.output"])
| summarize total_input = sum(inputTokens), total_output = sum(outputTokens)
  by bin(timestamp, 1h)
| render timechart
```

**Correlate a single request end-to-end:**
```kql
union requests, dependencies, traces, exceptions
| where customDimensions.CorrelationId == "<id-from-response-header>"
| order by timestamp asc
```

### Dashboards (planned, not yet built)
- Gateway health: request rate, error rate, p50/p95/p99 latency
- Provider health: claude.chat span latency, failure rate, circuit state
- Cost: tokens in/out per hour, projected monthly burn

### Alerts
- **[LIVE]** Error rate > 5% for 5 min — `alert-ai-gateway-5xx-rate-dev-eastus-gio`;
  KQL on `requests` table; severity 2; routes to `ag-ai-lab-dev-eastus-gio` → email.
  Bicep: `Infra/Day-006/appinsights.bicep`
- **[planned]** p95 latency > 5s for 10 min
- **[planned]** Circuit breaker opens (any provider)
- **[planned]** Daily token spend > $X

## Known Gaps and Future Work

- **Prompt/response redaction policy:** logs currently must not include
  full prompt bodies; this is enforced by convention, not by code.
  A redaction layer is parking-lot for a future ADR.
- **Sampling strategy:** currently 100% of telemetry is exported. At
  meaningful traffic this becomes expensive. A head-based sampling
  decision is parking-lot.
- **Multi-provider attribution:** when Azure OpenAI / Bedrock / Foundry
  land, the `llm.provider` tag and per-provider metrics already exist.
  Dashboards will need provider-faceted views.
- **Real readiness check:** `/health/ready` currently checks config
  presence, not provider reachability. Adding a live provider ping is
  parking-lot — ping cost vs. signal value tradeoff.

## Decision History

The current architecture is the result of:

- **ADR-006 (2026-04-15)** — Established the broad observability shape:
  correlation IDs, structured errors, OTel + Azure Monitor export,
  resilience pipeline, health endpoints. The Day 6 hardening plan.

- **ADR-008 (2026-04-29)** — Refined the library boundary during
  execution: OTel as the sole telemetry export pipeline; Serilog as a
  logging library inside that pipeline, not a parallel one. Captured a
  pipeline-duplication mistake nearly made when following the original
  Day 6 plan literally.

For *why* this architecture exists, read the ADRs.
For *what* this architecture is, this document is the source of truth.

## References

- `.claude/skills/observability-net/SKILL.md` — implementation patterns
- `docs/architecture/day-006-sequence-flow.md` — request flow diagram
- ADR-005 — provider abstraction this layer wraps
- OpenTelemetry .NET docs: https://opentelemetry.io/docs/languages/net/
- Azure Monitor OpenTelemetry distro: https://learn.microsoft.com/en-us/azure/azure-monitor/app/opentelemetry-enable