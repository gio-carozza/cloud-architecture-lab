# Concepts — Plan and Manage an Azure AI Solution (AI-102 Domain 1)

---

## Cost-Per-Token Attribution Across Processing Paths

### If you're 10 years old
Imagine you run a lemonade stand and you sell two sizes: a quick small cup (synchronous) and a big jug ordered in advance for a party (batch). They cost different amounts to make. If you want to know where your money is going, you need to track the cost of each size separately — you can't just add them all together and call it "lemonade costs." Cost-per-token attribution does the same thing for AI: it tracks how much each type of request actually costs so you can make smarter decisions.

### If you're a CEO
Without cost-per-token attribution, you cannot tell finance which team, product, or workload is driving AI spend growth. You're looking at a total bill with no line items. Attribution turns "our AI costs went up 40% this month" into "our batch summarization job is now 80% of our AI spend — here's the ROI case for it." That's the difference between a budget conversation and a guessing game.

### If you're an Engineer
Log the provider's usage response fields on every request: `prompt_tokens`, `completion_tokens`, `cache_creation_tokens`, and `cache_read_input_tokens`. Tag each log entry with a `path` dimension (`"sync"` or `"batch"`) and a `provider` dimension. In Application Insights, these become custom dimensions queryable via KQL: `customEvents | where customDimensions.path == "batch" | summarize sum(tolong(customDimensions.total_tokens)) by bin(timestamp, 1d)`. The common error: logging total tokens without the path tag, making per-path analysis impossible after the fact.

### If you're an Architect
Cost-per-token attribution is the practice of capturing and tagging LLM token consumption by processing path (synchronous vs. batch) so that costs can be analysed, budgeted, and reported at the workload level. In a gateway architecture, this means logging `prompt_tokens`, `completion_tokens`, and `cache_creation_tokens` from the provider's usage response to a telemetry sink (Application Insights, Log Analytics) alongside a `path` dimension (`"sync"` / `"batch"`).

The architectural value is governance: without path-level attribution, you cannot determine whether cost growth is driven by the interactive user-facing path or the offline batch path, and you cannot evaluate whether the 50% batch discount is actually being captured. Azure Cost Management operates at the resource level (e.g., Azure OpenAI deployment), not at the request level — gateway-layer attribution fills that gap.

**Why this matters in enterprise:** Cost transparency is a first-class governance requirement in enterprise AI. Finance teams and platform owners need per-workload cost data to make buy-vs-build and scale decisions. A gateway that cannot break down token cost by path is architecturally incomplete.

**Common beginner mistake:** Logging total tokens per request without tagging the processing path, then discovering months later that the "cheap batch jobs" represent 80% of spend because no one was routing correctly.

---

## Operationalizing a Generative AI Solution

### If you're 10 years old
Building a robot is fun, but keeping it running every day is harder work. You have to check that it's not broken, make sure it doesn't do anything weird, and fix it fast when something goes wrong. "Operationalizing" just means making sure your AI keeps working reliably day after day, not just on the first day you turn it on.

### If you're a CEO
An AI feature that works in a demo but has no monitoring is a liability waiting to happen. Operationalization means you'll know when the AI is slow, wrong, or too expensive — before your customers tell you. The cost of retrofitting observability after a production incident is 10x the cost of building it in from day one. Operationalized AI is trustworthy AI; unmonitored AI is a risk you're carrying silently.

### If you're an Engineer
Operationalizing means wiring four things before you ship: (1) structured logs with correlation IDs on every request — use `ILogger` with Serilog and the Application Insights sink; (2) token usage metrics logged as custom events per request; (3) a `/health` endpoint that names the running build (assembly version + git SHA); (4) a hard cap at the submission boundary (HTTP 400 before a request can generate an unbounded bill). Common mistake: shipping with `Console.WriteLine` logging and calling it "done."

### If you're an Architect
Operationalizing a generative AI solution means putting in place the instrumentation, processes, and controls that make it safe to run in production over time. The four pillars for an AI gateway are:

1. **Observability** — structured logs with correlation IDs, distributed traces, and metrics (latency p50/p95, error rate, token throughput). Without these, you are blind to degradation.
2. **Cost governance** — per-request token logging with path/provider dimensions, hard caps at submission boundaries, and budget alerts. Cost must be a first-class signal, not an afterthought.
3. **Resilience** — retry policies, circuit breakers, timeouts configured per processing path (tighter for synchronous, relaxed for batch).
4. **Routing correctness** — ensuring requests land on the right path (batch vs. synchronous) based on latency SLA, not by accident.

**Why this matters in enterprise:** An AI solution that works in a demo but has no observability or cost controls is a liability, not an asset. Elite architects design operability in from day one — not as a phase-2 retrofit.

