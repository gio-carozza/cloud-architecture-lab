# Day 007 — Files Changed

| File | Step | Change |
|---|---|---|
| `src/lab-observability-api/Options/AnthropicOptions.cs` | build | Added `EnablePromptCaching` (bool, default `true`) and `SystemPrompt` (string) |
| `src/lab-observability-api/Services/Claude/ClaudeApiClient.cs` | build + verification | `BuildAnthropicRequest` emits system as content array with `cache_control: {"type":"ephemeral","ttl":"1h"}`; `TryExtractUsage` returns 4-tuple with nested `cache_creation.ephemeral_*` fallback for Claude 4 API format; cache Activity tags and counters wired |
| `src/lab-observability-api/Telemetry/GatewayTelemetry.cs` | build | Added `CacheHits` (`ai.provider.cache.hits`) and `CacheMisses` (`ai.provider.cache.misses`) counters |
| `docs/adr/ADR-009-implement-prompt-caching-inside-provider-boundary.md` | build | New ADR — Accepted; documents placement decision and forward-compatibility path |
| `docs/notes/Day-007/completion-checklist.md` | verification | All 6 local verification items marked `[x]`; bug fixes and files-changed section added |
| `docs/notes/Day-007/posture-check.md` | docs pass | All 4 posture questions answered; graveyard entry for `claude-opus-4-6` included |
| `docs/notes/Day-007/architect-thinking.md` | docs pass | New section 8 — Claude 4 API format discoveries: TTL requirement, nested response format, model ID silent failure |
| `CLAUDE.md` | docs pass | Three new Gotchas bullets (TTL, model ID, nested usage format); `files-changed.md` convention added to Conventions |
| `Infra/Day-007/appsettings-template.md` | docs pass | `cache_control` description updated to include TTL; model row corrected from `claude-opus-4-6` to `claude-sonnet-4-6` |
| `docs/notes/Day-007/summary.md` | docs pass | `BuildAnthropicRequest` description updated to reflect `ttl:"1h"` and nested format fallback |
| `.claude/commands/new-day.md` | docs pass | Added `files-changed.md` and `appsettings-template.md` to scaffold list; added required-content templates for both |
| `.claude/instructions/daily-workflow.md` | docs pass | STEP 7 manual settings block replaced with automatic appsettings-template.md note; STEP 8 item 6 added for files-changed.md upsert; STEP 1 description updated |
| `.claude/commands/deploy.md` | docs pass | Step 1b added — reads appsettings-template.md and applies settings before publish; DO NOT rule added for settings-apply failure |
| `docs/notes/Day-007/completion-checklist.md` | audit | cache_control format corrected to include ttl:"1h"; appsettings-template, architect-thinking, posture-check items marked [x] |
| `docs/notes/Day-007/files-changed.md` | audit | Merged duplicate ClaudeApiClient.cs rows (dedup rule); .claude command changes added |
| `docs/standards/_principles.md` | audit | Two Day 7 graveyard entries added: claude-opus-4-6 model ID failure, cache_control TTL silent regression |
| `docs/architecture/day-007-prompt-caching-and-cost-observability.md` | audit | cache_control format in ASCII diagram updated to include ttl:"1h" |
| `docs/standards/kql-cookbook.md` | docs pass | Queries 8 (cache hit rate) and 9 (estimated token savings) added; pricing updated to claude-sonnet-4-6 rates |
| `docs/standards/azure-environment.md` | docs pass | Anthropic__EnablePromptCaching and Anthropic__SystemPrompt added (pending portal apply); Anthropic__Model updated to claude-sonnet-4-6 |
| `CLAUDE.md` | docs pass | North star items 1-3 annotated with ADR numbers and done status; SSL gotcha updated with appsettings PUT portal fallback |
