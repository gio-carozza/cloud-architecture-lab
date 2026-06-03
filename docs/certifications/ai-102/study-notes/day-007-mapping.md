# Day 7 → AI-102 Mapping

## What Day 7 covered (build side)
- Anthropic prompt caching via `cache_control: {"type":"ephemeral","ttl":"1h"}` annotation
- `AnthropicOptions.EnablePromptCaching` — operational toggle, no redeploy required
- `AnthropicOptions.SystemPrompt` — gateway-managed system prompt kept in config, not code
- `cache_read_input_tokens` / `cache_creation_input_tokens` extracted from response usage
- `ai.provider.cache.hits` and `ai.provider.cache.misses` counters in App Insights
- `llm.cache.read_tokens` / `llm.cache.creation_tokens` Activity tags on `claude.chat.api` span
- KQL Query 8 (cache hit rate) and Query 9 (estimated savings) in App Insights
- Verified live: 50% hit rate, cache creation 1488 tokens, cache read 1488 tokens in App Insights

## AI-102 objectives directly exercised

### Domain 1 — Plan and Manage an Azure AI Solution
- [x] **Manage costs for Azure AI services**
  - Prompt caching reduces input token costs by ~90% on cached content. The pattern
    (annotate stable content, measure hit rate, surface savings as a metric) is identical
    for Azure OpenAI prompt caching. Token cost is a first-class architectural constraint —
    not a billing-statement afterthought.
  - The AI-102 exam expects you to understand *which Azure tool gives you cost visibility*
    (App Insights / Log Analytics KQL) and *which levers reduce cost* (caching, batching,
    model tier selection).
- [x] **Monitor an Azure AI service**
  - Cache hit rate is an operational metric, not just a log field. An alert rule on
    "hit rate < 10% during business hours" is a defensible production guardrail.
    Without this, a silent cache regression is discovered on the monthly invoice.
  - Application Insights `dependencies` table, `claude.chat.api` span, custom dimensions
    `llm.cache.read_tokens` and `llm.cache.creation_tokens`.

### Domain 6 — Implement Generative AI Solutions
- [x] **Optimize Azure OpenAI usage**
  - Azure OpenAI supports prompt caching for compatible models. The mechanism differs
    in annotation format (no explicit `cache_control` block — Azure OpenAI caches
    automatically based on prefix hash for supported models), but the OBSERVABILITY
    pattern is identical: measure `cached_tokens` in usage response, compute hit rate,
    alert on degradation.
  - AI-102 exam question pattern: "You want to reduce the per-request input token cost
    for a generative AI workload where the system prompt is static. What should you do?"
    → Enable prompt caching / use a cached deployment tier.
- [x] **Monitor and optimize generative AI deployments**
  - Token usage attribution: `input_tokens`, `output_tokens`, `cached_tokens` in the
    Azure OpenAI response correspond to `llm.tokens.input`, `llm.tokens.output`,
    `llm.cache.read_tokens` in our gateway. Same pattern, different provider-specific
    field names.

## Concepts at two levels

---
## Prompt Caching

### If you're 10 years old
Imagine every time you called a customer service line, the rep had to read a 10-page
manual before answering you. Prompt caching is like giving the rep a "cheat sheet"
they can keep on their desk. The first caller pays for printing the cheat sheet. Every
caller after that gets a faster, cheaper answer because the cheat sheet is already there.

### If you're an architect
Anthropic (and Azure OpenAI for compatible models) implements prompt caching as a
server-side KV store keyed on a prefix hash of the request content. Annotating a
content block with `cache_control: {"type":"ephemeral","ttl":"1h"}` signals the
provider to write that block to the KV store on first use (`cache_creation_input_tokens`
billed at 125% of base rate). Subsequent requests within TTL key-hit the store
(`cache_read_input_tokens` billed at ~10% of base rate). The 90% cost reduction on
the cached portion is the enterprise ROI case. The architect-level obligation is to
make this observable: a cache that silently stops working looks exactly like a cache
that has had no repeat requests, and the only way to distinguish them is a hit-rate
metric with an alert threshold.

**Why it matters in enterprise:** At 10,000 daily requests with a 3,000-token system
prompt at $3/M, uncached cost is $90/day. Cached cost drops to ~$9/day. Over a year,
that's ~$30,000 in savings for one gateway. At enterprise scale (100 gateways), it's
a material line item justifying dedicated FinOps ownership.

**Common beginner mistake:** Annotating a system prompt that is shorter than the
provider's minimum cacheable size (1024 tokens for most Anthropic models) and concluding
"caching doesn't work." The provider silently returns `cache_creation_input_tokens: 0`.
Always verify with a prompt known to exceed the threshold.

---
## Cache Hit Rate as an SLO Metric

### If you're 10 years old
If your school library started tracking how often kids found a book they needed vs.
had to order it from another library, that "find rate" would tell the librarian whether
the collection was working. Cache hit rate does the same thing for your AI gateway —
it tells you whether the "pre-loaded answers" are actually being used.

