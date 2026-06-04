# Concepts — Implement Generative AI Solutions (AI-102 Domain 2)

---

## Batch API Processing Pattern

### If you're 10 years old
Imagine you have 1,000 letters to mail. You could run to the post office every time you finish one letter (slow, expensive), or you could pack all 1,000 letters into a big box, drop it off once, and pick up all the replies the next day. The Batch API is the "big box" approach — you send lots of AI requests at once, they get processed in the background, and you collect the results when they're ready.

### If you're a CEO
The Batch API cuts your AI processing costs in half for any workload that doesn't need an instant answer. Nightly reports, bulk document analysis, content generation at scale — anything where results can wait hours instead of seconds qualifies. The 50% cost reduction is not a discount you negotiate; it's a pricing tier that exists specifically for this workload shape. If you're not using it for deferrable workloads, you're paying double with no business benefit.

### If you're an Engineer
Submit a JSONL file where each line is a separate request object with a `custom_id`. Receive a batch job ID. Poll `GET /batches/{id}` until `status == "completed"` (Anthropic: `"ended"`). Then `GET /batches/{id}/output_file_id` to retrieve results as another JSONL. Each output line maps back to your `custom_id`. In .NET, implement `IBatchProvider` with `SubmitAsync(IEnumerable<BatchRequest>)`, `GetStatusAsync(string jobId)`, and `GetResultsAsync(string jobId)`. Common error: calling the results endpoint before polling for completion — it returns 404 or an error, not partial results.

### If you're an Architect
The batch API pattern (Anthropic Messages Batch API; Azure OpenAI Global Batch) is an asynchronous, offline-processing model: clients submit a JSONL file of requests, receive a job ID, poll for completion, then retrieve results. Azure OpenAI Global Batch guarantees a 24-hour turnaround and prices at **50% of the synchronous (global standard) rate**, funded by separate enqueued-token quota that does not compete with your online workload quota.

The abstraction contract matters across providers: both Anthropic and Azure OpenAI use the same three-phase semantic — **submit → poll → retrieve** — enabling a single `IBatchProvider` interface to span both without leaking provider-specific types. Enterprise architects choose the batch path when: (a) results are not user-facing in real time, (b) cost-per-token matters more than latency, and (c) workload volume is predictable enough for quota management.

**Why this matters in enterprise:** Nightly report generation, bulk document summarization, and offline classification jobs are natural batch workloads. Running them synchronously pays 2× the token cost with no latency benefit for the business.

**Common beginner mistake:** Using synchronous endpoints for batch-eligible workloads because "it's simpler to implement," then treating runaway LLM spend as an ops problem rather than an architecture mistake.

---

## Batch vs. Synchronous Routing Decision

### If you're 10 years old
Think of ordering food: fast food (synchronous) costs more but arrives in 5 minutes. Meal-prep delivery (batch) costs less but you order tonight and get food tomorrow morning. The trick is knowing which meal you're ordering — if your boss is waiting at the table, fast food. If it's for next week's lunches, meal prep wins every time.

### If you're a CEO
Every AI request in your system is either "someone is waiting for this right now" or "this can wait." The first category needs synchronous — pay full price for instant response. The second category should use batch — pay half price, get results in hours. Most companies run everything synchronously because it's the default, and quietly overpay for workloads nobody is watching in real time. Identifying and rerouting deferrable workloads to batch is one of the highest-ROI AI cost optimizations available.

### If you're an Engineer
Encode the routing decision as an explicit field in your API contract: `"mode": "sync" | "batch"`. Never infer it from request size or caller identity — callers know their latency requirements, the gateway doesn't. In the gateway, route to `IChatModelProvider` for sync and `IBatchProvider` for batch. For the batch path, validate the request against `MaxBatchSize` before enqueuing — rejecting at the boundary (HTTP 400) is cheaper than cancelling a job mid-flight. Log `path=sync|batch` as a dimension on every token-usage event so cost attribution works.

### If you're an Architect
The routing decision between batch and synchronous is a **latency-SLA vs. cost tradeoff** that belongs in application logic, not left to the caller to guess. Key signals for the batch path: SLA > 1 minute, no real-time user interaction, workload is parallelizable, and cost-per-token is a budget constraint.

For a gateway that exposes both paths, the decision boundary should be an **explicit input field** (e.g., `"mode": "batch"`) rather than inferred from heuristics — callers know their latency requirements; the gateway should not guess. The Azure Well-Architected Framework Cost Optimization pillar explicitly identifies async/batch compute as a mechanism to reduce per-unit cost for deferrable workloads.

