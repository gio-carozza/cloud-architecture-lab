# Concepts — Design Infrastructure Solutions (AZ-305 Domain 4)

---

<!-- Day 9 Additions: streaming response pattern, TTFT SLO design, proxy buffering -->

## Streaming vs. Buffered Response Pattern

### If you're 10 years old
Imagine watching a YouTube video. You don't have to wait for the whole video to download before you can start watching — it loads a little bit at a time as you watch. Streaming APIs work the same way: instead of making you wait for the whole answer, you start seeing words appear right away, almost like watching someone type. A "buffered" response is the opposite — you wait, wait, wait, then everything appears at once.

### If you're a CEO
Streaming is the difference between an AI product that feels instant and one that feels broken. Every major AI product users compare yours to (Claude, ChatGPT, Copilot) streams. A buffered gateway response with a 4-second blank wait will cause customers to assume the product is slow or failing before the answer even appears.

### If you're an Engineer
Streaming in HTTP uses `Content-Type: text/event-stream` (Server-Sent Events, SSE). The server sends `data: <json>\n\n` frames incrementally and calls `FlushAsync()` after each. The client reads line-by-line as frames arrive. In .NET 8 with Anthropic: set `"stream": true` in the POST body, use `HttpCompletionOption.ResponseHeadersRead` on `SendAsync`, and `StreamReader.ReadLineAsync(CancellationToken)` to read SSE events. Return type on the provider interface is `IAsyncEnumerable<ChatChunk>`. The controller sets `Content-Type: text/event-stream`, `Cache-Control: no-cache`, and `X-Accel-Buffering: no` (nginx proxy directive) BEFORE writing any body. Input validation must happen BEFORE setting SSE headers — once headers are committed, the HTTP status code cannot be changed. Mid-stream errors become SSE `event: error` frames.

### If you're an Architect
The architectural decision is whether streaming is a new interface seam or an extension of the existing chat interface — and the correct test is Liskov Substitutability, not return-type symmetry. If a provider that lacks native streaming can implement `StreamAsync` without throwing (by yielding a single terminal chunk), streaming belongs on the existing interface. If it cannot implement the method without throwing (as batch cannot implement `SubmitBatchAsync`), a new seam is warranted. This test is the difference between principled Interface Segregation and cosmetic consistency. At enterprise scale, the infrastructure concern is every proxy in the request path: App Service nginx, API Management, CDN, and load balancers all buffer responses by default. `X-Accel-Buffering: no` handles nginx; API Management requires response buffering policy disabled; CDN origins need streaming pass-through configured. Each buffering layer silently defeats SSE and produces a "works locally, broken in staging/prod" defect that is expensive to diagnose. Common beginner mistake: testing SSE locally where there is no nginx, and shipping to production without verifying incremental delivery through the full proxy chain.

---

## Latency SLO Design — TTFT vs. Total Response Time

### If you're 10 years old
If you're watching someone type an answer, you care about two different "waits": how long before you see the first letter, and how long before they finish typing. TTFT is the first wait — the blank screen. Total time is the whole thing. For streaming, only the first wait matters for whether the experience feels fast or slow.

### If you're a CEO
For streaming AI products, "the AI is slow" is almost always a first-token problem, not a total-duration problem. These have different causes, different fixes, and different SLAs. Measuring only total latency makes both invisible. A p95 TTFT SLA is the commitment that protects user experience; total duration is a secondary quality metric.

### If you're an Engineer
TTFT is measured by starting a `Stopwatch` before `SendAsync` and recording `Elapsed.TotalMilliseconds` on the first yielded chunk. Emit as `Histogram<double>` (not `Counter` — you need percentiles). KQL for SLO compliance: `customMetrics | where name == "ai.chat.stream.ttft_ms" | summarize p95=percentile(value, 95) by bin(timestamp, 5m)`. Alert rule: `p95 > 2000ms` over a 5-minute window, Severity 2, Action Group. Typical targets: p95 TTFT < 500ms for in-region (same Azure region as provider); < 2000ms for cross-region. Total response duration matters for cost (longer output = more tokens) but not for perceived latency in a streaming UX.

