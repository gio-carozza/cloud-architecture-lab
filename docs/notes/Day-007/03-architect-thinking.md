# Day 7 — Architect Thinking

## 1. The placement decision: inside vs. outside the provider boundary

The most important decision Day 7 makes is not "should we cache?" — that answer
is unambiguous. The decision is "where does the caching logic live?"

Two placements are defensible. ADR-009 documents both in full; the reasoning
worth internalizing here is the YAGNI application.

The temptation to introduce a `CachingChatModelProvider` decorator early comes
from a genuine instinct: keep caching strategy separate from transport. That
instinct is correct at scale, with multiple providers. With one provider, it
produces abstraction without abstraction value.

The specific trap: a decorator can decide *what* to cache ("the system prompt is
cacheable") but cannot decide *how* to annotate without provider-specific
knowledge. Anthropic uses `cache_control` content block annotations. Azure OpenAI
uses `prompt_cache_key`. Bedrock uses model-specific flags. These are not the same
concept with different spellings — they have different lifetimes, different
granularity (block vs. request vs. session), and different billing semantics.

An `ICacheAnnotator` interface designed today against Anthropic alone will be
shaped like Anthropic. When the second provider arrives, the interface will need
reshaping. The cost of a premature interface is: you build it against one example,
refactor it when the second example reveals a different shape, and then argue with
yourself about whether the original design was "close enough." Building against
two real examples avoids this entirely.

The rule to apply in future architectural decisions: don't extract an interface
from a single implementation. Wait for the second implementation to teach you the
interface's actual shape.

## 2. Cache observability is not cache reduction

Day 7's scope is "implement caching." The naive deliverable is: annotate the
system prompt, ship, tokens are cheaper. Done.

The architect-level deliverable is: annotate the system prompt AND make the cache's
behavior queryable, alertable, and trendable. The difference is `CacheHits` and
`CacheMisses` counters and `llm.cache.*` Activity tags.

Why does this matter? The cache can silently stop working. Anthropic's ephemeral
cache has a 5-minute TTL (approximate; undocumented precisely). If request volume
drops below the TTL renewal rate — say, 3 AM on a Sunday — the cache goes cold
and every call starts billing at full rate. Without instrumentation, this is
invisible until the invoice arrives.

With `ai.provider.cache.hits` and `ai.provider.cache.misses` as counters exported
to App Insights, KQL Query 8 computes hit rate in real time. An alert rule on
"cache hit rate < 10% over 30 minutes during business hours" is a defensible
production guardrail.

The principle: cost controls are only as good as their observability. A cache with
no metrics is a superstition. A cache with hit rate and savings metrics is a
managed system.

## 3. The minimum cacheable size constraint (operational gotcha)

Anthropic enforces a minimum cacheable block size — currently 1024 tokens for
Claude 3+ models. This is undocumented precisely and may change.

The practical implication: a `SystemPrompt` of 200 tokens will be annotated with
`cache_control` and submitted, but Anthropic will silently not cache it. The
response's `usage.cache_creation_input_tokens` will be 0. The gateway will observe
no cache hits, no cache misses, just normal billing — and it will look like the
caching code is broken when it is actually correct.

Day 7's verification step requires a ≥1100-token system prompt precisely to confirm
the mechanism end-to-end. The ADR documents this. The completion checklist enforces
it. This is the kind of operational constraint that belongs in the gotchas section
of a runbook, not just in the ADR.

A useful mental model: the cache annotation is a *hint* to Anthropic, not a
directive. Anthropic honors the hint only when the block meets its size threshold.
Design verification accordingly.

## 4. Why `SystemPrompt` lives in `AnthropicOptions`, not in `ChatRequest`

The system prompt is gateway-managed, not caller-managed. Callers of the AI gateway
post a user message; the gateway prepends the operational system prompt. Exposing
system prompt as a field on `ChatRequest` would:
- Leak an operational concern into a public contract
- Allow callers to override gateway-level instructions (a security issue at
  the enterprise layer)
- Make caching policy caller-dependent (different system prompts = different cache
  keys = cache fragmentation)

Keeping the system prompt in `AnthropicOptions` (bound from config/environment
variables) means it is:
- Controlled by the platform, not the consumer
- Changeable without code deployment (App Service env var)
- Guaranteed to be the same across all requests (cache key stability)

This is the enterprise pattern: the gateway owns the system prompt; callers own
the user message.

## 5. KQL for cache economics

Two queries the gateway now enables. Add these to `docs/standards/kql-cookbook.md`
as Query 8 and Query 9.

**Query 8 — Cache hit rate (last 1 hour)**
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

**Query 9 — Estimated savings (last 24 hours)**
```kql
// Assumes claude-opus-4-7 pricing: $15/M input tokens, $1.50/M cached read tokens
// Cached write is billed at 125% of input rate ($18.75/M)
let inputPricePerToken = 15.0 / 1000000;
let cacheReadPricePerToken = 1.50 / 1000000;
let cacheWritePricePerToken = 18.75 / 1000000;
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

## 6. What elite architects do differently

- They **instrument the toggle**, not just the feature. `EnablePromptCaching=false`
  is a valid production state (debugging, cost comparison). The logs should make
  it obvious which mode is active.

- They **write the verification step before the code**. "Cache is working" has a
  precise definition: `cache_read_input_tokens > 0` on the second identical
  request within the cache TTL window. If you can't state the verification
  criterion before you write the code, you don't understand what you're building.

- They **account for TTL in the SLO**. Anthropic's ~5-minute cache TTL means the
  cache hit rate metric is only meaningful for systems with request intervals
  shorter than 5 minutes. Low-traffic systems will see near-0% hit rate at night
  not because caching is broken but because the TTL is expiring. Alert thresholds
  must account for traffic patterns.

## 8. Verification finding: Claude 4 API format changes (discovered Day 7)

Three issues surfaced during local verification that are not obvious from the Anthropic
documentation and will catch any engineer migrating from Claude 3 to Claude 4 models.

### Cache_control requires a TTL on Claude 4 models

The original implementation used `{"type":"ephemeral"}` without a `ttl` field.
This format was correct for Claude 3 models. On Claude 4 models, it is silently ignored:
the API accepts the request without error, returns `cache_creation_input_tokens: 0`,
and bills at full rate. The fix is `{"type":"ephemeral","ttl":"1h"}` (or `"5m"`).

The silent failure mode is the dangerous part. The gateway's observability layer works
correctly — it observes zero cache activity. But this is indistinguishable from the
legitimately warm case where an identical request hasn't been sent yet. Without knowing
the TTL requirement, an operator will conclude "caching is working, just no repeat
requests yet" when in fact caching is entirely inoperative.

### The response format evolved: nested `cache_creation` object

Claude 4 usage responses include both:
- The old flat field: `"cache_creation_input_tokens": N` (present for backward compat)
- A new nested object: `"cache_creation": {"ephemeral_1h_input_tokens": N, "ephemeral_5m_input_tokens": N}`

The nested format separates creation tokens by TTL tier, enabling multi-tier cache accounting.
`TryExtractUsage` was updated to fall back to summing the nested fields when the old flat field
is absent or 0, maintaining forward compatibility with future API format changes.

### Model ID validation is not a 4xx

The user-secrets model was set to `claude-opus-4-6`, which is not a valid current
Anthropic model ID (valid IDs: `claude-opus-4-8`, `claude-sonnet-4-6`, `claude-haiku-4-5-20251001`).
The API accepted every request and returned valid content with HTTP 200. Cache tokens were
consistently 0. This is because the model ID is silently aliased or routed to a model that
does not support the new caching format.

**Rule:** when prompt caching produces no cache tokens after a verified-correct request,
check the model ID before debugging the code.

## 7. Common beginner mistakes

- **Annotating a short system prompt and concluding caching is broken.** The
  minimum cacheable size (1024 tokens) is the hidden prerequisite. Always verify
  with a known-sufficient prompt.
- **Treating `CacheMisses` as failures.** Cache misses are normal — the first
  request after a cold start or TTL expiry is always a miss. The metric to watch
  is the miss-to-hit ratio over time, not the miss count in isolation.
- **Putting the system prompt in `ChatRequest`.** See section 4 above.
- **Hardcoding the system prompt in code.** If it lives in code, changing it
  requires a redeploy. If it lives in config, it's operational — changeable via
  App Service env var with an immediate `Restart-AzWebApp`.
