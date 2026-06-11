# Day 7 — Posture Check

> Honest answers only. The graveyard is more valuable than the trophy case.
> Fill this at the END of the day, BEFORE marking the day complete.

## 1. Whose problem did I actually solve today?

The **FinOps engineer** who asks "what does this gateway cost per hour and is the cache actually working?" — they now have a KQL query instead of a monthly invoice guess.

The **on-call architect** who needs to catch "caching silently stopped working at 3 AM" before it shows up on the bill — the `ai.provider.cache.hits` counter is the alertable signal.

## 2. What would I refuse to ship if I were the only one in the room?

The original `{"type":"ephemeral"}` cache_control without a TTL. On Claude 4 models it is silently ignored — the API accepts the request, returns `cache_creation_input_tokens: 0`, and bills at full rate. No error, no log, no alert. The observability is working correctly: it's measuring zero cache activity. But that looks identical to "caching is enabled and I just haven't sent a second request yet." An operator cannot distinguish a warm cache from a broken one.

The fix was not optional. Shipped as `{"type":"ephemeral","ttl":"1h"}`.

## 3. What did I try, fail at, and learn?

**Model name mismatch.** User-secrets had `claude-opus-4-6`. That model ID accepts requests and returns content, but returns `cache_creation_input_tokens: 0` on every call. The valid current Sonnet model (`claude-sonnet-4-6`) activates caching immediately with the same system prompt. Lesson: an invalid or wrong model ID can fail silently in ways unrelated to HTTP status codes.

**Claude 4 cache_control format changed.** The bare `{"type":"ephemeral"}` format worked for Claude 3 models. Claude 4 requires an explicit TTL: `{"type":"ephemeral","ttl":"1h"}` or `{"type":"ephemeral","ttl":"5m"}`. The API does not return an error for the TTL-less format — it just doesn't cache. This is a silent behavioral regression when migrating from Claude 3 to Claude 4 models.

**Anthropic usage response format evolved.** Claude 4 responses include both the old flat field (`cache_creation_input_tokens`) AND a new nested object (`cache_creation: {ephemeral_1h_input_tokens, ephemeral_5m_input_tokens}`). The original `TryExtractUsage` only read the flat field, which would have been correct for Claude 3 but needed the fallback for future-proofing.

All three were fixed and verified in the same session.

> **Graveyard entry:** `claude-opus-4-6` as a model alias — appears valid (200 responses), does not activate prompt caching, not a recognized current model ID.

## 4. Could I explain today's work to a 10-year-old AND defend it at a doctorate level?

### 10-year-old version

The AI gateway is like a homework helper. Every time a student asks a question, the helper reads a big instruction booklet from scratch — that's expensive. Prompt caching is like saying "remember this booklet for the next hour." The first student who asks pays for reading the booklet. Every student after that for the next hour gets the answer faster AND cheaper, because the helper already has the booklet memorized. Today we wired up a counter that tells us exactly how often the helper is using the memorized version vs. re-reading from scratch.

### Doctorate-level version

Anthropic implements prompt caching as a server-side KV store keyed on a content prefix hash. The `cache_control: {"type":"ephemeral","ttl":"1h"}` annotation marks the end of the stable prefix. On the first request, the KV entry is written (`cache_creation_input_tokens` > 0, billed at 125% of base input rate). Subsequent requests within TTL key-hit the store (`cache_read_input_tokens` > 0, billed at ~10% of base input rate). The TTL field distinguishes the 5-minute ephemeral tier (for hot transactional prompts) from the 1-hour tier (for operational system prompts).

The gateway surfaces this through Activity tags (`llm.cache.read_tokens`, `llm.cache.creation_tokens`) on the `claude.chat.api` span, and via counters (`ai.provider.cache.hits`, `ai.provider.cache.misses`) exported to App Insights. KQL Query 8 computes the real-time hit rate; Query 9 estimates dollar savings. An alert on sustained hit rate below threshold (during business hours, adjusted for traffic envelope) is the production guardrail — the difference between discovering cache breakage in 30 minutes vs. on the next invoice.
