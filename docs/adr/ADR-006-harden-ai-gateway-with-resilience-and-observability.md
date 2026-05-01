# ADR-006: Harden AI Gateway with Resilience and Observability

- Status: Accepted
- Date: 2026-04-15
- Related:
  - ADR-004-first-workload-on-app-service-with-app-insights.md
  - ADR-005-introduce-provider-abstraction-for-claude-integration.md

## Context

Day 5 introduced the first functional AI Gateway version by exposing a Claude-backed chat endpoint through the `Lab.Observability.Api` application.

The Day 5 implementation proved:
- provider abstraction works
- Anthropic configuration binding works
- local secrets and Azure App Service configuration work
- the live endpoint can be deployed and invoked successfully

However, the Day 5 implementation was still closer to a functional prototype than a production-oriented AI platform component.

The next architectural concern is operational maturity.

The gateway must now support:
- request tracing and correlation
- structured failure handling
- basic health modeling
- bounded provider call behavior
- resilience for transient faults
- Azure-native telemetry export for diagnostics and operations

## Decision

We will harden the AI Gateway by introducing the following Day 6 design decisions:

1. Use a dedicated `ClaudeApiClient` as the transport boundary for outbound Anthropic communication.
2. Keep `ClaudeChatModelProvider` focused on provider orchestration and response mapping rather than raw HTTP transport details.
3. Introduce correlation IDs through custom middleware so requests can be traced consistently across logs and responses.
4. Introduce a standard API error contract for stable downstream error handling.
5. Add liveness and readiness endpoints with configuration-based readiness checks.
6. Use OpenTelemetry with Azure Monitor export for logs, traces, and metrics.
7. Add resilience behavior on the Claude client with timeout and circuit breaker protection.
8. Avoid automatic retries for Claude chat generation flows because the operation is a `POST` to an external AI provider and may have cost and duplication implications.

## Rationale

This decision improves the system in four major ways:

### 1. Reliability

AI providers are external dependencies and should be treated as fault domains.
The application must behave predictably when the provider is slow, unavailable, or returns transient failures.

### 2. Observability

A production-grade AI service must be diagnosable.
It is not enough for a request to succeed or fail; operators must be able to answer:
- which request failed
- which provider failed
- what the latency was
- whether the issue was transient or persistent
- how often failures are occurring

### 3. Separation of concerns

The provider class should not own all transport and operational logic.
Transport concerns belong in a client layer.
Provider orchestration belongs in the provider layer.
Endpoint concerns belong in controllers.

### 4. Enterprise readiness

A cloud architect should design for:
- supportability
- diagnosability
- operational review
- incident triage
- future multi-provider expansion

Day 6 shifts the project from a “working demo” toward a supportable AI platform component.

## Consequences

### Positive

- Clearer transport boundary
- Better logs and traceability
- Stable error shapes
- Safer handling of provider faults
- Better production-readiness story for portfolio and interview discussion
- Stronger alignment to Azure architecture and AI platform design

### Negative

- More code and operational complexity
- More moving parts during local debugging
- More configuration to maintain
- Greater need for version alignment across resilience and telemetry packages

## Alternatives considered

### 1. Keep all Claude logic inside the provider class

Rejected because it mixes:
- HTTP transport
- provider orchestration
- error classification
- operational concerns

This would make the provider harder to test, reason about, and extend.

### 2. Add retries to Claude chat calls

Rejected for Day 6 because chat generation is a `POST` operation and automatic retries may:
- duplicate requests
- increase cost
- create confusing user-facing behavior

### 3. Use only controller-level try/catch

Rejected because global exception handling provides:
- more consistent API error responses
- a cleaner controller
- better centralization of failure mapping

## Implementation notes

Day 6 introduces or standardizes these files and responsibilities:

- `Program.cs`
  - dependency registration
  - telemetry registration
  - resilience pipeline registration
  - global exception handling
  - health endpoints

- `Middleware/CorrelationIdMiddleware.cs`
  - correlation ID generation and propagation

- `Contracts/ApiError.cs`
  - stable error contract

- `Services/Claude/ClaudeApiClient.cs`
  - outbound Anthropic HTTP transport
  - provider-specific error interpretation

- `Services/AI/ClaudeChatModelProvider.cs`
  - provider orchestration
  - request-to-payload mapping
  - response shaping

- `Telemetry/GatewayTelemetry.cs`
  - custom metrics and activity source

## Follow-up

Potential Day 7+ extensions:
- centralized prompt/response redaction policy
- request/response size limits
- authentication and authorization
- rate limiting at the API edge
- multi-provider routing
- cost-aware telemetry dimensions
- Azure dashboard and alert rules as code

## Errata

ADR-008 (2026-04-29) refines the observability implementation decision in this ADR.
Serilog is positioned as a logging library inside the OpenTelemetry export pipeline,
not as a parallel sink to Application Insights. The OTel-first export architecture
remains unchanged; ADR-008 makes the Serilog placement explicit.
See: ADR-008-adopt-opentelemetry-first-observability-with-serilog-request-logging.md