**Common beginner mistake:** Shipping an AI feature with application-level logging only (stdout/stderr), with no structured telemetry, no cost tracking, and no budget ceiling. The first production incident reveals the gap.

---

## Quota Management for Azure AI Services

### If you're 10 years old
Imagine your school gives each student 100 library books per month. If you use them all in one week, you can't borrow any more until next month. AI services work the same way — there's a monthly limit (quota), and if you hit it, requests start failing. Good architects plan ahead so they don't run out.

### If you're a CEO
Quota exhaustion means your AI feature returns errors to users — without warning, often during peak hours, sometimes caused by a background job no one noticed. It is predictable and preventable. The business risk is user trust: users who see AI errors at 9am on Monday don't think "quota problem," they think "the product is broken." Proper quota planning is a reliability investment, not an ops detail.

### If you're an Engineer
Azure OpenAI enforces tokens-per-minute (TPM) and requests-per-minute (RPM) per deployment. Check remaining quota from response headers: `x-ratelimit-remaining-tokens` and `x-ratelimit-remaining-requests`. Monitor `TokensPerMinuteUsagePercentage` in Azure Monitor — alert at 80%, not 100%. For batch workloads, use Global Batch which has a separate enqueued-token quota pool that cannot consume your synchronous TPM. A 429 response means you've hit the limit; implement retry with exponential backoff + jitter using Polly's `AddStandardResilienceHandler`.

### If you're an Architect
Azure AI Services and Azure OpenAI enforce quota at multiple levels: tokens-per-minute (TPM) for synchronous calls, requests-per-minute (RPM), and enqueued tokens for batch. Quota is scoped to a deployment within a subscription and region. Key operational concerns:

- **Quota isolation** — synchronous and batch quota pools should be separate deployments (or use the built-in isolation of Global Batch's enqueued-token quota) so a batch spike cannot throttle interactive traffic.
- **Quota monitoring** — track `x-ratelimit-remaining-tokens` response headers or Azure Monitor metrics (`TokensPerMinuteUsagePercentage`) to alert before exhaustion, not after.
- **Quota planning** — estimate peak load for each path, add headroom (≥20%), and size deployments accordingly. Undersizing is a common root cause of production 429 errors.
- **Regional failover** — if a region's quota is exhausted, routing to a secondary region is the escape hatch. This requires the provider abstraction to support region-aware deployment selection.

**Why this matters in enterprise:** Quota exhaustion is one of the most common causes of AI service outages in production. It is predictable and preventable with proper planning — but only if you understand the quota model for each processing path.

**Common beginner mistake:** Setting quota on a single shared deployment for all workloads, then being surprised when a nightly batch job causes 429 errors for users the next morning.

---

## Responsible AI and Model Selection Governance

### If you're 10 years old
When you pick a helper for a job, you think about whether they're trustworthy, fair, and good at that specific job. Choosing an AI model is the same — you need to pick the right one for the task, and make sure it won't say harmful things or make unfair decisions. That's "responsible AI."

### If you're a CEO
Every AI model you deploy carries risk: it can produce harmful content, make biased decisions, or expose private data. Responsible AI governance means documenting which model is used for what, filtering harmful outputs before they reach users, and having an audit trail if something goes wrong. The business consequence of not doing this: regulatory fines, reputational damage, and legal liability. The cost of doing it right: a few hours of architecture work per feature.

### If you're an Engineer
Implement Azure AI Content Safety to filter inputs and outputs: configure harm categories (violence, hate, self-harm, sexual) with thresholds appropriate to your use case. Log every filtering decision to Application Insights as a custom event with the request's correlation ID. For model governance, store the model ID in configuration (`IOptions<T>`), not hardcoded — this makes model changes a config deployment, not a code deployment. Add a model ID assertion in your health check so the running model is always observable.

### If you're an Architect
Model selection governance means choosing and documenting the right model for each workload based on: capability (does it handle the task accurately?), cost (tokens-per-dollar tradeoff), latency profile (acceptable for synchronous?), content safety (Azure AI Content Safety filters needed?), and compliance (data residency, model terms of use).

In a gateway architecture, the model selection decision should be explicit and documented — not left to callers. The routing configuration (`DefaultModel`, `BatchModel`, per-path overrides) is the governance artefact. Key AI-102 exam concepts: Azure AI Content Safety for filtering harmful outputs, Responsible AI principles (fairness, reliability, privacy, inclusiveness, transparency, accountability), and model evaluation (benchmarking against ground truth for quality regression detection).

**Why this matters in enterprise:** Undocumented model choices are a compliance and audit risk. If a model is changed without evaluation, quality regressions are invisible until a user complaint surfaces them.

**Common beginner mistake:** Using the same model for every workload because "it's the default" rather than matching model capability and cost to the specific task's requirements.
