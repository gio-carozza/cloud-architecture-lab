# Concepts — Plan and Manage an Azure AI Solution (AI-102 Domain 1)

---

## Cost-Per-Token Attribution Across Processing Paths

### If you're 10 years old
Imagine you run a lemonade stand and you sell two sizes: a quick small cup (synchronous) and a big jug ordered in advance for a party (batch). They cost different amounts to make. If you want to know where your money is going, you need to track the cost of each size separately — you can't just add them all together and call it "lemonade costs." Cost-per-token attribution does the same thing for AI: it tracks how much each type of request actually costs so you can make smarter decisions.

### If you're an architect
Cost-per-token attribution is the practice of capturing and tagging LLM token consumption by processing path (synchronous vs. batch) so that costs can be analysed, budgeted, and reported at the workload level. In a gateway architecture, this means logging `prompt_tokens`, `completion_tokens`, and `cache_creation_tokens` from the provider's usage response to a telemetry sink (Application Insights, Log Analytics) alongside a `path` dimension (`"sync"` / `"batch"`).

The architectural value is governance: without path-level attribution, you cannot determine whether cost growth is driven by the interactive user-facing path or the offline batch path, and you cannot evaluate whether the 50% batch discount is actually being captured. Azure Cost Management operates at the resource level (e.g., Azure OpenAI deployment), not at the request level — gateway-layer attribution fills that gap.

**Why this matters in enterprise:** Cost transparency is a first-class governance requirement in enterprise AI. Finance teams and platform owners need per-workload cost data to make buy-vs-build and scale decisions. A gateway that cannot break down token cost by path is architecturally incomplete.

**Common beginner mistake:** Logging total tokens per request without tagging the processing path, then discovering months later that the "cheap batch jobs" represent 80% of spend because no one was routing correctly.

---

## Operationalizing a Generative AI Solution

### If you're 10 years old
Building a robot is fun, but keeping it running every day is harder work. You have to check that it's not broken, make sure it doesn't do anything weird, and fix it fast when something goes wrong. "Operationalizing" just means making sure your AI keeps working reliably day after day, not just on the first day you turn it on.

### If you're an architect
Operationalizing a generative AI solution means putting in place the instrumentation, processes, and controls that make it safe to run in production over time. The four pillars for an AI gateway are:

1. **Observability** — structured logs with correlation IDs, distributed traces, and metrics (latency p50/p95, error rate, token throughput). Without these, you are blind to degradation.
2. **Cost governance** — per-request token logging with path/provider dimensions, hard caps at submission boundaries, and budget alerts. Cost must be a first-class signal, not an afterthought.
3. **Resilience** — retry policies, circuit breakers, timeouts configured per processing path (tighter for synchronous, relaxed for batch).
4. **Routing correctness** — ensuring requests land on the right path (batch vs. synchronous) based on latency SLA, not by accident.

These are the same pillars the AI-102 "Plan and manage" domain tests: selecting the right tier, monitoring a deployed solution, implementing responsible AI, and managing costs.

**Why this matters in enterprise:** An AI solution that works in a demo but has no observability or cost controls is a liability, not an asset. Elite architects design operability in from day one — not as a phase-2 retrofit.

**Common beginner mistake:** Shipping an AI feature with application-level logging only (stdout/stderr), with no structured telemetry, no cost tracking, and no budget ceiling. The first production incident reveals the gap.

---

## Quota Management for Azure AI Services

### If you're 10 years old
Imagine your school gives each student 100 library books per month. If you use them all in one week, you can't borrow any more until next month. AI services work the same way — there's a monthly limit (quota), and if you hit it, requests start failing. Good architects plan ahead so they don't run out.

### If you're an architect
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

### If you're an architect
Model selection governance means choosing and documenting the right model for each workload based on: capability (does it handle the task accurately?), cost (tokens-per-dollar tradeoff), latency profile (acceptable for synchronous?), content safety (Azure AI Content Safety filters needed?), and compliance (data residency, model terms of use).

In a gateway architecture, the model selection decision should be explicit and documented — not left to callers. The routing configuration (`DefaultModel`, `BatchModel`, per-path overrides) is the governance artefact. Key AI-102 exam concepts: Azure AI Content Safety for filtering harmful outputs, Responsible AI principles (fairness, reliability, privacy, inclusiveness, transparency, accountability), and model evaluation (benchmarking against ground truth for quality regression detection).

**Why this matters in enterprise:** Undocumented model choices are a compliance and audit risk. If a model is changed without evaluation, quality regressions are invisible until a user complaint surfaces them.

**Common beginner mistake:** Using the same model for every workload because "it's the default" rather than matching model capability and cost to the specific task's requirements.
