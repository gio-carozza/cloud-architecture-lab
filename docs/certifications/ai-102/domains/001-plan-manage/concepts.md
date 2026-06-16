# Concepts — Plan and Manage an Azure AI Solution (AI-102 Domain 1)

---

<!-- Day 6 Additions: structured logging, correlation IDs, token telemetry as metrics, error classification -->

## Structured Logging and Distributed Tracing for AI Workloads

### If you're 10 years old

Imagine every request to your AI app leaves a trail of breadcrumbs. Each breadcrumb has the exact time and what happened. If something goes wrong, you follow the trail backwards and find exactly where things went sideways. "Structured logging" means each breadcrumb has labelled pockets — a pocket for the time, a pocket for who made the request, a pocket for how long it took — so you can search through millions of breadcrumbs instantly.

### If you're a CEO

When an AI feature breaks at 2am, the on-call engineer needs to find the problem in minutes, not hours. Structured logging is the investment that makes this possible. Without it, the engineer is reading unstructured text looking for clues. With it, they run a query and see the problem in 30 seconds. The ROI is measured in mean-time-to-resolution — and indirectly in customer trust when incidents are resolved quickly.

### If you're an Engineer

In .NET 8, implement structured logging via Serilog + Azure Monitor OpenTelemetry exporter: `UseSerilog()` in `Program.cs`, `services.AddOpenTelemetry().UseAzureMonitor()`. Each log call is a structured record: `_logger.LogInformation("LLM call completed {Provider} {Model} {InputTokens} {OutputTokens} {LatencyMs}", ...)`. These become queryable dimensions in Application Insights: `traces | where customDimensions["Provider"] == "anthropic"`. Correlation IDs: use `CorrelationIdMiddleware` to read `X-Correlation-Id` from request headers (or generate a new GUID), store in `HttpContext.Items`, and enrich all log lines via `LogContext.PushProperty("CorrelationId", ...)`. The correlation ID must travel the entire request — from incoming HTTP through the LLM provider call — so any failure in the chain is linked to the original request in a single KQL query.

### If you're an Architect

Structured logging and distributed tracing are the foundation of operational AI. Three design principles: (1) **correlation as a first-class field** — every log line, metric, and trace span must carry the same correlation ID so a single incident can be stitched across the gateway, the LLM provider call, and any downstream effects; (2) **semantic logging** — a log saying `"completed"` is not queryable; a log with `{"event":"llm_call_completed","provider":"anthropic","model":"claude-sonnet-4-6","input_tokens":1200,"output_tokens":340,"latency_ms":1850}` enables p95 latency queries by model, error rate by provider, and cost trending by path; (3) **log levels as routing signals** — `Information` for normal operations, `Warning` for retry attempts and degraded paths, `Error` for unhandled exceptions. Misclassifying everything as `Information` floods logs with noise and renders alert rules on `Error`-level events worthless. At enterprise scale, structured logging is the prerequisite for all downstream analytics — cost attribution, SLO compliance, and anomaly detection all depend on consistent, labelled telemetry from day one. Common beginner mistake: logging full prompt and completion text — this creates PII exposure risk, inflates log ingestion costs, and may violate data governance policies. Log token counts, model IDs, and latency; never log actual prompt content unless under a controlled access and retention policy.

---

## Token Telemetry as a Cost Metric

### If you're 10 years old

Imagine you're running a candy store and each AI answer costs a certain number of candies (tokens). If you don't count how many candies each type of question uses, you'll run out of candies and not know why. Token telemetry means counting the candies per question, labelling them by type, and storing those counts so you can see where they're all going.

### If you're a CEO

Token usage is your direct AI cost driver — each token costs money. Without token telemetry, you have a monthly invoice but no breakdown of which feature, which user type, or which processing path is driving the bill. With it, you can identify that 60% of your AI spend comes from 5% of your requests — and make targeted decisions about caching, batching, or prompt optimisation that dramatically reduce costs. Unmonitored token spend grows silently and surprises every team that skips this.

### If you're an Engineer

Log token usage as a structured custom event on every LLM call with dimensions: `{ "llm.provider": "anthropic", "llm.model": "claude-sonnet-4-6", "llm.tokens.input": 1200, "llm.tokens.output": 340, "llm.tokens.cache_read": 800, "llm.path": "sync" }`. Additionally emit as OpenTelemetry `Counter<long>` instruments — `llm.tokens.input` and `llm.tokens.output` counters aggregate in `customMetrics`, enabling time-series budget queries. KQL for hourly cost trend: `customEvents | where name == "llm_call_completed" | summarize total_input=sum(tolong(customDimensions["llm.tokens.input"])), total_output=sum(tolong(customDimensions["llm.tokens.output"])) by bin(timestamp, 1h)`. The `path` dimension (`sync`/`batch`) is critical — without it, you cannot separate interactive cost from batch cost.

