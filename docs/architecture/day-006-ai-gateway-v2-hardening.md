# Day 006 — AI Gateway v2 Hardening

## Objective

Move the AI Gateway from a working provider integration to a more production-oriented cloud AI service component.

Day 5 delivered:
- provider abstraction
- Claude-backed endpoint
- Azure deployment
- basic operational success

Day 6 adds:
- resilience
- observability
- health modeling
- correlation
- stable error handling

## Scope

The Day 6 scope includes:

- `ClaudeApiClient` as a dedicated transport client
- `ClaudeProviderException` as a provider-specific failure abstraction
- request correlation middleware
- global exception handling in `Program.cs`
- readiness and liveness endpoints
- Azure Monitor / OpenTelemetry integration
- custom telemetry instruments
- resilience protections on outbound AI calls

## Problem statement

A functional AI endpoint is not yet a production-grade AI gateway.

Without Day 6 hardening, the service has the following gaps:
- difficult to trace individual requests across logs
- external provider failures are not normalized well enough
- provider timeouts and recurring failures are not bounded strongly enough
- readiness and liveness are not modeled separately
- telemetry is insufficient for real operational diagnosis

## Target architecture

### Inbound flow

1. Client sends HTTP request to `/api/ai/chat`
2. Correlation middleware ensures a request correlation ID exists
3. Controller validates and forwards request to the provider abstraction
4. Provider constructs Claude-compatible payload
5. Claude transport client sends the request through a resilience-aware `HttpClient`

### Outbound flow

1. `ClaudeApiClient` sends `POST /v1/messages`
2. Timeout and circuit-breaker protections apply
3. Provider response is interpreted
4. Text content is extracted
5. `ChatResponse` is returned to the caller

### Failure flow

1. Claude returns non-success or transport failure occurs
2. `ClaudeApiClient` throws `ClaudeProviderException`
3. Global exception middleware maps failure to stable API error output
4. Correlation ID is returned with the error for traceability

## Day 6 design themes

### 1. Bounded failure behavior

Provider calls should not fail indefinitely or unpredictably.
Timeouts and circuit breaking provide controlled behavior.

### 2. Centralized operational logic

Provider-specific transport logic belongs in a transport client rather than in the controller or provider abstraction.

### 3. Stable external contract

Consumers should receive structured errors even when the provider fails.

### 4. Azure-native observability

The service should emit logs, metrics, and traces to Azure Monitor/Application Insights when configured.

## Main components

### `AiController`

Responsibility:
- receive API request
- validate prompt presence
- call the provider abstraction
- return `ChatResponse`

### `IChatModelProvider`

Responsibility:
- define the application-level abstraction for sending a chat request

### `ClaudeChatModelProvider`

Responsibility:
- map application request model to Claude request model
- call `ClaudeApiClient`
- shape the `ChatResponse`

### `ClaudeApiClient`

Responsibility:
- own outbound provider HTTP behavior
- classify provider failures
- extract response text
- emit provider-specific telemetry/logging

### `CorrelationIdMiddleware`

Responsibility:
- create or preserve `x-correlation-id`
- store it in request context
- return it in the response
- improve log traceability

### `GatewayTelemetry`

Responsibility:
- define shared activity source
- define counters and histograms for provider telemetry

## Health model

### `/health/live`

Meaning:
- the process is alive
- the application pipeline is running

### `/health/ready`

Meaning:
- the service has required Anthropic configuration present
- the app is prepared to attempt real provider calls

The readiness check intentionally does not call Anthropic on every request.

## Operational outcomes

After Day 6, the service should allow an operator to answer:
- did the request reach the app
- which request failed
- what provider was called
- how long the provider call took
- whether the provider failure was transient
- whether the service is configured and ready

## Portfolio value

Day 6 changes the portfolio story from:
- “I integrated Claude into an API”

to:
- “I designed and hardened an AI gateway with transport isolation, resilience, observability, health modeling, and stable error handling on Azure”

That is a materially stronger architecture narrative.