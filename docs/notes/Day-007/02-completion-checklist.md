# Day 7 — Completion Checklist

## Code

- [x] `Options/AnthropicOptions.cs` — `EnablePromptCaching { get; init; } = true` added
- [x] `Options/AnthropicOptions.cs` — `SystemPrompt { get; init; } = ""` added
- [x] `Services/Claude/ClaudeApiClient.cs` — `BuildAnthropicRequest` emits system prompt as content array with `cache_control: {"type":"ephemeral","ttl":"1h"}` when `EnablePromptCaching=true`
- [x] `Services/Claude/ClaudeApiClient.cs` — `BuildAnthropicRequest` falls back to `node["system"] = _options.SystemPrompt` (string) when `EnablePromptCaching=false`
- [x] `Services/Claude/ClaudeApiClient.cs` — `TryExtractUsage` returns 4-tuple including `cacheReadTokens` and `cacheCreationTokens`
- [x] `Services/Claude/ClaudeApiClient.cs` — `llm.cache.read_tokens` Activity tag set on `claude.chat.api` span when `cacheReadTokens > 0`
- [x] `Services/Claude/ClaudeApiClient.cs` — `llm.cache.creation_tokens` Activity tag set when `cacheCreationTokens > 0`
- [x] `Services/Claude/ClaudeApiClient.cs` — `GatewayTelemetry.CacheHits.Add(1)` called when `cacheReadTokens > 0`
- [x] `Services/Claude/ClaudeApiClient.cs` — `GatewayTelemetry.CacheMisses.Add(1)` called when `cacheCreationTokens > 0`
- [x] `Telemetry/GatewayTelemetry.cs` — `CacheHits` counter (`ai.provider.cache.hits`) added
- [x] `Telemetry/GatewayTelemetry.cs` — `CacheMisses` counter (`ai.provider.cache.misses`) added
- [x] `IChatModelProvider.cs` — NOT modified (provider-agnostic seam unchanged per ADR-009)
- [x] `Models/ChatRequest.cs` — NOT modified
- [x] `Models/ChatResponse.cs` — NOT modified
- [x] `Providers/ClaudeChatModelProvider.cs` — NOT modified

## Build & Local Verification

- [x] `dotnet build` succeeds — 0 errors, 0 warnings
- [x] System prompt of ≥1100 tokens configured (user-secrets — 6920 chars ≈ 1490 tokens)
- [x] `dotnet run` starts without errors
- [x] First `POST /api/ai/chat`: console logs show `cache_creation_input_tokens > 0` (CacheCreationTokens=1488)
- [x] Second identical request: console logs show `cache_read_input_tokens > 0` (CacheReadTokens=1488)
- [x] `EnablePromptCaching=false` path verified: payload falls back to `"system": "<string>"`

> **Fixes required during verification:**
> 1. `claude-opus-4-6` (in user-secrets) does not activate caching — switched to `claude-sonnet-4-6`.
> 2. `{"type":"ephemeral"}` without TTL produces 0 cache tokens on Claude 4 models — added `"ttl":"1h"`.
> 3. `TryExtractUsage` updated to also sum `cache_creation.ephemeral_*_input_tokens` (new Anthropic API response format).

> **Files changed during verification:**
>
> | File | Change |
> |---|---|
> | `02-completion-checklist.md` | All 6 local verification items marked `[x]`; bug fixes footnoted |
> | `04-posture-check.md` | All 4 posture questions answered — graveyard entry for `claude-opus-4-6` included |
> | `03-architect-thinking.md` | New section 8 documents all three Claude 4 API format discoveries with the "silent failure" failure mode explained |
> | `CLAUDE.md` | Three new Gotchas bullets: TTL requirement, wrong model ID = silent 0, new nested usage format |
> | `Infra/Day-007/appsettings-template.md` | `cache_control` description updated to include TTL; model row updated from `claude-opus-4-6` to `claude-sonnet-4-6` |
> | `docs/notes/Day-007/01-summary.md` | `BuildAnthropicRequest` description updated to reflect `ttl:"1h"` and the nested fallback |
> | `src/.../ClaudeApiClient.cs` | `cache_control` TTL added; `TryExtractUsage` extended with nested format fallback |

## Infra & Config

- [x] `Infra/Day-007/appsettings-template.md` documents `Anthropic__EnablePromptCaching` and `Anthropic__SystemPrompt`
- [x] `Anthropic__EnablePromptCaching=true` set on App Service — applied via ARM PATCH (PATCH path works; PUT blocked by network proxy)
- [x] `Anthropic__SystemPrompt` set on App Service (6920 chars / ≈1490 tokens) — applied same pass

## KQL

- [x] Query 8 — cache hit rate added to `docs/standards/kql-cookbook.md`
- [x] Query 9 — estimated savings added to `docs/standards/kql-cookbook.md`

## Deploy & Azure Verification

- [x] Deploy via `/deploy` slash command — Kudu zip deploy `d5e4982` to `app-ai-lab-api-dev-eastus-gio`
- [x] `GET /health` returns 200 from Azure — `{"status":"healthy"}`
- [x] `POST /api/ai/chat` returns 200 from Azure — model: `claude-sonnet-4-6`, `X-Correlation-Id` present
- [x] App Insights `dependencies` table: `claude.chat.api` span shows `llm.cache.creation_tokens=1488` on first post-deploy request
- [x] App Insights `dependencies` table: `llm.cache.read_tokens=1488` visible on second request
- [x] KQL Query 8 returns a non-null cache hit rate after ≥2 live requests — **50% hit rate (1/2 requests)**

## Documentation

- [x] `docs/adr/ADR-009-implement-prompt-caching-inside-provider-boundary.md` — Accepted (pre-written)
- [x] `docs/architecture/day-007-prompt-caching-and-cost-observability.md` written
- [x] `docs/notes/Day-007/03-architect-thinking.md` written
- [x] `docs/notes/Day-007/04-posture-check.md` filled (end of day, before commit)
- [x] Git commit: `feat(day-007): prompt caching and cost observability` — `d5e4982`