### If you're an Architect

Token telemetry occupies a dual role in AI observability: it is both a **cost signal** (tokens × price-per-token = direct spend) and a **quality signal** (output-token growth may indicate prompt regression or model behavior change). The architectural design principle is to treat token counts as **metrics**, not log fields: metrics aggregate cheaply at O(1) per time-bucket; logs require O(N) scan for aggregation. Use `Counter<long>` instruments for input/output token totals (enables time-series cost graphs) and log the per-request breakdown as a custom event for root-cause queries. The `model` and `provider` dimensions on both the metric and the event enable cost attribution across the full provider strategy when additional models are added. For AI-102: the exam tests whether you know that Azure OpenAI usage is tracked in Azure Cost Management at the resource level, but **request-level attribution** (by path, model, user, or feature) requires application-layer telemetry — there is no built-in Azure feature that provides this automatically. Common beginner mistake: emitting token counts only to Application Insights `traces` as unstructured text, making it impossible to aggregate or alert on cost trends without manual log parsing.

---

## Error Classification for AI Workloads

### If you're 10 years old

When something goes wrong, there are different kinds of "wrong." If you spell a word incorrectly, no amount of trying again will fix it — it's a "you" problem. If the internet goes down for a moment, trying again in a few seconds might work — it's a "temporary" problem. AI gateways classify errors the same way: "your fault", "their temporary fault", "they won't let you in" — so the system knows whether to retry, report, or give up.

### If you're a CEO

Not all AI errors are equal. An authentication error should alert the team and stop retrying immediately — retrying wastes budget and time on something a human must fix. A transient provider error should retry silently and only alert if retries are exhausted. Proper error classification means your on-call alerts are accurate signals, not noise, and costs are not wasted on futile retries.

### If you're an Engineer

Classify LLM provider errors into four buckets and handle each differently:

- **4xx Client error** (400, 422): caller sent bad input — return 400 to caller, log at `Warning`, no retry
- **401/403 Auth error**: credential problem — return 401/403, log at `Error`, alert immediately, no retry
- **429 Throttle**: rate limit — retry with backoff honouring `Retry-After`, log at `Warning`
- **5xx Transient**: provider fault — retry with jitter, log at `Warning` per attempt, `Error` on final failure
- **Timeout / IOException**: network failure — retry with jitter, log at `Warning`

Map `HttpRequestException` + HTTP status code to the class in the exception handler. Add an `error_class` dimension to every telemetry event. Safe error contract for callers: return only `{ "error": "An unexpected error occurred.", "correlationId": "..." }` — never return stack traces or provider error messages.

### If you're an Architect

Error classification is the prerequisite for actionable alerting in an AI gateway. Without it, all failures are homogenised into a single error-rate metric that cannot distinguish "the API key expired" from "Anthropic had a 10-second blip." Four classes are the minimum viable taxonomy: **Client** (caller bug), **Auth** (credential problem requiring human action), **Throttle** (quota issue requiring scaling or batching), **Transient** (provider temporary fault requiring retry). Each class has a different retry policy, a different alert severity, and a different remediation runbook. The architecture decision: encode error classification in the provider abstraction layer (`IChatModelProvider`) so all implementations classify consistently — rather than leaving classification to individual callers. At enterprise scale, error classification feeds the SLO burn-rate calculation: transient errors that stay below SLO thresholds are acceptable operational noise; auth errors are never acceptable and always actionable. Common beginner mistake: logging all errors as `Error` level with no classification, then paging on every 400 bad request from a misconfigured client, drowning the on-call rotation in noise.

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

<!-- Day 7 Additions: prompt caching for cost management, cache hit rate as operational metric -->

## Prompt Caching for AI Workload Cost Management

### If you're 10 years old

Imagine your teacher reads the same 5-page story to 30 students in a row. Instead of reading it fresh each time, what if they memorised it once and just recited it? Prompt caching works the same way — instead of sending a long instruction to the AI every single time someone asks a question, the AI remembers the instruction after the first call and charges you much less for every subsequent read.

### If you're a CEO

For every AI request, you pay for every word of the instruction the system sends. If your system prompt is 2,000 words and you make 10,000 calls per day, you're paying for 20 million instruction-words daily. Prompt caching cuts that 90% — the AI remembers the instruction after the first call and charges 10× less for each subsequent read. This is one of the highest-ROI cost reductions available in production AI systems, requiring only a single configuration change.

