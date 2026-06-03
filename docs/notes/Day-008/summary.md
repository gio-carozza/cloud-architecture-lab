# Day 008 — Batch API Cost Controls

## Track
Build (primary) — Hybrid (AI-102, AZ-305 cert reinforcement secondary)

## Focus
Implement the Anthropic Messages Batch API as a standalone async job abstraction — a second cost lever alongside prompt caching, serving an offline evaluation pipeline that stress-tests the gateway at 50% of interactive token cost.

## Why This Matters

Day 7 cut the *per-request* cost of the system prompt by ~90% on the cached portion. That wins on the interactive path. But the interactive path is not the only path.

Enterprise AI teams run workloads that are not latency-sensitive: nightly prompt quality evaluations, regression tests against a golden set, bulk document classification, content moderation sweeps. These workloads are currently blocked from using the gateway because the gateway only speaks request-response. Every one of those batch-eligible calls is being either skipped entirely or run through the expensive synchronous path when the team runs it at all.

The Anthropic Batch API delivers 50% cost reduction on these jobs — and unlike prompt caching (which requires a minimum token threshold and a warm TTL), the savings are unconditional. The architectural question Day 8 must answer is not whether to use the Batch API — that answer is obvious. The question is *where in the codebase the batch contract belongs*, and whether it extends or replaces the interactive contract.

This is the inverse of ADR-009's decision. Prompt caching went **inside** the interactive seam on YAGNI grounds — one provider, one mechanism, no new abstraction needed. Batch is **not** a variant of request-response. It is submit → poll → retrieve: an async job shape that is semantically incompatible with `SendAsync`. A gateway that pretends batch is just a slow `SendAsync` is lying to its callers about what it does. Day 8 names that lie and refuses it.

## Whose Problem Am I Solving?

The **AI quality engineer** who runs a 1,000-prompt regression suite against the gateway before every release. Currently this costs $X at interactive rates and takes 20 minutes of wall-clock time waiting for synchronous responses. With the Batch API, it costs $X/2 and runs as a job they submit at end of day and retrieve in the morning — freeing the engineer's time and the budget.

Secondary: the **FinOps lead** who asks "what fraction of our AI spend could be deferred to batch without affecting the user-facing SLA?" Day 8 gives them a gateway that can answer that question in telemetry.

## What I Will Build

1. **`IBatchJobProvider` interface** — the new seam. Three methods: `SubmitAsync`, `GetStatusAsync`, `GetResultsAsync`. Separate from `IChatModelProvider`. Batch is a job, not a request.
2. **`BatchJobRequest` / `BatchJobStatus` / `BatchJobResult`** — new provider-agnostic contracts. Batch ID is a first-class return value. Status is an enum: `InProgress`, `Ended`, `Canceling`, `Expired`.
3. **`ClaudeBatchApiClient`** — Anthropic implementation. Calls `POST /v1/messages/batches` (submit), `GET /v1/messages/batches/{id}` (status), `GET /v1/messages/batches/{id}/results` (JSONL stream). Separate from `ClaudeApiClient` — different HTTP semantics, no resilience pipeline on submit (duplicate batch on retry = wrong).
4. **`ClaudeBatchJobProvider`** — DI-registered implementation of `IBatchJobProvider`, wraps `ClaudeBatchApiClient`.
5. **`BatchController`** — three endpoints: `POST /api/ai/batch` (submit), `GET /api/ai/batch/{id}` (status), `GET /api/ai/batch/{id}/results` (retrieve). Gateway is stateless — callers own the batch ID.
6. **Telemetry** — `batch.job.submitted` counter, `batch.job.result_count` histogram, `batch.savings_vs_sync` tagged metric (50% per token on batch path). KQL Query 10: batch cost vs. sync equivalent.
7. **ADR-010** — documents why batch gets its own seam rather than extending `IChatModelProvider`. The inverse of ADR-009.

## Step-by-Step Execution

### Phase A — ADR and contracts
Define the interface and contracts before any implementation:
- Write ADR-010: *Implement batch API as a separate IBatchJobProvider seam*
- Create `IBatchJobProvider.cs`, `BatchJobRequest.cs`, `BatchJobStatus.cs`, `BatchJobResult.cs`
- No implementation yet — interface-first so the boundary is clean before writing code

### Phase B — Anthropic implementation
- `ClaudeBatchApiClient.cs`: submit, status, results methods
- JSONL result parsing (`results` endpoint streams one result object per line)
- `ClaudeBatchJobProvider.cs`: wires `ClaudeBatchApiClient` behind `IBatchJobProvider`
- Register in DI with a named key `"claude-batch"`

### Phase C — API surface
- `BatchController.cs`: three endpoints with proper error contracts (`ApiError`, correlation IDs)
- No auth on Day 8 (gateway is internal); noted as Day 9–10 hardening item
- Response shape: submit returns `{ batchId, submittedAt, requestCount }`, status returns `{ batchId, status, requestCount, completedCount }`, results returns JSONL or a structured list

### Phase D — Telemetry
- `GatewayTelemetry` gains `BatchJobsSubmitted`, `BatchJobsCompleted` counters
- `batch.job.result_count` histogram on completion
- Log: savings calculation at retrieval time (`resultCount * avgInputTokens * 0.50 * pricePerToken`)
- KQL Query 10 in `kql-cookbook.md`

### Phase E — Local verification
- Submit a 3-request batch via `POST /api/ai/batch`
- Poll `GET /api/ai/batch/{id}` until `status == Ended`
- Retrieve results via `GET /api/ai/batch/{id}/results`
- Confirm telemetry shows batch savings metric

