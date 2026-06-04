# KQL Cookbook

Reusable Kusto Query Language queries for the Lab.Observability.Api gateway.
Run against the Application Insights workspace `appi-ai-lab-api-dev-eastus-gio`
or its underlying Log Analytics workspace `law-ai-lab-dev-eastus-gio`.

Save each query as a Query Pack entry in the workspace for one-click reuse.

## Conventions
- Replace `0HN...` placeholders with real correlation IDs from response headers
- All queries assume W3C Trace Context propagation (active since Day 6)
- Custom dimensions used: `CorrelationId`, `llm.tokens.input`, `llm.tokens.output`,
  `llm.provider`, `llm.model`, `llm.latency_ms`,
  `llm.cache.read_tokens` (Day 7+), `llm.cache.creation_tokens` (Day 7+),
  `batch.job_id` (Day 8+), `batch.request_count` (Day 8+)

---

## 1. Latency p50/p95/p99 for /api/ai/chat (last hour)
```kql
requests
| where timestamp > ago(1h)
| where url contains "/api/ai/chat"
| summarize
    p50 = percentile(duration, 50),
    p95 = percentile(duration, 95),
    p99 = percentile(duration, 99),
    count = count()
  by bin(timestamp, 5m)
| render timechart
```

## 2. Top 10 slowest chat requests (last 24h) with correlation IDs
```kql
requests
| where timestamp > ago(24h)
| where url contains "/api/ai/chat"
| top 10 by duration desc
| project
    timestamp,
    duration_ms = duration,
    resultCode,
    correlationId = tostring(customDimensions.CorrelationId),
    operation_Id
```

## 3. Error rate by hour (5xx + dependency failures)
```kql
requests
| where timestamp > ago(24h)
| extend bucket = bin(timestamp, 1h)
| summarize
    total = count(),
    errors = countif(success == false)
  by bucket
| extend error_rate_pct = round(100.0 * errors / total, 2)
| render timechart
```

## 4. Token usage per hour (input + output)
```kql
// Activity spans land in dependencies, NOT traces
dependencies
| where timestamp > ago(24h)
| where name == "claude.chat.api"
| extend inputTokens  = toint(customDimensions["llm.tokens.input"])
| extend outputTokens = toint(customDimensions["llm.tokens.output"])
| summarize
    total_input  = sum(inputTokens),
    total_output = sum(outputTokens)
  by bin(timestamp, 1h)
| render timechart
```

## 5. Provider latency vs gateway latency
```kql
let gateway =
    requests
    | where url contains "/api/ai/chat"
    | project op = operation_Id, gateway_ms = duration;
let provider =
    dependencies
    | where target contains "anthropic"
    | project op = operation_Id, provider_ms = duration;
gateway
| join kind=inner provider on op
| extend overhead_ms = gateway_ms - provider_ms
| summarize
    avg_gateway = avg(gateway_ms),
    avg_provider = avg(provider_ms),
    avg_overhead = avg(overhead_ms)
  by bin(now(), 1m)
```

## 6. Failures classified by category
```kql
exceptions
| where timestamp > ago(24h)
| extend category = case(
    type contains "Timeout", "timeout",
    type contains "HttpRequestException", "transport",
    type contains "Unauthorized", "auth",
    "other")
| summarize count = count() by category
| render piechart
```

## 7. Trace a single correlation ID end-to-end
```kql
union requests, dependencies, traces, exceptions
| where customDimensions.CorrelationId == "0HN..."
       or operation_Id == "0HN..."
| project timestamp, itemType, name, message, resultCode, duration
| order by timestamp asc
```

