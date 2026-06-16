# ADR-008: Adopt OpenTelemetry-First Observability with Serilog for Request Logging

## Status

Accepted

## Date

2026-04-29

## Context

Day 5 shipped the AI Gateway with OpenTelemetry instrumentation already
wired via `Azure.Monitor.OpenTelemetry.AspNetCore`. Custom metrics and
activities live in `GatewayTelemetry.cs` and `ClaudeApiClient.cs`,
emitting OTel-native signals (`ai.provider.latency.ms`,
`ai.provider.requests`, `ai.provider.failures`).

Day 6 was originally planned around Serilog + the classic Application
Insights SDK (`Microsoft.ApplicationInsights.AspNetCore` +
`Serilog.Sinks.ApplicationInsights`). Executing that plan as written
would have created two parallel telemetry pipelines emitting to the
same Application Insights resource — producing duplicate request
telemetry, conflicting Activity propagation, mixed sampling decisions,
and amplified ingestion cost.

The roadmap also calls for future multi-provider integration (Azure
OpenAI, Amazon Bedrock, Microsoft Foundry). Classic Application Insights
SDK is Azure-specific and in maintenance mode. OpenTelemetry is the
vendor-neutral standard and the direction Microsoft is investing in
for .NET observability.

## Decision

Adopt OpenTelemetry as the single telemetry pipeline for traces, metrics,
and logs. Use Serilog exclusively for structured request logging and log
enrichment (correlation IDs, scope properties), and pipe Serilog's output
through the OpenTelemetry logging provider rather than through a
Serilog-to-AppInsights sink.

The single export path is:

Application code → OpenTelemetry SDK → Azure Monitor exporter → Application Insights (workspace-based)

Serilog is a logging library inside that pipeline, not a parallel one.

## Alternatives Considered

### Alternative 1 — Original Day 6 plan: Serilog + classic AI SDK

Install `Serilog.Sinks.ApplicationInsights` and
`Microsoft.ApplicationInsights.AspNetCore`, remove the existing OTel wiring.

Rejected:

- Throws away working Day 5 instrumentation.
- Locks observability to a single Azure-specific SDK on a deprecation runway.
- Conflicts with future multi-cloud provider goals (Bedrock, Vertex, Foundry).
- Microsoft's strategic direction for .NET observability is OTel-first.

### Alternative 2 — Run both pipelines in parallel

Install everything, accept duplicate telemetry as a tradeoff.

Rejected:

- Duplicate request telemetry breaks every percentile, error rate, and count.
- Two sampling algorithms with no shared state — un-reasonable about drops.
- Doubles ingestion cost.
- This is a broken design, not a tradeoff.

### Alternative 3 — OpenTelemetry only, no Serilog

Use OTel's built-in `ILogger` integration and skip Serilog entirely.

Considered seriously and partially adopted. Serilog earns its keep for:

- `LogContext.PushProperty` enrichment (correlation IDs propagate cleanly through async).
- `UseSerilogRequestLogging` middleware (rich, structured request logs in one line).
- Console formatting during local development.

Serilog adds value as a logging library; it does not need to also be the
export pipeline. This ADR captures that distinction explicitly.

## Consequences

### Positive

- Single telemetry pipeline → no duplicate signals, no cost amplification.
- Vendor-neutral instrumentation → portable to multi-cloud.
- Preserves Day 5 work → no rewrite of `GatewayTelemetry` or `ClaudeApiClient`.
- Future-proof: aligned with Microsoft's strategic direction for .NET observability.
- Rich logging ergonomics via Serilog without the export coupling.

### Negative

- Slightly less canonical Serilog wiring (no `WriteTo.ApplicationInsights`).
- Requires understanding the boundary between "logging library" and
  "telemetry export pipeline" — this is a real concept that some
  developers conflate.

### Neutral / Tradeoffs

- Some classic AI SDK conveniences (`TelemetryClient.TrackCustomEvent`,
  `TrackDependency`) are not directly available. They are replaced by
  `Activity` events and `Meter` counters, which the codebase already uses.

## Implementation Notes

Files affected:

- `lab-observability-api.csproj` — add `Serilog.AspNetCore`,
  `Serilog.Settings.Configuration`, `Serilog.Sinks.Console`, and
  `OpenTelemetry.Extensions.Hosting`.
- `Program.cs` — wire Serilog as the host logger, route Serilog output
  through the OTel logging provider, add `UseSerilogRequestLogging`.
- `Middleware/CorrelationIdMiddleware.cs` — already correct from Day 5;
  enrichment via `LogContext.PushProperty` added in Day 6.
- No changes to `GatewayTelemetry.cs` or `ClaudeApiClient.cs` — Day 5
  OTel instrumentation is preserved verbatim.

NOT installed (explicitly avoided to prevent pipeline duplication):

- `Microsoft.ApplicationInsights.AspNetCore`
- `Serilog.Sinks.ApplicationInsights`

## References

- ADR-006 (AI Gateway resilience and observability hardening — this ADR refines its implementation)
- ADR-005 (Provider abstraction)
- `docs/notes/Day-006/01-summary.md`
- `.claude/skills/observability-net/SKILL.md` (must be updated to reflect
  this decision; the original guidance assumed classic AI SDK)
- OpenTelemetry .NET docs: <https://opentelemetry.io/docs/languages/net/>
- Azure Monitor OpenTelemetry distro: <https://learn.microsoft.com/en-us/azure/azure-monitor/app/opentelemetry-enable>
