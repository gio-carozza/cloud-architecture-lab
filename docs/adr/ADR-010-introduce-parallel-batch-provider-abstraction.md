# ADR-010: Introduce a Parallel Batch Provider Abstraction Rather Than Extend the Chat Seam

## Status

Accepted

## Date

2026-06-03

## Related

- ADR-005-introduce-provider-abstraction-for-claude-integration.md (the interactive seam this decision deliberately does *not* extend)
- ADR-009-implement-prompt-caching-inside-provider-boundary.md (the inverse case — same test, opposite answer)
- ADR-006-harden-ai-gateway-with-resilience-and-observability.md (why batch needs its own resilience/alerting profile)
- ADR-008-adopt-opentelemetry-first-observability-with-serilog-request-logging.md (telemetry pattern reused with batch-specific span names)

## Context

Day 5 (ADR-005) established `IChatModelProvider` as the single seam for all
LLM calls, with a strictly synchronous request/response contract:

```csharp
public interface IChatModelProvider
{
    Task<ChatResponse> SendAsync(ChatRequest request, CancellationToken ct);
    string ProviderName { get; }
}
```

One prompt in, one completion out, on the interactive path. `ChatRequest` and
`ChatResponse` are provider-agnostic and have been protected from provider-specific
leakage since Day 5.

Day 7 (ADR-009) added Anthropic prompt caching *inside* that seam, on YAGNI
grounds. The justification was that caching is a transport-layer optimization
that does **not** change the operation set or the substitutability contract:
`SendAsync` is unchanged, `ChatResponse` is unchanged, a caching-enabled Claude
provider remains fully substitutable for any other `IChatModelProvider`, and
cache status is observable only via telemetry. No new abstraction was warranted.

Day 8 adds the Anthropic Message Batches API for **offline prompt evaluation and
regression testing** — an offline workload where many `ChatRequest`s are scored
or summarized without an interactive user waiting on each one.

Batch is a fundamentally different interaction model:

- **Job-shaped, not call-shaped.** Submit a set of N requests → poll for status
  → retrieve results later. There is no single `ChatResponse` to return synchronously.
- **Latency in minutes to hours**, not seconds. "Still in progress" is the normal,
  expected state — not a failure, not an alert condition.
- **~50% input/output cost reduction** versus the synchronous endpoint, which is
  the entire economic reason the workload uses batch rather than fanning out
  interactive calls.

The architectural question Day 8 must answer is **not** whether to integrate batch
(the cost case is unambiguous for offline workloads). It is *where in the
architecture batch lives*, and whether that placement is consistent with the
reasoning that put caching inside the seam on Day 7.

### The test being applied (made explicit, because the ADR-009 symmetry depends on it)

The seam abstracts the **operation set** and the **substitutability contract**,
not the implementation. A change earns a *new* abstraction when it alters either:

| Change | New operations? | Synchronous `ChatResponse`? | Substitutable for a provider lacking the feature? | Verdict |
|---|---|---|---|---|
| Prompt caching (Day 7) | No | Yes | Yes | Inside the seam |
| Batch (Day 8) | Yes (submit/poll/retrieve) | No | No — would require `NotSupportedException` | New seam |

"Interaction model changed" is the *symptom*. The load-bearing reason batch needs
its own seam is **Interface Segregation + Liskov substitutability**: forcing batch
methods onto `IChatModelProvider` would (a) require any provider that cannot batch
to throw at runtime, breaking substitutability, and (b) force interactive-path
clients to depend on operations they never call. Caching triggered neither
condition; batch triggers both. Same test, opposite answer — which is why the
009/010 symmetry is principled rather than a post-hoc tidiness story.

## Decision

We will introduce a **parallel** batch abstraction, sibling to (not derived from
or bolted onto) `IChatModelProvider`:

```csharp
public interface IBatchChatModelProvider
{
    Task<BatchJob>                  SubmitBatchAsync(IReadOnlyList<ChatRequest> requests, CancellationToken ct);
    Task<BatchJobStatus>            GetBatchStatusAsync(string batchJobId, CancellationToken ct);
    Task<IReadOnlyList<BatchResult>> GetBatchResultsAsync(string batchJobId, CancellationToken ct);
    string ProviderName { get; }
}
```

