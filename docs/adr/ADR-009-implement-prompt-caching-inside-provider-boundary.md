# ADR-009: Implement Prompt Caching Inside the Provider Boundary

## Status
Accepted

## Date
2026-05-06

## Related
- ADR-005-introduce-provider-abstraction-for-claude-integration.md
- ADR-006-harden-ai-gateway-with-resilience-and-observability.md
- ADR-008-adopt-opentelemetry-first-observability-with-serilog-request-logging.md

## Context

Day 5 introduced the `IChatModelProvider` abstraction with provider-agnostic
`ChatRequest` and `ChatResponse` contracts. Day 6 hardened the gateway with
correlation IDs, structured error contracts, resilience policies, and
LLM-specific telemetry on the `claude.chat.api` Activity span. The gateway is
now production-shaped but uncached — every request to Anthropic re-bills the
full input token cost of the system prompt, regardless of how many times that
prompt has already been sent.

Anthropic supports ephemeral prompt caching via `cache_control: {"type":"ephemeral"}`
content block annotation on cacheable portions of the request payload. When a
cache hit occurs, the cached input tokens are billed at approximately 10% of
the standard input token rate. For a typical 3,000-token system prompt repeated
10,000 times per day at $3 per million input tokens, this is the difference
between $90/day and roughly $9/day in input token cost for the cached portion.

The architectural question Day 7 must answer is not whether to implement
caching — the cost case is unambiguous. The question is *where in the codebase
caching belongs*, and what that placement implies for the future multi-provider
roadmap (Azure OpenAI, Bedrock, Foundry).

Two placements are credible:

1. **Inside the provider boundary** — `cache_control` annotation lives in
   `ClaudeApiClient` payload construction. `IChatModelProvider`, `ChatRequest`,
   and `ChatResponse` stay unchanged. Caching is an Anthropic implementation
   detail, opaque to callers.

2. **In a decorator above the provider boundary** — A `CachingChatModelProvider`
   decorator wraps any `IChatModelProvider`, decides what's cacheable, and
   delegates the annotation to the wrapped provider via a new abstraction
   (e.g., `ICacheAnnotator`). Caching strategy is provider-agnostic; only the
   provider-specific annotation format lives in each provider.

Both placements are defensible. The decision turns on the YAGNI question:
how much abstraction is justified by *one* example?

## Decision

We will implement prompt caching inside the provider boundary on Day 7.

Specifically:
- `ClaudeApiClient` constructs the system prompt as a content array with a
  `cache_control: {"type":"ephemeral"}` annotation on the system prompt block.
- `AnthropicOptions` gains an `EnablePromptCaching` boolean (default `true`)
  to allow operational toggling without redeploys.
- `ClaudeApiClient` extracts `cache_read_input_tokens` and
  `cache_creation_input_tokens` from the Anthropic response and surfaces them
  as Activity tags (`llm.cache.read_tokens`, `llm.cache.creation_tokens`) on
  the existing `claude.chat.api` span.
- `GatewayTelemetry` gains two new counters: `ai.provider.cache.hits` and
  `ai.provider.cache.misses`, incremented based on the response usage data.
- `IChatModelProvider`, `ChatRequest`, and `ChatResponse` are not modified.
  Cache status does not appear in the API surface; it is a transport-layer
  optimization observable only via telemetry.

