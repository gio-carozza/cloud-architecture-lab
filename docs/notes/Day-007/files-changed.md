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
| `CLAUDE.md` | docs pass + deploy | Gotchas: TTL, model ID, nested usage format, appsettings PATCH workaround; files-changed.md convention; north star items annotated with ADR numbers and done status |
| `Infra/Day-007/appsettings-template.md` | docs pass | `cache_control` description updated to include TTL; model row corrected from `claude-opus-4-6` to `claude-sonnet-4-6` |
| `docs/notes/Day-007/summary.md` | docs pass → collab-lens | `BuildAnthropicRequest` description updated; collab-lens block inserted under "Whose Problem Am I Solving?" — primary: Cloud & Model-Vendor Support |
| `.claude/commands/new-day.md` | docs pass | Added `files-changed.md` and `appsettings-template.md` to scaffold list; added required-content templates for both |
| `.claude/instructions/daily-workflow.md` | docs pass | STEP 7 manual settings block replaced with automatic appsettings-template.md note; STEP 8 item 6 added for files-changed.md upsert; STEP 1 description updated |
| `.claude/commands/deploy.md` | docs pass | Step 1b added — reads appsettings-template.md and applies settings before publish; DO NOT rule added for settings-apply failure |
| `docs/notes/Day-007/completion-checklist.md` | audit + deploy | cache_control format corrected; three stale [ ] items marked [x]; all deploy items marked [x] after Azure verification |
| `docs/notes/Day-007/files-changed.md` | audit + deploy → collab-lens | Merged duplicate ClaudeApiClient.cs rows; .claude command changes added; deploy pass rows upserted; collab-lens row upserted |
| `docs/standards/_principles.md` | audit + closeout | Three Day 7 graveyard entries; 4th posture question added; "three questions" → "four questions" |
| `docs/architecture/day-007-prompt-caching-and-cost-observability.md` | audit | cache_control format in ASCII diagram updated to include ttl:"1h" |
| `docs/standards/kql-cookbook.md` | docs pass | Queries 8 (cache hit rate) and 9 (estimated token savings) added; pricing updated to claude-sonnet-4-6 rates |
| `docs/standards/azure-environment.md` | docs pass + standards | Identity table added; stale "pending portal apply" removed; both App Insights connection string keys listed; naming-conventions Action Groups section updated |
| `docs/standards/naming-conventions.md` | standards | Action Groups section updated with full pattern and two real examples |
| `docs/certifications/ai-102/study-notes/day-007-mapping.md` | cert-update | New file — AI-102 Domain 1 and 6 mapping, 5 exam questions, two-level concept explanations |
| `docs/notes/_index.md` | closeout | Day 007 status updated from "In Progress" to "Complete" |
| `docs/certifications/ai-102/domains/001-plan-manage/concepts.md` | cert-update | Day 7 additions: prompt caching for cost management + cache hit rate as operational metric (all four levels) |
| `docs/certifications/ai-102/domains/001-plan-manage/practice-q.md` | cert-update | Q11–Q15: prompt caching threshold, placement in provider abstraction, cache hit rate KQL, silent failure diagnosis, savings estimation |
| `docs/certifications/ai-102/domains/001-plan-manage/day-mapping.md` | cert-update | Day-007 row added |
| `docs/certifications/az-305/domains/004-infrastructure/concepts.md` | cert-update | Day 7 additions: caching solution recommendation tiers + YAGNI/abstraction deferral (all four levels) |
| `docs/certifications/az-305/domains/004-infrastructure/practice-q.md` | cert-update | Q16–Q20: caching solution recommendation, YAGNI principle, cache tier selection, operational toggle pattern, KQL savings query |
| `docs/certifications/az-305/domains/004-infrastructure/day-mapping.md` | cert-update | Day-007 row added |
| `docs/certifications/domain-coverage.md` | cert-update | AZ-305 Domain 4 updated to Day 6, 7, 8, 9; header updated to Day-007 cert-update |