### If you're an architect
Cache hit rate (`cacheHits / totalRequests * 100`) is a leading indicator of cost
efficiency. It belongs on an SLO dashboard alongside latency and error rate — not buried
in logs. The distinction matters because:
- A log field is queried reactively (you look when something seems wrong)
- An SLO metric is queried proactively (an alert fires before you look)

Azure Monitor alert rules on custom metrics or KQL scheduled queries can fire when
hit rate drops below a threshold during a defined time window. The threshold must
account for traffic patterns: low-traffic overnight windows will naturally see lower
hit rates as TTLs expire between requests — alerting on 24h windows or gating on
minimum request count avoids false positives.

**Common beginner mistake:** Treating `CacheMisses` as a failure metric. Cache misses
are expected on cold start and after TTL expiry. The signal is miss-to-hit ratio over
time in a traffic-bearing window, not absolute miss count.

---
## Token Cost Attribution

### If you're 10 years old
If you and three friends all use the same shared phone plan, someone needs to track
who made which calls so the bill gets split fairly. Token attribution is the same idea
for AI costs — tracking which team, feature, or user consumed how many tokens, so
you know who to charge or where to optimize.

### If you're an architect
In enterprise AI gateways, every request produces a usage tuple: `(input_tokens,
output_tokens, cache_read_tokens, cache_creation_tokens)`. These fields, surfaced as
Activity tags on distributed traces, flow into Application Insights and become queryable
via KQL. A well-designed gateway adds a `caller_id` or `team_id` tag at the
orchestration span level, enabling `summarize sum(inputTokens) by teamId` in App
Insights — the foundation for internal chargeback.

**Why it matters in enterprise:** FinOps teams cannot optimize what they cannot see.
Token-level telemetry is the primitive that enables cost allocation, budget forecasting,
and anomaly detection. Without it, the first signal of runaway AI cost is an invoice.

## Likely exam-style questions Day 7 answers

**Q1.** *You are building an Azure OpenAI application. The system prompt is 2,000 tokens
and is identical for every user request. You want to reduce input token costs. What is
the most effective approach?*
→ **Enable prompt caching / use a deployment that supports prefix caching.** The system
prompt will be cached after the first request, reducing cached-portion cost to ~10%.

**Q2.** *Your generative AI gateway is configured with prompt caching. After deploying a
new version, engineers notice input token costs increased 10x. No errors were reported.
What should you check first?*
→ **Cache hit rate in Application Insights.** A silent cache miss (TTL expiry, system
prompt changed, minimum token threshold not met) would cause all requests to bill at
full rate with no error surfaced.

**Q3.** *Which Azure service should you query to compute the cache hit rate for an Azure
OpenAI-backed workload?*
→ **Application Insights / Log Analytics (KQL against the `dependencies` table).** Cache
token counts are surfaced as custom dimensions on the AI provider dependency span.

**Q4.** *You want to implement chargeback for AI token costs across five internal teams
using a shared Azure OpenAI deployment. What must the gateway include?*
→ **A caller or team identifier propagated as a custom dimension on telemetry spans,**
so KQL can group token usage by team. The usage data comes from the OpenAI response
`usage` object; attribution requires the gateway to tag requests with team context.

**Q5.** *An AI workload's system prompt changes daily. Prompt caching is enabled. A
developer says "caching is pointless because our prompt changes." What is the correct
architectural response?*
→ **Cache only the stable portion of the prompt.** Multi-block caching allows annotating
a stable prefix (company context, safety rules) separately from the dynamic daily
content. The stable prefix caches; the dynamic portion bills normally. This is a
design decision at annotation time, not a binary cache-on/off choice.

## Gaps to study (NOT covered by Day 7 — read on Microsoft Learn)
- Azure OpenAI Provisioned Throughput Units (PTU) — reserved capacity vs pay-as-you-go
- Azure OpenAI prefix caching (automatic, no annotation required — differs from Anthropic)
- Content Safety integration for cost-responsible filtering before expensive model calls
- Azure AI Foundry cost management dashboard
- Diagnostic settings routing to Storage Account for long-term cost archival
- Batch API for non-latency-sensitive workloads (Day 8 target)

## Action items
- [ ] Read MS Learn: "Optimize Azure OpenAI costs" — learn.microsoft.com/azure/ai-services/openai/how-to/prompt-caching
- [ ] Read MS Learn: "Monitor Azure OpenAI" — usage metrics and cost dashboards
- [ ] Note: Azure OpenAI prompt caching minimum is 1024 tokens (same as Anthropic 3.x); verify current threshold before exam
- [x] KQL Queries 8 and 9 verified in live App Insights — cache hit rate and savings queries working (`docs/standards/kql-cookbook.md`)
- [x] Live telemetry confirmed: `llm.cache.creation_tokens=1488` on first request, `llm.cache.read_tokens=1488` on second