When a second provider with caching semantics is added (Azure OpenAI's
`prompt_cache_key`, Bedrock's prompt caching, etc.), we will revisit this
decision and extract a `CachingChatModelProvider` decorator with an
`ICacheAnnotator` per-provider seam. That extraction is documented here as
the named forward-compatibility path; it is explicitly not part of Day 7
scope.

## Alternatives Considered

### Alternative 1 — Implement caching inside `ClaudeChatModelProvider` / `ClaudeApiClient`

This is the chosen alternative. See Decision above.

**Why chosen:**
- Anthropic's `cache_control` is a payload annotation specific to Anthropic's
  API shape. Other providers (Azure OpenAI, Bedrock, Foundry) use different
  mechanisms — cache keys, automatic caching by content hash, model-specific
  flags. There is no shared "caching protocol" across providers to abstract.
- The provider boundary is already the place where Anthropic-specific request
  construction lives. Adding `cache_control` there is consistent with the
  current architecture.
- YAGNI: introducing a `CachingChatModelProvider` decorator and an
  `ICacheAnnotator` interface for a single provider creates abstraction
  weight without abstraction value. The second provider — whose caching
  semantics will inform the right shape of `ICacheAnnotator` — does not yet
  exist. Designing the abstraction now risks designing it against assumptions
  that turn out to be wrong.

**Consequences accepted:**
- When a second provider lands, we will rewrite caching logic in two places
  before extracting the decorator. This is a known cost.
- The caching strategy ("which content is cacheable, for how long, under what
  conditions") is colocated with the Anthropic transport. That is acceptable
  while one provider exists; it becomes a code-smell when two exist.

### Alternative 2 — Introduce `CachingChatModelProvider` decorator now

A decorator class implementing `IChatModelProvider` would wrap any concrete
provider (initially `ClaudeChatModelProvider`). The decorator decides what to
cache; the wrapped provider performs the actual annotation via a new method
on `IChatModelProvider` or a sibling `ICacheAnnotator` interface.

**Why rejected:**
- The decorator can only decide *what* to cache (e.g., "the system prompt is
  cacheable"). It cannot decide *how* to annotate without provider-specific
  knowledge — `cache_control` blocks are not portable to Azure OpenAI's API
  shape. The decorator's actual logic reduces to "tell the wrapped provider
  to please cache the system prompt," which is information the wrapped
  provider already has via configuration.
- Adding `ICacheAnnotator` (or modifying `IChatModelProvider` to include a
  caching hint method) introduces an interface with one implementation and
  one consumer. By definition, that interface's shape is fitted to its
  single implementation. When the second provider arrives, the interface is
  almost certain to need reshaping.
- The unit-testability argument for the decorator is real but small. The
  caching decision in scope for Day 7 is "always cache the system prompt
  when `EnablePromptCaching=true`." That is a configuration check, not a
  policy worth isolating behind a mock.

**Revisit conditions:** Add a second provider with caching semantics. At
that point, the shape of `ICacheAnnotator` can be designed against two
real examples instead of one assumed one.

### Alternative 3 — Add `CacheHint` field to `ChatRequest`

`ChatRequest` could grow a field like `CacheHint: { CacheSystemPrompt: true }`
that providers may or may not honor. Callers express intent; providers
translate it to provider-specific annotations.

**Why rejected:**
- This pollutes the provider-agnostic contract with provider-operational
  concerns. `ChatRequest` is the public-facing API shape, intended to be
  stable across providers and across time. Cache hints are an internal
  optimization concern, not an application concern.
- The hint is not actionable by callers — there is no scenario in which the
  caller of the gateway has better information about cacheability than the
  gateway itself does. The hint is therefore noise on the public contract.
- The same observability outcome (cache hit rate, savings telemetry) is
  achievable via Activity tags without the contract change.

### Alternative 4 — Defer caching entirely; design the abstraction first

Wait until a second provider is added before implementing caching at all,
on the grounds that the cleanest design will only be visible with two
examples to learn from.

**Why rejected:**
- The cost case for caching is immediate and quantifiable. Deferring caching
  in order to design a better abstraction in some future quarter trades
  measurable savings against speculative architectural elegance. That is
  the wrong trade for a cost-control feature.
- The Day 7 implementation does not foreclose the future abstraction. The
  forward-compatibility path is named and the migration cost is bounded
  (one provider's worth of caching code to refactor).

## Consequences

### Positive
- Substantial input token cost reduction (~90% on the cached portion) for
  any workload with a stable system prompt.
- Cache economics are observable as first-class telemetry: cache hit rate
  becomes a queryable metric, not a billing-statement surprise.
- Day 7 ships a working cost control without introducing a premature
  abstraction.
- The decision is reversible: the forward-compatibility path is named and
  the refactoring boundary is small.

### Negative
- Anthropic-specific caching logic lives inside the provider rather than at
  a higher abstraction layer. When a second cacheable provider lands, the
  code shape will need refactoring (decorator extraction).
- Caching strategy ("what to mark cacheable") is colocated with transport
  concerns. This is acceptable for one Anthropic-specific cache target
  (the system prompt) but would become unwieldy if caching policy grows
  more nuanced (multi-block caching, conditional caching by user tier,
  etc.).
- The `EnablePromptCaching` flag is a per-provider config, not a gateway-wide
  policy. If the gateway grows providers, each will need its own toggle.

### Neutral / Tradeoffs
- Cache observability uses the same Activity tag pattern as the existing
  `llm.tokens.*` tags (Day 6). This is consistent but means cache telemetry
  is queried via the dependencies table, not a dedicated cache table.
- The `cache_read_input_tokens` and `cache_creation_input_tokens` fields are
  Anthropic-specific names. Generic tag names (`llm.cache.read_tokens`,
  `llm.cache.creation_tokens`) abstract over them, so future providers can
  emit the same tag names with provider-appropriate semantics.

## Implementation Notes

### Files affected
- `src/lab-observability-api/Options/AnthropicOptions.cs`
  - Add `bool EnablePromptCaching { get; init; } = true;`
- `src/lab-observability-api/Services/Claude/ClaudeApiClient.cs`
  - Modify payload construction to emit the system prompt as a content array
    with `cache_control: {"type":"ephemeral"}` when `EnablePromptCaching` is
    true.
  - Extract `cache_read_input_tokens` and `cache_creation_input_tokens` from
    the response `usage` block.
  - Set `llm.cache.read_tokens` and `llm.cache.creation_tokens` Activity tags
    on the existing `claude.chat.api` span.
  - Increment `GatewayTelemetry.CacheHits` when read tokens > 0; increment
    `GatewayTelemetry.CacheMisses` when creation tokens > 0.
- `src/lab-observability-api/Telemetry/GatewayTelemetry.cs`
  - Add `Counter<long> CacheHits` and `Counter<long> CacheMisses` instruments.
- `Infra/Day-007/appsettings-template.md`
  - Document `Anthropic__EnablePromptCaching` as a new app setting.

### Files NOT affected
- `src/lab-observability-api/Models/ChatRequest.cs` — provider-agnostic
  contract is unchanged.
- `src/lab-observability-api/Models/ChatResponse.cs` — provider-agnostic
  contract is unchanged.
- `src/lab-observability-api/Providers/IChatModelProvider.cs` — abstraction
  seam is unchanged.
- `src/lab-observability-api/Providers/ClaudeChatModelProvider.cs` —
  orchestration layer does not need to know caching is happening.

### Operational requirements
- The `Anthropic__EnablePromptCaching` app setting must exist on the deployed
  App Service. Default `true` if absent (caching is the desired production
  state; the toggle exists for debugging).
- Anthropic's prompt caching has a minimum cacheable size (currently 1024
  tokens for most models). System prompts shorter than this will not produce
  cache hits regardless of annotation. Day 7 verification must use a system
  prompt of at least 1100 tokens to confirm the mechanism is working.

### Rollback strategy
- Set `Anthropic__EnablePromptCaching=false` on the App Service. The
  `ClaudeApiClient` will emit the legacy payload shape (system prompt as a
  string, no `cache_control` block). No code redeploy required.

### Migration path (when forward-compatibility triggers)
When a second provider with caching semantics is added:
1. Extract an `ICacheAnnotator` interface with one method (shape TBD by the
   second provider's needs, not pre-designed here).
2. Implement `ICacheAnnotator` on `ClaudeChatModelProvider` and on the new
   provider.
3. Introduce `CachingChatModelProvider` as a DI-registered decorator that
   wraps the keyed `IChatModelProvider` instances.
4. Move the cache strategy decision ("system prompt is cacheable") from
   `ClaudeApiClient` configuration to the decorator.
5. Telemetry tags do not change — they are already provider-agnostic.

## References
- Anthropic prompt caching documentation:
  https://docs.anthropic.com/en/docs/build-with-claude/prompt-caching
- ADR-005 (provider abstraction this layer wraps)
- ADR-006 (observability foundation this extends)
- `docs/architecture/observability-architecture.md` — telemetry pillars
- `docs/standards/kql-cookbook.md` — will gain Query 8 and Query 9 (cache hit
  rate, savings) on Day 7