### If you're an Architect
TTFT and total response time are separate SLO dimensions requiring separate alert rules, separate histograms, and separate incident response runbooks. The diagnosis paths are different: high TTFT → provider latency, network routing, system prompt size, prompt cache miss rate; high total duration → output token count (prompt engineering problem), model choice. Conflating them in a single `ai.provider.latency.ms` metric hides which variable is degrading. At enterprise scale, multi-provider routing based on TTFT percentiles (route to the provider with the lower p95 TTFT for interactive workloads) is a genuine cost-performance optimization — but only possible if TTFT is instrumented per provider. Common mistake: using average latency for alerts. The 99th percentile user's experience is always worse than the average; p95 is the minimum responsible alert threshold for any user-facing latency SLO.

---

## Async / Job-Shaped Workload Pattern

### If you're 10 years old
Imagine a restaurant. Some orders go to the "fast lane" (burgers, ready in 3 minutes). Others go to the "slow lane" — a whole roast chicken that takes an hour. You don't make the customer stand at the counter for an hour; you give them a ticket number, seat them, and bring the food when it's ready. Async workloads work the same way: the system accepts the job, gives back a ticket (job ID), does the work in the background, and lets the caller collect the result when it's done.

### If you're a CEO
Async architecture means your users don't wait. For expensive or long-running AI operations — summarizing 1,000 documents, processing a batch of invoices — the user submits the request, gets a confirmation immediately, and collects results when they're ready. No spinning wheel, no timeout, no frustrated user. The alternative (making users wait for a 5-minute AI job to complete) is a usability and reliability failure. Async design is what separates AI products that feel professional from ones that feel like prototypes.

### If you're an Engineer
HTTP contract for async: return `202 Accepted` immediately with a `Location` header pointing to a status endpoint and a job ID in the response body. Never return `200` for a job that isn't finished. The status endpoint returns `{"status":"in_progress"|"completed"|"failed", "resultUrl":"..."}`. Poll it; don't hold the connection open. In .NET with Azure OpenAI: submit via `client.GetBatchClient().CreateBatchAsync(...)`, poll via `client.GetBatchClient().GetBatchAsync(jobId)`, retrieve via the output file ID. Common mistake: returning `200` with a job ID — clients have no way to know they should poll rather than read the body.

### If you're an Architect
The async/job-shaped workload pattern decouples job submission from result retrieval via a durable queue or broker. The canonical shape is: **submit → acknowledge (202 Accepted + job ID) → poll status → retrieve result**. This maps directly to the Anthropic Batch API, Azure OpenAI Global Batch, and any durable task framework (Azure Durable Functions, Service Bus + worker pattern).

The pattern is appropriate when: (a) processing time exceeds acceptable synchronous wait time (typically > 1–2 seconds for interactive, > 60 seconds for any user-facing), (b) the workload is resource-intensive and benefits from dedicated compute, or (c) bursts require queue-based load levelling rather than direct scaling. The tradeoff is operational complexity: you now have a queue, a worker, a status store, and a retrieval endpoint — four components instead of one.

**Why this matters in enterprise:** The async pattern is the foundation of scalable AI pipelines. Without it, every expensive LLM call blocks a thread and a connection. The exam tests whether you can identify which compute architecture fits an async workload (durable functions vs. container jobs vs. batch-native APIs) and articulate why.

**Common beginner mistake:** Building a synchronous request-response API for a job that takes 30+ seconds, then adding timeouts and "please wait" spinners as a band-aid, rather than designing the async handoff from the start.

---

## Recommending Compute for Batch Workloads — WAF Decision Framework

