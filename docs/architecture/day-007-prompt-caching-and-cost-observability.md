# Day 7 — Prompt Caching & Cost Observability

## Change Summary

Day 7 adds Anthropic prompt caching to the AI gateway's provider implementation,
keeping the `IChatModelProvider` seam and all provider-agnostic contracts
unchanged. Cache behavior is surfaced as first-class telemetry on the existing
`claude.chat.api` Activity span.

See `docs/adr/ADR-009-implement-prompt-caching-inside-provider-boundary.md` for
the full decision rationale, alternatives considered, and forward-compatibility
migration path.

## Architecture Delta (Day 6 → Day 7)

Day 6 established the full observability and resilience stack. Day 7 extends
the `ClaudeApiClient` layer only — no new middleware, no new endpoints, no
changes above the provider boundary.

```
Before (Day 6):
  ChatController
    └── ClaudeChatModelProvider
          └── ClaudeApiClient
                └── POST /messages
                    body: { model, messages, max_tokens, system: "<string>" }

After (Day 7, EnablePromptCaching=true):
  ChatController
    └── ClaudeChatModelProvider          (unchanged)
          └── ClaudeApiClient
                └── POST /messages
                    body: {
                      model, messages, max_tokens,
                      system: [
                        {
                          type: "text",
                          text: "<system prompt>",
                          cache_control: { type: "ephemeral", ttl: "1h" }
                        }
                      ]
                    }
                    response.usage: {
                      input_tokens,
                      output_tokens,
                      cache_read_input_tokens,    ← new
                      cache_creation_input_tokens ← new
                    }

After (Day 7, EnablePromptCaching=false):
  Body reverts to system: "<string>" — same as Day 6.
  Toggle is an App Service env var; no redeploy required.
```

## Telemetry Delta

### New Activity tags on `claude.chat.api` span

| Tag | Type | When set |
|-----|------|----------|
| `llm.cache.read_tokens` | int | `cache_read_input_tokens > 0` (cache hit) |
| `llm.cache.creation_tokens` | int | `cache_creation_input_tokens > 0` (cache miss/creation) |

### New Metrics

| Metric | Type | Description |
|--------|------|-------------|
| `ai.provider.cache.hits` | Counter\<long\> | Requests that read from the prompt cache |
| `ai.provider.cache.misses` | Counter\<long\> | Requests that populated the prompt cache |

Existing metrics and tags (`llm.tokens.input`, `llm.tokens.output`,
`llm.latency_ms`, `ai.provider.requests`, `ai.provider.failures`,
`ai.provider.latency.ms`) are unchanged.

## Sequence Flow — Cache Miss (first request after cold start or TTL expiry)

```
Client          ChatController      ClaudeChatModelProvider    ClaudeApiClient     Anthropic API
  │                  │                        │                      │                    │
  │ POST /chat       │                        │                      │                    │
  │─────────────────>│                        │                      │                    │
  │                  │ CompleteAsync()         │                      │                    │
  │                  │───────────────────────>│                      │                    │
  │                  │                        │ SendChatAsync()       │                    │
  │                  │                        │─────────────────────>│                    │
  │                  │                        │                      │ POST /messages      │
  │                  │                        │                      │ [system: array      │
  │                  │                        │                      │  with cache_control]│
  │                  │                        │                      │───────────────────>│
  │                  │                        │                      │                    │
  │                  │                        │                      │     usage:          │
  │                  │                        │                      │  cache_creation=N   │
  │                  │                        │                      │<───────────────────│
  │                  │                        │                      │                    │
  │                  │                        │                      │ tag: llm.cache.creation_tokens=N
  │                  │                        │                      │ counter: CacheMisses++
  │                  │ ChatResponse            │                      │                    │
  │<─────────────────────────────────────────────────────────────────│                    │
```

## Sequence Flow — Cache Hit (subsequent request within TTL)

```
Client          ChatController      ClaudeChatModelProvider    ClaudeApiClient     Anthropic API
  │                  │                        │                      │                    │
  │ POST /chat       │                        │                      │                    │
  │─────────────────>│                        │                      │                    │
  │                  │ CompleteAsync()         │                      │                    │
  │                  │───────────────────────>│                      │                    │
  │                  │                        │ SendChatAsync()       │                    │
  │                  │                        │─────────────────────>│                    │
  │                  │                        │                      │ POST /messages      │
  │                  │                        │                      │ [same cache_control │
  │                  │                        │                      │  annotation]        │
  │                  │                        │                      │───────────────────>│
  │                  │                        │                      │                    │
  │                  │                        │                      │     usage:          │
  │                  │                        │                      │  cache_read=N       │
  │                  │                        │                      │  (billed at ~10%)   │
  │                  │                        │                      │<───────────────────│
  │                  │                        │                      │                    │
  │                  │                        │                      │ tag: llm.cache.read_tokens=N
  │                  │                        │                      │ counter: CacheHits++
  │                  │ ChatResponse            │                      │                    │
  │<─────────────────────────────────────────────────────────────────│                    │
```

## Configuration

### New App Service settings (Day 7)

| Setting | Default | Description |
|---------|---------|-------------|
| `Anthropic__EnablePromptCaching` | `true` | Enables `cache_control` annotation on system prompt |
| `Anthropic__SystemPrompt` | `""` | The system prompt to cache. Must be ≥1024 tokens for Anthropic to honor the hint. |

### Rollback

Set `Anthropic__EnablePromptCaching=false` on the App Service. No code redeploy
required. The `ClaudeApiClient` falls back to `"system": "<string>"` payload shape
(Day 6 behavior).

## Files Changed

| File | Change |
|------|--------|
| `Options/AnthropicOptions.cs` | Added `EnablePromptCaching`, `SystemPrompt` |
| `Services/Claude/ClaudeApiClient.cs` | Cache annotation in `BuildAnthropicRequest`; cache token extraction in `TryExtractUsage`; telemetry tags and counter increments |
| `Telemetry/GatewayTelemetry.cs` | Added `CacheHits`, `CacheMisses` counters |

## Files NOT Changed

| File | Reason |
|------|--------|
| `Providers/IChatModelProvider.cs` | Provider-agnostic seam — must not change |
| `Models/ChatRequest.cs` | Public contract — cache is not a caller concern |
| `Models/ChatResponse.cs` | Public contract — cache status is telemetry only |
| `Providers/ClaudeChatModelProvider.cs` | Orchestration layer — does not need to know caching is happening |

## Related

- `docs/adr/ADR-009-implement-prompt-caching-inside-provider-boundary.md`
- `docs/architecture/observability-architecture.md` — telemetry pillars (Day 6)
- `docs/standards/kql-cookbook.md` — Query 8 (cache hit rate), Query 9 (savings)
- `Infra/Day-007/appsettings-template.md`