Specifically:

- `ChatRequest` is **reused as the element type** of a batch. A request authored
  for the interactive path can be submitted to batch unchanged. This is a feature,
  not contract pollution: `ChatRequest` is not modified and gains no batch-specific
  fields. (Reusing `ChatRequest` as a list element is *not* the same as Alternative
  3, which overloads `ChatRequest`/`ChatResponse` with batch *semantics*.)
- New batch-shaped contracts live in `Models/`: `BatchJob`, `BatchJobStatus`
  (enum + per-status counts), `BatchResult` (`custom_id` + a `ChatResponse`-or-error).
- `ClaudeBatchChatModelProvider` implements `IBatchChatModelProvider` and **shares
  per-message payload construction** with `ClaudeChatModelProvider` via the common
  `ClaudeApiClient`/payload builder. Shared internals, separate seams — this closes
  the only legitimate objection to splitting the interface (drift between paths).
- `IChatModelProvider`, `ChatRequest`, `ChatResponse`, and `ClaudeChatModelProvider`
  are **not modified**.

### Scope discipline (consistent with ADR-009's named-forward-path)

Day 8 scopes to the **provider transport seam only** — the provider-specific
ability to submit a batch, poll its status, and retrieve its results. The **job
orchestration layer** (durable persistence of job IDs, a polling scheduler,
result storage, per-item retry of failed requests) is an application concern that
does **not** vary by provider. It is named here as forward work and is explicitly
**not** in Day 8 scope.

Accepted consequence of that scoping: until orchestration lands, Day-8 batch is
"submit, then manually (or via an ad-hoc caller) poll and retrieve." That is
adequate for the first offline workload and must not be mistaken for production
batch infrastructure.

When a second provider with batch semantics is added (OpenAI Batch, Bedrock batch
inference), `IBatchChatModelProvider` gains a second implementation — exactly as
the interactive seam does. The orchestration layer is built when a real workload
needs unattended, durable batch execution, not before.

## Alternatives Considered

### Alternative 1 — Parallel batch provider abstraction, provider-seam-scoped

**This is the chosen alternative. See Decision above.**

**Why chosen:**

- The operation set and substitutability contract both change (see the test table
  in Context). A sibling interface is the honest shape: interactive and batch are
  *siblings*, not variants of one interface.
- Reusing `ChatRequest` as the batch element type keeps request authoring shared
  and the provider-agnostic contract intact.
- Sharing implementation internals (`ClaudeApiClient`/payload builder) neutralizes
  the only real argument for unification (cohesion/drift — see Alternative 2).

**Consequences accepted:** two interfaces, two DI registrations, two resilience
and telemetry wirings. The orchestration gap is deferred (see Scope discipline).

### Alternative 2 — Extend `IChatModelProvider` with batch methods (`SubmitBatchAsync`, etc.)

One interface, two interaction models.

**The real point in its favor (conceded):** *cohesion.* A single Claude provider
supports both modes; splitting into independent classes risks drift — the batch
and interactive paths building subtly different per-message payloads, diverging on
auth or model ID.

**Why rejected anyway:**

- That concern is about **implementation reuse**, not **interface unification**,
  and this alternative conflates the two. The cohesion concern is fully satisfied
  under Alternative 1 by sharing internals (one `ClaudeApiClient`/payload builder
  feeding both providers). You do not need a unified interface to get a unified
  payload path.
- The Liskov cost is unavoidable here: any `IChatModelProvider` that cannot batch
  (a future lightweight or interactive-only provider) must throw
  `NotSupportedException` on the batch methods. Substitutability is broken at the
  type level.
- ISP cost: interactive-path consumers (`AiController`) would depend on batch
  operations they never invoke.

**Revisit conditions:** none foreseen. Cohesion is the only motivation and it is
better served by shared internals under Alternative 1.

### Alternative 3 — Overload `ChatRequest`/`ChatResponse` with batch semantics