### If you're 10 years old
If you need to move 10 boxes, you could carry them one at a time (slow, tiring), hire someone to carry them on a cart for you (a service that handles it), or rent a van and drive them all at once (batch). Picking the right option depends on how heavy the boxes are, how fast you need them moved, and how much money you want to spend. Architects make the same kind of decision for compute — they pick the right "vehicle" for the job.

### If you're a CEO
For AI batch workloads, the provider-native batch API (Azure OpenAI Global Batch, Anthropic Batch API) is almost always the right choice: zero infrastructure to manage, zero compute cost, and 50% off token pricing built in. Choosing Kubernetes or Container Apps for a nightly batch job means paying for cluster management, image builds, and scaling configuration — all for a job that runs once a day and the managed batch API handles at half the price. Over-engineering batch workloads is one of the most common ways AI projects overspend.

### If you're an Engineer
Decision tree for batch compute: (1) LLM batch workload with no heavy pre/post-processing → use provider-native Batch API (zero infra, 50% cost). (2) Short jobs (< 10 min), event-triggered → Azure Functions Consumption (pay per execution, no idle cost). (3) Long jobs (> 10 min) with stateful orchestration → Azure Durable Functions. (4) Containerized worker, scale-to-zero → Azure Container Apps Jobs. (5) HPC-style large parallel compute → Azure Batch. For AZ-305 exam: when the scenario describes a nightly LLM job with no custom processing, the answer is provider-native batch API + zero additional compute. When custom processing is described (parsing, enrichment, storage), use Functions or Container Apps Jobs.

### If you're an Architect
The Azure Well-Architected Framework (Cost Optimization pillar) directs architects to **match compute size and billing model to workload shape**. For batch AI workloads the decision tree is:

| Factor | Leaning |
|--------|---------|
| Job duration < 10 min, event-triggered | Azure Functions (Consumption or Flex) |
| Job duration > 10 min, stateful orchestration | Azure Durable Functions |
| Very large parallel compute, HPC-style | Azure Batch |
| Containerised worker, scale-to-zero | Azure Container Apps Jobs |
| Provider-native batch (LLM) | Azure OpenAI Global Batch / Anthropic Batch API |

For LLM-specific batch workloads, the provider-native batch API is the default recommendation: it requires no additional compute resource, no queue management, and delivers the 50% cost reduction as a built-in pricing tier. You pay only for tokens — not for the compute that processed them.

**Why this matters in enterprise:** The AZ-305 exam presents scenarios where you must choose among multiple valid compute options. The WAF cost lens eliminates options that are over-engineered or over-priced for the workload's actual requirements.

**Common beginner mistake:** Reaching for Azure Kubernetes Service for a simple nightly batch job, adding operational overhead (cluster management, pod scaling, ingress) that is wildly disproportionate to the workload's needs.

---

## Cost Optimization Pillar — Deferrable Workload Routing

### If you're 10 years old
Electricity costs less at night (off-peak hours) than during the day (peak hours). Smart home owners run their dishwasher at midnight to save money. The Cost Optimization pillar in Azure works the same way — run things that can wait during "off-peak" (batch mode) and save the expensive "right now" capacity for things that truly need it.

### If you're a CEO
Every AI request in your system is either urgent or deferrable. Urgent requests — user-facing, real-time — need synchronous processing; pay full price. Deferrable requests — nightly reports, batch analysis, background enrichment — do not need instant results. Routing them to batch pricing cuts their token cost by 50%, automatically, with no quality tradeoff. If 40% of your AI workload is deferrable and you're running it all synchronously, you're overpaying for that 40% by 2x. Identifying and rerouting deferrable workloads is the highest-ROI architectural change available for most AI platforms.

### If you're an Engineer
Implement routing via an explicit `"mode": "sync" | "batch"` field in your API contract. Never infer the processing path from request characteristics — the caller knows their SLA, the gateway doesn't. In the gateway, route `sync` to `IChatModelProvider.CompleteChatAsync()` and `batch` to `IBatchProvider.SubmitAsync()`. Log the `path` dimension on every token-usage telemetry event. Add a `MaxBatchSize` guard before enqueuing to prevent cost blowouts. Document the routing decision in an ADR so it's not reversed during refactoring.