## 8. Cache hit rate (last 1 hour) — Day 7+
```kql
dependencies
| where name == "claude.chat.api"
| where timestamp > ago(1h)
| extend
    cacheReadTokens = toint(customDimensions["llm.cache.read_tokens"]),
    cacheCreationTokens = toint(customDimensions["llm.cache.creation_tokens"])
| summarize
    totalRequests = count(),
    cacheHits = countif(cacheReadTokens > 0),
    cacheMisses = countif(cacheCreationTokens > 0 and (isnull(cacheReadTokens) or cacheReadTokens == 0))
| extend hitRate = round(todouble(cacheHits) / todouble(totalRequests) * 100, 1)
| project totalRequests, cacheHits, cacheMisses, hitRate
```

## 9. Estimated token cost savings (last 24 hours) — Day 7+
```kql
// claude-sonnet-4-6 pricing: $3/M input tokens, $0.30/M cached read tokens
// Cached write billed at 125% of input rate ($3.75/M)
// Update price constants when switching models
let inputPricePerToken = 3.0 / 1000000;
let cacheReadPricePerToken = 0.30 / 1000000;
let cacheWritePricePerToken = 3.75 / 1000000;
dependencies
| where name == "claude.chat.api"
| where timestamp > ago(24h)
| extend
    inputTokens = toint(customDimensions["llm.tokens.input"]),
    cacheReadTokens = toint(customDimensions["llm.cache.read_tokens"]),
    cacheCreationTokens = toint(customDimensions["llm.cache.creation_tokens"])
| summarize
    totalInputTokens = sum(inputTokens),
    totalCacheReadTokens = sum(cacheReadTokens),
    totalCacheCreationTokens = sum(cacheCreationTokens)
| extend
    actualCost = (todouble(totalInputTokens) * inputPricePerToken)
               + (todouble(totalCacheReadTokens) * cacheReadPricePerToken)
               + (todouble(totalCacheCreationTokens) * cacheWritePricePerToken),
    uncachedCost = todouble(totalInputTokens + totalCacheReadTokens) * inputPricePerToken,
    savingsUSD = uncachedCost - actualCost
| project totalInputTokens, totalCacheReadTokens, totalCacheCreationTokens,
          actualCost = round(actualCost, 4),
          uncachedCost = round(uncachedCost, 4),
          savingsUSD = round(savingsUSD, 4)
```

## 10. Batch job activity and cost vs sync equivalent — Day 8+

> **Important:** batch latency (minutes to hours) is expected behavior, not a
> degradation signal. Do NOT alarm on `batch.submit`, `batch.poll`, or
> `batch.retrieve` span duration. The `alert-ai-gateway-5xx-rate` rule and
> interactive latency thresholds do not apply to batch spans.

```kql
// Batch submissions and completions per hour
customMetrics
| where timestamp > ago(24h)
| where name in (
    "ai.provider.batch.submitted",
    "ai.provider.batch.completed",
    "ai.provider.batch.result_count")
| summarize total = sum(valueSum) by name, bin(timestamp, 1h)
| render timechart
```

```kql
// Batch cost vs synchronous equivalent (last 24h)
// Batch is 50% of synchronous rate — savings are unconditional (no cache hit
// dependency). avgInputTokensPerRequest is an approximation; refine once you
// have real token logs from batch spans.
let inputPricePerMillion = 3.0;          // claude-sonnet-4-6: $3/1M input tokens
let avgInputTokensPerRequest = 500.0;    // approximation
customMetrics
| where timestamp > ago(24h)
| where name == "ai.provider.batch.result_count"
| summarize batchResultCount = sum(valueSum)
| extend
    syncEquivalentUSD  = batchResultCount * avgInputTokensPerRequest * (inputPricePerMillion / 1000000),
    batchCostUSD       = batchResultCount * avgInputTokensPerRequest * (inputPricePerMillion / 1000000) * 0.5,
    savingsUSD         = batchResultCount * avgInputTokensPerRequest * (inputPricePerMillion / 1000000) * 0.5
| project
    batchResultCount,
    syncEquivalentUSD  = round(syncEquivalentUSD, 4),
    batchCostUSD       = round(batchCostUSD, 4),
    savingsUSD         = round(savingsUSD, 4)
```