Add a mode flag and a nullable job ID so the existing contracts carry both
interactive and batch meaning.

**Why rejected:**

- This is the identical anti-pattern ADR-009 rejected in *its* Alternative 3:
  polluting the provider-agnostic contract with operational concerns. `ChatRequest`
  and `ChatResponse` are the stable public shape, meant to be invariant across
  providers and time.
- A nullable job ID on `ChatResponse` breaks the type's invariant — "a
  `ChatResponse` is a completion" — and forces every consumer to branch on whether
  it received an answer or a handle. Null-checking a job ID on a response object is
  a code smell that propagates to every caller.
- Note the distinction from Alternative 1: *reusing* `ChatRequest` as a batch
  element type is clean and intended; *overloading* it with a mode flag is not.

**Revisit conditions:** none.

### Alternative 4 — Build the full batch orchestration layer now

Provider seam **plus** durable job persistence, a polling scheduler, a result
store, and per-item retry — all on Day 8.

**Why rejected:**

- YAGNI, and inconsistent with the restraint ADR-009 exercised on the decorator.
  The first batch workload needs the transport seam; it does not yet need
  unattended, durable, multi-tenant batch execution.
- The orchestration layer's correct shape (persistence model, scheduling strategy,
  retry policy) will be informed by the first one or two real workloads. Designing
  it against an assumed workload risks fitting it to assumptions that turn out wrong
  — the same trap ADR-009 named for the `ICacheAnnotator` interface.

**Revisit conditions:** a workload requires unattended, durable batch execution
(jobs that must survive process restarts, automatic result collection, SLA-bound
completion). At that point orchestration is designed against real requirements.

## Consequences

### Positive

- The interactive contract protected since Day 5 is untouched. `IChatModelProvider`,
  `ChatRequest`, `ChatResponse` are unchanged.
- Batch gets a contract that fits its actual shape (job-oriented, asynchronous)
  rather than being forced through a synchronous seam.
- ~50% cost reduction available for offline workloads.
- `ChatRequest` reuse means request authoring is shared across interactive and batch.
- Resilience and observability can be tuned to batch's real profile instead of
  inheriting interactive thresholds that are wrong for it.

### Negative

- Two provider interfaces to maintain.
- Risk of implementation drift between interactive and batch payload construction.
  **Mitigation (mandatory):** both providers route per-message payload construction
  through one shared `ClaudeApiClient`/payload builder. If this mitigation is
  skipped, Alternative 2's cohesion objection becomes real.
- DI, telemetry, and resilience must be wired a second time for the batch path.
- The orchestration gap is deferred: Day-8 batch is "submit and manually retrieve"
  until the orchestration layer is built. This must be communicated, not assumed
  to be production batch infrastructure.

### Neutral / Tradeoffs

- `ChatRequest` is intentionally coupled across interactive and batch (shared element
  type). If the two ever need to diverge (e.g., batch-only metadata), that is a
  future ADR, not a reason to fork the type now.
- Telemetry reuses the Day 6/8 Activity + Meter patterns but with **new span names**
  (`batch.submit`, `batch.poll`, `batch.retrieve`) and **different latency semantics**.
  A long-running batch is normal; the interactive `alert-ai-gateway-5xx-rate` and
  latency thresholds **must not** be applied to batch spans. An `in_progress` poll is
  a 200, not a failure — the circuit breaker and 5xx alerting must not treat it as one.

## Implementation Notes

### New files

- `src/lab-observability-api/Services/AI/IBatchChatModelProvider.cs` — the sibling seam.
- `src/lab-observability-api/Services/AI/ClaudeBatchChatModelProvider.cs` — implementation;
  shares payload construction with `ClaudeChatModelProvider`.
- `src/lab-observability-api/Models/BatchJob.cs` — job id, status, created/expires timestamps.
- `src/lab-observability-api/Models/BatchJobStatus.cs` — enum (validating / in_progress /
  completed / canceled / expired / failed) plus per-status request counts.