## Architect Thinking

**The central decision: new seam vs. extending IChatModelProvider.**

The temptation is to extend `IChatModelProvider` with a `SubmitBatchAsync` overload. This fails for the same reason adding `cache_control` to `ChatRequest` would have failed in Day 7: it leaks a provider-specific concern into a shared contract. But the problem here is worse — it's not just a provider-specific concern, it's a *different computation model*. `SendAsync` is synchronous in the caller's frame: you call it, you get a result. `SubmitBatchAsync` returns a handle, not a result. These are not the same thing with different latency — they are fundamentally different contracts.

An `IChatModelProvider` that does both synchronous and async-job semantics is no longer a coherent abstraction. Every future implementer (Azure OpenAI, Bedrock, Foundry) would need to implement both semantics even if they don't support batch. The abstraction becomes a tax on every provider for a capability that only some providers have.

`IBatchJobProvider` keeps the batch concern separate and additive. The interactive path is unchanged. A provider that doesn't support batch simply doesn't register `IBatchJobProvider`. No mandatory interface methods that can't be implemented — no `NotImplementedException` hiding in production code.

**The stateless gateway principle.**

The gateway does not store batch IDs. It proxies the Anthropic Batch API directly: the caller gets the batch ID on submit and is responsible for polling. This matches how the interactive path works — the gateway is stateless; it doesn't remember past requests. Making the batch path stateful (storing IDs in memory or a database) would introduce a new failure mode: the gateway restarts and all in-flight batch IDs are lost. The stateless design defers that problem to a future day (when Azure Table Storage or Cosmos DB is introduced as a state backend).

**No resilience pipeline on submit.**

The interactive path has a circuit breaker, attempt timeout, and (deliberately disabled) retry. The batch submit must not retry — a network error during submit may have succeeded server-side; retrying creates a duplicate batch. The correct behavior on submit failure is: surface the error, let the caller decide whether to resubmit. The resilience pipeline is not wired for the batch client.

**Why batch savings are 50%, not variable.**

Anthropic prices batch at exactly 50% of the synchronous rate, unconditionally. This is different from prompt caching (which depends on cache hit rate, TTL, and system prompt size). Batch savings can be stated as a fact in telemetry: `savedUSD = resultCount * avgInputTokens * inputPricePerToken * 0.5`. This gives the FinOps engineer a precise number, not a probabilistic estimate.

**What elite architects do differently.**

They name the computation model explicitly before writing an interface. "Batch" and "interactive" are not the same model with different speed — they differ in caller semantics, error handling, retry logic, state management, and billing. An architect who treats them as the same model and patches the difference is setting up a future refactor when the difference becomes undeniable. Name the seam now; pay the refactor cost never.

**Common beginner mistakes.**

- Extending `IChatModelProvider` with a batch method — makes every provider implement something they may not support
- Retrying the batch submit on network error — creates duplicate jobs with real billing consequences
- Storing batch IDs in the gateway — adds statefulness to a stateless service without a persistence layer
- Not surfacing savings as a metric — leaves the FinOps case invisible

## Artifacts

- **Code:**
  - `Models/Batch/BatchJobRequest.cs`, `BatchJobStatus.cs`, `BatchJobResult.cs`
  - `Services/Batch/IBatchJobProvider.cs`
  - `Services/Batch/ClaudeBatchApiClient.cs`
  - `Services/Batch/ClaudeBatchJobProvider.cs`
  - `Controllers/BatchController.cs`
  - `Telemetry/GatewayTelemetry.cs` — two new counters, one histogram
- **Docs:**
  - `docs/adr/ADR-010-introduce-batch-job-provider-seam.md`
  - `docs/architecture/day-008-batch-api-cost-controls.md`
  - `docs/standards/kql-cookbook.md` — Query 10
  - `docs/notes/Day-008/` — summary, checklist, architect-thinking, posture-check
- **Infra:**
  - `Infra/Day-008/appsettings-template.md` — no new settings (batch uses existing `Anthropic__ApiKey` and `Anthropic__BaseUrl`)

## Portfolio Value

"Extended an AI gateway with an async batch processing path, making a principled architectural decision to introduce a separate `IBatchJobProvider` seam rather than polluting the interactive contract. Reduced token cost by 50% on all offline workloads. Kept the gateway stateless by proxying Anthropic's job ID directly to callers. Made savings visible as telemetry from day one. Documented the decision in ADR-010 as the explicit inverse of ADR-009 — demonstrating that YAGNI is not a blanket rule but a judgment call made per-case with the tradeoffs named."

This proves:
- You know when *not* to reuse an abstraction, not just when to reuse one
- You understand that different computation models need different contracts
- You make cost reduction visible, not just present

## Completion Checklist
See `completion-checklist.md`

## Certification Reinforcement

### AZ-900 — None
No direct mapping this day.

### AZ-104 — None
No new infrastructure provisioned; no App Service configuration changes.

### AZ-305 — **Secondary**
- Design application architecture: async/job-shaped workload pattern (batch processing vs. request-response); recommend a compute solution for batch processing (WAF decision point)
- Cost optimization as a design principle: unconditional 50% reduction on batch-eligible workloads; when to route to batch vs. synchronous path based on latency SLA

### AI-102 — **Primary**
- Implement generative AI solutions: Azure OpenAI Batch API pattern (same semantic model as Anthropic Batch — submit/poll/retrieve); cost optimization on offline AI workloads
- Optimize and operationalize a generative AI solution: batch vs. synchronous routing decision; cost-per-token attribution for batch path

## Architect Posture Check
See `posture-check.md` (filled at end of day, BEFORE marking complete)
