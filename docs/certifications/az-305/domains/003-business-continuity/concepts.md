# Concepts — Design Business Continuity Solutions (AZ-305 Domain 3)

---

<!-- Day 6 Additions: retry + jitter, circuit breaker, timeout, Microsoft.Extensions.Http.Resilience -->

## Retry with Exponential Backoff and Jitter

### If you're 10 years old

Imagine you're trying to call a friend but the line is busy. If you immediately redial every second, you and 100 other callers all redial at the same second and the line stays jammed. "Jitter" means you wait a random extra second before redialing — so callers spread out and the line clears faster. Exponential backoff means waiting a little longer each time you try (1s, 2s, 4s, 8s...) so you're not constantly hammering a struggling service.

### If you're a CEO

A system that retries failed calls without a delay can make an overloaded service worse — all callers pile back in at the same instant and re-saturate it immediately. Retry with jitter spreads retries across time, so a temporary spike doesn't become a cascade failure. This is the difference between a 30-second blip and a 15-minute outage.

### If you're an Engineer

Implement with `Microsoft.Extensions.Http.Resilience`: call `AddStandardResilienceHandler()` on a named or typed `IHttpClientFactory` registration. By default it wires 3 retries with `ExponentialWithJitter` backoff. Critical rule: do NOT retry on 4xx client errors (400, 401, 403, 404, 422) — they indicate a bad request or auth failure that retrying cannot fix. Only retry on 429 (honoring the `Retry-After` header), 5xx (transient server errors), and network failures (`IOException`, `TaskCanceledException`). Log the attempt number and jitter delay on each retry as structured events for debugging. Raw Polly configuration: `new RetryStrategyOptions { MaxRetryAttempts = 3, BackoffType = DelayBackoffType.Exponential, UseJitter = true }`.

### If you're an Architect

Retry policy design has three governing constraints: (1) **idempotency** — only retry operations that are safe to repeat (GET, PUT with the same payload); never blindly retry a POST that creates a resource unless the server guarantees idempotent handling via an idempotency key; (2) **error classification** — retrying on 401/403 burns attempts on a request that will never succeed; retrying on 429 must honour the `Retry-After` header to avoid worsening rate-limit pressure; (3) **retry amplification** — in a service mesh, if Service A retries 3× and it calls Service B which also retries 3×, a single user request can generate 9 upstream calls. Each tier's retry policy must account for the total fan-out. At enterprise scale, the canonical approach is: retries at the edge (API gateway), resilience pipelines within each service, and no retries at the infrastructure layer (load balancer) to avoid amplification. Common beginner mistake: retrying on all non-2xx responses, causing a 401 (invalid API key) to burn all retry attempts before returning an error that will repeat on every request forever.

---

## Circuit Breaker Pattern

### If you're 10 years old

In your house, if a fuse blows, it doesn't keep trying to send electricity through the broken wire — it stops, prevents a fire, and waits for someone to fix it. A software circuit breaker does the same thing: if too many requests to a service are failing, it "opens" and stops sending new requests for a while, letting the struggling service recover instead of drowning it in even more traffic.

### If you're a CEO

Without a circuit breaker, when a dependency (like an AI provider or payment service) goes down, your system keeps hammering it with requests, slowing your own system down as threads pile up waiting for responses that will never come. A circuit breaker detects the pattern of failures, "opens" the circuit for a short cooldown, and lets both systems recover. The business result: a 30-second outage in one dependency doesn't cascade into a 10-minute degradation in your own service.

### If you're an Engineer

`Microsoft.Extensions.Http.Resilience` includes a circuit breaker in the standard resilience pipeline. Configurable parameters: `FailureRatio` (e.g., 0.5 — opens when 50% of calls in the sampling window fail), `SamplingDuration` (30s window), `MinimumThroughput` (minimum calls before evaluation — prevents opening on cold-start outliers), `BreakDuration` (15–30s half-open probe interval). States: **Closed** (normal operation) → **Open** (fail fast, no upstream calls) → **Half-Open** (probe request allowed) → back to **Closed** if probe succeeds. In Application Insights, log circuit breaker state transitions as structured events (`"event":"circuit_breaker_opened"`, `"provider":"anthropic"`) so on-call engineers see the transition in KQL.

### If you're an Architect

The circuit breaker prevents cascade failures across service boundaries. The three-state model (Closed / Open / Half-Open) is the standard pattern (described in Nygard's *Release It!* and formalised in Azure's cloud design patterns). Key design decisions: (1) **scope** — one breaker per downstream resource (one for Anthropic, a separate one for Azure OpenAI) so a single provider failure doesn't affect independent providers; (2) **failure classification** — increment the failure counter only on transient errors (5xx, network timeout), not on client errors (4xx); a flood of 400 bad requests should not open the circuit; (3) **half-open probe strategy** — allow a single probe request; if it succeeds, close the circuit; if it fails, re-open for another break duration; (4) **observability** — alert on circuit breaker open events, not just on error rates, because the circuit opening is the actionable signal. At enterprise scale, circuit breakers are complemented by bulkhead isolation (separate thread pools per dependency) and timeout policies. Common beginner mistake: setting the failure ratio without a minimum throughput, causing the circuit to open after a single cold-start failure at low traffic hours.

---

## Attempt Timeout vs. Overall Timeout

### If you're 10 years old

If you're waiting for a pizza delivery, you might decide "if it's not here in 30 minutes, I'll cancel and call somewhere else." That's a timeout. But if the first delivery driver gets stuck in traffic, you might wait a bit, then send a second driver. An "attempt timeout" is how long each driver gets before you give up on that specific driver. An "overall timeout" is how long you'll wait for any driver before cancelling the whole order.

### If you're a CEO

Timeouts are what prevent a slow dependency from bringing your whole system down. Without them, threads pile up waiting for responses that may never arrive, eventually exhausting your server's capacity to handle any request. Two timeouts — attempt (per retry) and overall (per request) — give architects precise control over how long any one user waits, regardless of how many retries happen in between.

### If you're an Engineer

With `Microsoft.Extensions.Http.Resilience`, the standard pipeline includes two timeout policies: `AttemptTimeout` (applied per retry attempt — default 30s) and an outer `TotalRequestTimeout` (applied to the entire request including all retries — default 30s × max attempts). Set `AttemptTimeout` below the upstream SLA to guarantee a retry can start within the total budget. Example: 3 retries × 10s attempt timeout = 30s max for three attempts, well within a 45s total timeout. Configure: `AddStandardResilienceHandler().Configure(options => { options.AttemptTimeout.Timeout = TimeSpan.FromSeconds(15); })`. Never set attempt timeout without a total timeout — without the outer boundary, retry amplification can hold a request for minutes.

### If you're an Architect

The attempt/overall timeout separation bounds latency at two levels of granularity. The constraint: the **overall timeout must be less than the caller's SLA** (to leave time for the response to travel back and be processed); the **attempt timeout must be short enough to allow at least two retries within the overall budget**. This is the "retry budget" concept: `overall_timeout > (max_attempts × attempt_timeout) + jitter_headroom`. For AI gateway workloads, provider latency varies significantly — set attempt timeout based on observed p95 provider latency plus headroom, not an arbitrary default. Log both timeout dimensions on retry events: `attempt_timeout_ms` and `total_elapsed_ms` help on-call engineers distinguish "provider is slow" (attempt timeouts) from "we're retrying too much" (overall timeout hit). Common beginner mistake: setting a single global `HttpClient.Timeout` without per-attempt granularity, which prevents retries from starting before the total deadline expires.

---