- `src/lab-observability-api/Models/BatchResult.cs` — `custom_id` + `ChatResponse`-or-error.
- `src/lab-observability-api/Services/Claude/ClaudeBatchApiClient.cs` — *or* extend
  `ClaudeApiClient` with batch methods; prefer a sibling client that shares the payload
  builder, to keep transport concerns separable. Endpoint base is `/v1/messages/batches`
  (**confirm exact paths, size limits, and result-retrieval mechanics against current
  Anthropic docs during Build mode** — these are product specifics that move).
- `src/lab-observability-api/Controllers/AiBatchController.cs` — `POST /api/ai/batch`
  (submit), `GET /api/ai/batch/{id}` (status), `GET /api/ai/batch/{id}/results` (retrieve).
  Thin controller, delegates to `IBatchChatModelProvider`, accepts `CancellationToken`.

### Modified files

- `Program.cs`
  - `services.AddKeyedSingleton<IBatchChatModelProvider, ClaudeBatchChatModelProvider>("claude");`
  - A **separate** typed `HttpClient` + resilience config for the batch client. Do NOT
    reuse the interactive `AddStandardResilienceHandler` configuration: the interactive
    60s `TotalRequestTimeout` and the circuit breaker tuned for synchronous chat are wrong
    for batch. Each individual batch HTTP call (submit / single poll / retrieve) is itself
    quick, so per-call timeouts can be modest — but `in_progress` is a success response and
    must never count toward the failure ratio.
- `Telemetry/GatewayTelemetry.cs`
  - New `ActivitySource` spans: `batch.submit`, `batch.poll`, `batch.retrieve`.
  - New counters: `ai.provider.batch.submitted`, `ai.provider.batch.completed`,
    `ai.provider.batch.failed`.
- `Options/AnthropicOptions.cs`
  - Add batch-relevant settings if the batch endpoint differs from `BaseUrl`
    (e.g., a `BatchBaseUrl`), plus any batch enable/disable toggle if desired.
- `docs/standards/kql-cookbook.md`
  - Add batch queries (e.g., Query 10: batch job completion latency distribution;
    Query 11: batch cost-vs-interactive comparison) — with the explicit note that batch
    latency must not be alarmed against interactive SLOs.

### Files explicitly NOT affected

- `src/lab-observability-api/Providers/IChatModelProvider.cs` — interactive seam unchanged.
- `src/lab-observability-api/Models/ChatRequest.cs` — reused as batch element, **not modified**.
- `src/lab-observability-api/Models/ChatResponse.cs` — unchanged.
- `src/lab-observability-api/Providers/ClaudeChatModelProvider.cs` — interactive
  orchestration does not learn about batch.

### Resilience / alerting note

Batch must not inherit interactive resilience or alerting. Key invariants:

- `in_progress` polling responses are HTTP 200 successes — never failures.
- The `alert-ai-gateway-5xx-rate-dev-eastus-gio` rule and interactive latency
  percentiles do not apply to batch spans.
- Batch "slowness" (minutes to hours) is expected behavior, not a degradation signal.

### Rollback strategy

Batch is purely additive: new interface, new provider, new controller, new DI
registration. To disable, do not register `ClaudeBatchChatModelProvider` and remove
`AiBatchController`. Zero impact on the interactive path. No redeploy of interactive
behavior required.

### Forward work (named, not built)

1. Job orchestration layer: durable persistence of batch job IDs, polling scheduler,
   result storage, per-item retry of failed requests.
2. Second batch provider (OpenAI Batch / Bedrock) implements `IBatchChatModelProvider`
   — no change to the seam.
3. Unified cost telemetry comparing batch vs interactive cost-per-token for the same
   workload class (informs routing decisions in north-star item #4).

## References

- ADR-005 (interactive provider abstraction this deliberately does not extend)
- ADR-009 (caching inside the seam — the inverse case; same test, opposite answer)
- ADR-006 / ADR-008 (resilience and observability foundations that batch must NOT
  inherit verbatim)
- Anthropic Message Batches API documentation:
  <https://docs.anthropic.com/en/docs/build-with-claude/batch-processing>
  (verify current endpoints, limits, and result mechanics during Build mode)
- `docs/standards/kql-cookbook.md` — batch queries to be added on Day 8