### If you're an Engineer

Implement prompt caching by annotating the system prompt as a cacheable content block. In Anthropic's API: instead of a string `"system": "..."`, send `"system": [{"type":"text","text":"...","cache_control":{"type":"ephemeral","ttl":"1h"}}]`. Requirements: (1) the cached block must be ≥1024 tokens — shorter blocks are silently ignored; (2) for Claude 4 models (`claude-sonnet-4-6`, `claude-opus-4-8`, `claude-haiku-4-5-20251001`), the `ttl` field is mandatory — `{"type":"ephemeral"}` without TTL produces 0 cache tokens and full billing with no error; (3) read tokens appear in `usage.cache_read_input_tokens`, creation tokens in `usage.cache_creation_input_tokens` — log both as custom dimensions. Common error: expecting `cache_creation_input_tokens > 0` on every request — it is non-zero only on the first call (and after TTL expiry); subsequent cache hits show `cache_read_input_tokens`.

### If you're an Architect

Prompt caching is an API-level optimization where the LLM provider stores a hashed copy of annotated content blocks after the first request. Subsequent requests with the same leading content up to the cache boundary pay a fraction of the creation cost (Anthropic: ~10% of creation price per cache read). The key architectural decisions are: (1) **placement** — caching logic belongs inside the provider boundary (`ClaudeApiClient`), not in a middleware decorator above the provider seam, because `cache_control` is Anthropic-specific and `ChatRequest` must remain provider-agnostic (ADR-009); (2) **observability** — cache creation and read token counts must be logged with the same telemetry dimensions as regular tokens so effectiveness is queryable; (3) **minimum size** — the 1024-token minimum means short system prompts offer no benefit; the optimization is most valuable for large, stable instruction blocks. Enterprise implication: at 10,000 calls/day with a 3,000-token system prompt at $3/M tokens, the cache reduces input cost from ~$90/day to ~$9/day — a 90% reduction that accumulates silently if not measured. Common beginner mistake: implementing caching without measuring it, operating on the assumption that cache hits are occurring when a misconfigured TTL or wrong model ID silently zeroes all cache reads.

---

## Cache Hit Rate as an Operational Metric

### If you're 10 years old

Imagine you keep score every time the AI uses its memory (cache hit) vs. every time it has to re-read the instructions from scratch (cache miss). If you're getting lots of hits, you're saving money. If you're getting all misses, either the instructions keep changing or something is broken. Cache hit rate is just that score — and measuring it is how you prove the savings are real.

### If you're a CEO

Cache hit rate is the number that tells you whether your cost optimization is actually working. If your AI is set up for caching but the hit rate is 0%, you're paying full price for every request and the configuration failed silently. A 90% hit rate means 90% of your input-token cost on those calls has been eliminated. Without measuring this number, you cannot prove ROI to finance or detect when the savings disappear.

### If you're an Engineer

Log `cache_read_input_tokens` and `cache_creation_input_tokens` as custom dimensions on every LLM call span. Cache hit rate KQL: `dependencies | where name == "claude.chat.api" | summarize hits=countif(toint(customDimensions["llm.cache.read_tokens"]) > 0), total=count() by bin(timestamp, 1h) | extend hitRate=hits*100.0/total`. Estimated savings KQL: `dependencies | where name == "claude.chat.api" | summarize cache_reads=sum(toint(customDimensions["llm.cache.read_tokens"])) by bin(timestamp, 1d) | extend savings_usd=(cache_reads/1000000.0)*2.70`. Alert candidate: cache hit rate < 10% during business hours — likely indicates broken cache configuration (wrong TTL, wrong model ID, system prompt below threshold).

### If you're an Architect

Cache hit rate is a second-order cost metric: it measures how efficiently your caching configuration is reducing the primary cost metric (input tokens). The design principle is to treat cache metrics as first-class telemetry. Two reasons: (1) **silent failures** — a missing TTL on Claude 4, a wrong model ID, or a sub-threshold prompt all produce zero cache reads with no API error; without hit-rate monitoring, you pay full price indefinitely while believing you've saved 90%; (2) **trend detection** — a hit rate degrading from 90% to 20% over a week signals that the system prompt is changing too frequently, the TTL is too short, or deployments are routing to multiple model versions. Both diagnoses require the hit-rate time series. For AI-102: the exam tests whether you understand that enabling a caching feature is not the same as the feature working — observability of the cache is a separate, required design step. Common beginner mistake: checking the provider billing dashboard monthly and concluding caching is working, rather than instrumenting hit rate as a real-time operational signal.

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