**Why this matters in enterprise:** The routing decision is a cost-governance choice, not just a technical one. Encoding it as an explicit parameter surfaces the decision in code review, audit logs, and cost attribution.

**Common beginner mistake:** Treating batch as a performance optimization rather than a cost optimization. Batch does not make individual requests faster — it makes large volumes cheaper.

---

## Batch Budget Controls and Hard Caps

### If you're 10 years old
Imagine a self-serve candy store where kids can fill a bag themselves. Without a rule, one kid could take everything. A hard cap is like a sign: "Maximum 20 pieces per bag." The AI gateway does the same thing — before accepting a big batch job, it checks: "Is this request too large? If so, refuse it before any money is spent."

### If you're a CEO
A single misconfigured API call can submit 100,000 batch requests and exhaust your monthly AI budget overnight. A hard cap at the submission boundary stops this before it costs anything — the request is rejected with an explanation, no tokens are consumed. Billing alerts tell you after money is spent; hard caps prevent it from being spent. One is reactive governance, the other is proactive governance. Only one of them protects the budget.

### If you're an Engineer
Implement a `MaxBatchSize` limit in `IOptions<BatchOptions>` and validate on every submit request before calling the provider API. Return HTTP 400 with a body that includes the request count, the limit, and the estimated token cost so the caller can fix their request. Decouple the policy (the ceiling value in config) from enforcement (the validation logic in the controller/handler) so the limit can be tuned per environment via App Service settings without a code deployment. Test the boundary explicitly: submit `MaxBatchSize + 1` requests and assert HTTP 400.

### If you're an Architect
Budget enforcement at the batch submission layer is **pre-emptive cost control**: estimate total token cost from the request payload before the job is enqueued, and reject it (HTTP 400 / 402 with a cost breakdown in the response body) if it exceeds a configured ceiling. Refusing at the boundary is architecturally preferable to accepting and cancelling mid-flight, which may have already consumed quota.

The pattern has two components: (1) an estimator that approximates token consumption from the request payload, and (2) a configurable hard cap in an `IOptions<T>` class. Decoupling policy (the ceiling value) from enforcement (the estimator logic) means the ceiling can be tuned per environment without a code deployment.

**Why this matters in enterprise:** Uncapped batch submissions are a common vector for runaway LLM spend — a single misconfigured client can exhaust a monthly budget overnight. Pre-flight rejection is the only control that prevents cost before it occurs; billing alerts only tell you after the fact.

**Common beginner mistake:** Relying exclusively on post-hoc billing alerts and Azure Cost Management budgets rather than enforcing limits at the API boundary where the work is requested.

---

## Azure OpenAI Global Batch — Quota Model

### If you're 10 years old
The online lane at a store and the self-checkout lane have separate lines. Using self-checkout (batch) doesn't slow down the regular checkout lane (online). Azure does the same thing — batch jobs have their own line so they don't slow down your live app.

### If you're a CEO
Global Batch has its own quota pool that is completely separate from your live application's quota. This means you can run a large overnight batch job without risking any degradation to your users the next morning. The two workloads don't compete. This isolation is architectural — it's not something you configure at runtime, it's built into how the pricing tier works.

### If you're an Engineer
Global Batch quota is measured in enqueued tokens, not tokens-per-minute. Configure it separately from your synchronous deployment in Azure OpenAI Studio. There is no `x-ratelimit-remaining-tokens` header for batch (unlike synchronous) — monitor queue depth and job status via the Batch API instead. Global Batch routes jobs across multiple Azure regions for availability; you cannot pin it to a single region. The 24-hour SLA is the target, not a guarantee — monitor job duration in Application Insights and alert if jobs run longer than your acceptable window.

### If you're an Architect
Azure OpenAI Global Batch uses **separate enqueued-token quota**, completely isolated from the tokens-per-minute (TPM) quota that governs synchronous calls. This means a large overnight batch job cannot starve your real-time API consumers. The tradeoff: batch quota is typically measured in enqueued tokens with a 24-hour processing SLA, not a sub-second response guarantee.

Deployment architecture implication: a single Azure OpenAI deployment can serve both synchronous and batch traffic without quota contention, provided the batch quota is configured independently. Global Batch routes requests across Azure regions for best availability — it is not pinned to a single deployment region.

**Why this matters in enterprise:** Quota contention between interactive and batch workloads is a common production incident. Separate quota pools are the architectural isolation that prevents a bulk job from degrading the user-facing experience.

**Common beginner mistake:** Sharing a standard deployment's TPM quota for both online and batch traffic, causing throttling (429s) on interactive endpoints during peak batch processing windows.