### If you're an Architect
The Azure Well-Architected Framework Cost Optimization pillar defines **deferrable workload routing** as one of the primary cost levers: workloads without a real-time latency requirement should be routed to a lower-cost compute or pricing tier. For AI workloads this translates directly to the batch vs. synchronous routing decision:

- **Synchronous path** — tokens billed at standard rate, dedicated TPM quota consumed, response in seconds
- **Batch path** — tokens billed at 50% of standard rate, isolated enqueued-token quota, response within 24 hours

The routing criterion is the **latency SLA**: if the business requirement can tolerate a result in minutes-to-hours rather than seconds, the batch path is the correct choice on cost grounds alone. This is a WAF design decision, not an implementation detail — it should be documented in an ADR and encoded in the gateway's routing logic.

**Why this matters in enterprise:** The unconditional 50% cost reduction on batch-eligible workloads is one of the highest-ROI architectural decisions available for AI-heavy platforms. Missing it because "synchronous was easier to implement" is a recurring pattern that costs organisations tens of thousands of dollars per month at scale.

**Common beginner mistake:** Treating the batch vs. synchronous choice as a performance decision ("batch is slower") rather than a cost-governance decision ("batch is cheaper for deferrable work"). Synchronous is not "better" — it is more expensive and appropriate only when latency SLA demands it.

---

## Latency SLA as an Architectural Constraint

### If you're 10 years old
Some things have to happen right now — like a fire alarm going off the second there's smoke. Other things can wait — like getting your school report card at the end of term instead of the minute each test is marked. Architects decide which things are "right now" versus "can wait" — and that decision changes how they build the whole system.

### If you're a CEO
Not every AI request needs a sub-second response. A nightly report can take minutes. A background classification job can take hours. The mistake companies make is building everything for sub-second latency — because that's the easiest path — then discovering they've been overpaying for latency headroom they never needed. Defining latency SLAs per workload type before building lets you match the technology and pricing tier to the actual requirement. This is architecture doing its job.

### If you're an Engineer
Capture the latency SLA as an explicit field in your API contract, not a runtime inference. Decision tree by SLA: < 1s → synchronous streaming; 1–30s → synchronous with timeout or async short-poll; 30s–5min → `202 Accepted` + status endpoint + poll; > 5min → batch API with 24h SLA. In the gateway, validate that `mode=batch` is only accepted for requests where the caller's contract explicitly tolerates a delayed response — don't allow callers to route to batch by accident. Log the `sla_class` dimension alongside `path` for attribution queries.

### If you're an Architect
The latency SLA is the maximum acceptable time between a request being submitted and the result being available to the caller. It is a **first-class architectural constraint** that drives compute selection, processing path, and pricing tier. The decision tree:

- **< 1 second** — synchronous API, premium compute tier, low-latency routing
- **1–30 seconds** — synchronous with streaming (token streaming for LLMs) or async with short-poll
- **30 seconds – 5 minutes** — async with status endpoint (202 + poll), standard compute
- **> 5 minutes / overnight** — batch API or scheduled job, lowest-cost compute tier

For AI gateways, explicitly capturing the latency SLA for each request class in the gateway contract (as an input field or endpoint segment) forces the routing decision to be made by the client at design time, not by the gateway at runtime based on guesswork.

**Why this matters in enterprise:** Latency SLAs are contractual obligations — violating them triggers SLA credits or escalations. Designing systems without explicit SLA tiers leads to over-provisioning (paying for sub-second latency everywhere) or under-provisioning (batching something that needed to be interactive).

**Common beginner mistake:** Designing a single endpoint that "tries to be fast but falls back to async" without documenting which path a given request will take, leaving callers with unpredictable response times and no way to reason about cost.
