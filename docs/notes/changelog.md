# Files Changed — Running Changelog

Consolidated from the per-day `docs/notes/Day-NNN/07-files-changed.md` files
(2026-06-16 — see the Day 009 section's `tooling` row for why). One section per
day, in day order. **Dedup key is the file path within each day's section** — if
a file already has a row under the same `## Day NNN` heading, update it in place;
never add a duplicate row for that day. A file legitimately appearing under
multiple different `## Day NNN` headings is not a duplicate — that's history (the
file was touched on more than one day).

When logging a fix made *during* Day N's session to a file that was originally
created on an earlier, already-closed day (e.g. a drift fix to `Day-006/01-summary.md`
found while auditing on Day 009), log the row under **Day N's** section — the day
the edit actually happened — not under the older day that originally created the
file. Git already has the authoritative timestamp; don't spend effort re-deriving
"which day owns this file" the way the per-day-file convention forced.

---

## Day 001

> Day-001 predates the file-change-log convention (established Day 6). This
> section is a retroactive stub for completeness.

| File | Step | Change |
|---|---|---|
| `docs/notes/Day-001/01-summary.md` | scaffold | Created — day summary (Azure fundamentals, subscription setup) |
| `docs/notes/Day-001/02-learning-goals.md` | scaffold | Created — learning goals for Day 1 |
| `docs/notes/Day-001/03-azure-core-concepts.md` | scaffold | Created — core concepts notes |

## Day 002

> Day-002 predates the file-change-log convention (established Day 6). This
> section is a retroactive stub for completeness.

| File | Step | Change |
|---|---|---|
| `docs/notes/Day-002/01-summary.md` | scaffold | Created — day summary (CAF, landing zone concepts) |
| `docs/notes/Day-002/02-caf-notes.md` | scaffold | Created — Cloud Adoption Framework notes |
| `docs/notes/Day-002/03-well-architected-notes.md` | scaffold | Created — Well-Architected Framework notes |
| `docs/architecture/day-002-landing-zone-concept.md` | docs pass | Created — landing zone architecture diagram |

## Day 003

> Day-003 predates the file-change-log convention (established Day 6). This
> section is a retroactive stub for completeness.

| File | Step | Change |
|---|---|---|
| `docs/notes/Day-003/01-summary.md` | scaffold | Created — day summary (Well-Architected applied to lab) |
| `docs/notes/Day-003/02-well-architected-applied.md` | scaffold | Created — WAF pillar analysis |
| `docs/notes/Day-003/03-qa.md` | scaffold | Created — Q&A and review notes |
| `docs/adr/ADR-003-adopt-well-architected-framework.md` | docs pass | Created — ADR for WAF adoption |

## Day 004

> Day-004 predates the file-change-log convention (established Day 6). This
> section is a retroactive stub for completeness.

| File | Step | Change |
|---|---|---|
| `docs/notes/Day-004/01-summary.md` | scaffold | Created — day summary (first workload on App Service) |
| `docs/notes/Day-004/02-well-architected-applied.md` | scaffold | Created — WAF pillar analysis for App Service workload |
| `docs/adr/ADR-004-first-workload-on-app-service-with-app-insights.md` | docs pass | Created — ADR for App Service + App Insights adoption |
| `docs/adr/ADR-004-first-workload-on-app-service-with-app-insights.md` | drift-fix | Heading casing corrected: `## Alternatives considered` → `## Alternatives Considered` to match repo-wide ADR heading convention |
| `src/lab-observability-api/lab-observability-api.csproj` | retro-fill | Project scaffold — confirmed via `git log` (commit `29248be`, "LLM Roadmap Day-4"); first commit predates this changelog's existence so it was never logged at the time |
| `src/lab-observability-api/Controllers/TestController.cs` | retro-fill | Created — `GET /api/test/ping` and `GET /api/test/error` smoke-test endpoints for verifying App Service deploy + App Insights exception capture, per `docs/notes/Day-004/02-well-architected-applied.md`; same `git log` provenance as above |

## Day 005

| File | Step | Change |
|---|---|---|
| `src/lab-observability-api/Options/AnthropicOptions.cs` | build | Created — options binding for Anthropic config section |
| `src/lab-observability-api/Models/AI/ChatRequest.cs` | build | Created — provider-agnostic request contract |
| `src/lab-observability-api/Models/AI/ChatResponse.cs` | build | Created — provider-agnostic response contract |
| `src/lab-observability-api/Services/AI/IChatModelProvider.cs` | build | Created — provider seam interface (ADR-005) |
| `src/lab-observability-api/Services/AI/ClaudeChatModelProvider.cs` | build | Created — Anthropic implementation of IChatModelProvider |
| `src/lab-observability-api/Controllers/AiController.cs` | build | Created — `POST /api/ai/chat` endpoint |
| `src/lab-observability-api/Program.cs` | build | Updated — DI registration for options and provider |
| `docs/adr/ADR-005-introduce-provider-abstraction-for-claude-integration.md` | docs pass | Created — ADR for provider abstraction decision |
| `docs/architecture/day-005-ai-gateway-v1.md` | docs pass | Created — system diagram v1 |
| `docs/architecture/day-005-sequence-flow.md` | docs pass | Created — sequence flow for chat endpoint |
| `Infra/Day-005/appsettings-template.md` | docs pass | Created — Anthropic app settings template |
| `docs/notes/Day-005/01-summary.md` | docs pass | Created — day summary |
| `docs/notes/Day-005/02-completion-checklist.md` | docs pass | Created — completion checklist (all items checked) |

## Day 006

| File | Step | Change |
|---|---|---|
| `docs/notes/Day-006/01-summary.md` | collab-lens | Collaboration Lens block inserted under "Whose Problem Am I Solving?" — primary: DevOps / SRE |
| `src/lab-observability-api/appsettings.json` | sync-audit | `claude-opus-4-7` → `claude-sonnet-4-6`; stale model ID found during full-repo sync audit |
| `src/lab-observability-api/appsettings.Development.json` | sync-audit | `claude-opus-4-7` → `claude-sonnet-4-6`; stale model ID found during full-repo sync audit |
| `docs/notes/Day-006/06-deployment-log.md` | deployment-log | Created — retroactive deployment record: infra pre-work, build/publish/zip/Kudu, 5 post-deploy tests (health, chat, bad-key, App Insights requests, App Insights token spans), issues & fixes |
| `docs/notes/Day-006/02-completion-checklist.md` | docs pass | Cert items closed — AI-102 retired, AZ-104 restructured to Day 035; inline superseded notes added |
| `docs/notes/Day-006/01-summary.md` | drift-fix | Certification Reinforcement AZ-104 line: stale "~Day 10-15" timeline corrected to restructured "~Day 035" |
| `docs/adr/ADR-006-harden-ai-gateway-with-resilience-and-observability.md` | drift-fix | Heading casing corrected: `## Alternatives considered` → `## Alternatives Considered` to match repo-wide ADR heading convention |
| `src/lab-observability-api/Contracts/ApiError.cs` | retro-fill | Created — 3-field error contract (`Code`, `Message`, `CorrelationId`); confirmed via `git log` (commit `b8728a7`, "Day 6 doc estate cleanup"), this changelog convention didn't exist yet when it was actually built |
| `src/lab-observability-api/Extensions/HttpContextExtensions.cs` | retro-fill | Created — `GetCorrelationId()` extension method used by every controller and the global exception pipeline; same `git log` provenance as above |
| `src/lab-observability-api/Middleware/CorrelationIdMiddleware.cs` | retro-fill | Created — generates/propagates `X-Correlation-Id`; same `git log` provenance as above |
| `src/lab-observability-api/Services/Claude/ClaudeProviderException.cs` | retro-fill | Created — the single exception type carrying `Provider`/`ProviderStatusCode`/`ProviderErrorCode`/`IsTransient`, thrown by `ClaudeApiClient` and caught by the global exception pipeline; same `git log` provenance as above |

## Day 007

| File | Step | Change |
|---|---|---|
| `src/lab-observability-api/Options/AnthropicOptions.cs` | build | Added `EnablePromptCaching` (bool, default `true`) and `SystemPrompt` (string) |
| `src/lab-observability-api/Services/Claude/ClaudeApiClient.cs` | build + verification | `BuildAnthropicRequest` emits system as content array with `cache_control: {"type":"ephemeral","ttl":"1h"}`; `TryExtractUsage` returns 4-tuple with nested `cache_creation.ephemeral_*` fallback for Claude 4 API format; cache Activity tags and counters wired |
| `src/lab-observability-api/Telemetry/GatewayTelemetry.cs` | build | Added `CacheHits` (`ai.provider.cache.hits`) and `CacheMisses` (`ai.provider.cache.misses`) counters |
| `docs/adr/ADR-009-implement-prompt-caching-inside-provider-boundary.md` | build | New ADR — Accepted; documents placement decision and forward-compatibility path |
| `docs/notes/Day-007/02-completion-checklist.md` | verification | All 6 local verification items marked `[x]`; bug fixes and files-changed section added |
| `docs/notes/Day-007/04-posture-check.md` | docs pass | All 4 posture questions answered; graveyard entry for `claude-opus-4-6` included |
| `docs/notes/Day-007/03-architect-thinking.md` | docs pass | New section 8 — Claude 4 API format discoveries: TTL requirement, nested response format, model ID silent failure |
| `CLAUDE.md` | docs pass + deploy | Gotchas: TTL, model ID, nested usage format, appsettings PATCH workaround; files-changed.md convention; north star items annotated with ADR numbers and done status |
| `Infra/Day-007/appsettings-template.md` | docs pass | `cache_control` description updated to include TTL; model row corrected from `claude-opus-4-6` to `claude-sonnet-4-6` |
| `docs/notes/Day-007/01-summary.md` | docs pass → collab-lens | `BuildAnthropicRequest` description updated; collab-lens block inserted under "Whose Problem Am I Solving?" — primary: Cloud & Model-Vendor Support |
| `.claude/commands/new-day.md` | docs pass | Added `files-changed.md` and `appsettings-template.md` to scaffold list; added required-content templates for both |
| `.claude/instructions/daily-workflow.md` | docs pass | STEP 7 manual settings block replaced with automatic appsettings-template.md note; STEP 8 item 6 added for files-changed.md upsert; STEP 1 description updated |
| `.claude/commands/deploy.md` | docs pass | Step 1b added — reads appsettings-template.md and applies settings before publish; DO NOT rule added for settings-apply failure |
| `docs/notes/Day-007/02-completion-checklist.md` | audit + deploy | cache_control format corrected; three stale [ ] items marked [x]; all deploy items marked [x] after Azure verification |
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
| `docs/notes/Day-007/06-deployment-log.md` | deployment-log | Created — retroactive deployment record: 3 local bugs found and fixed (wrong model, no TTL, nested cache format), 5 post-deploy tests (health, chat, App Insights cache creation, cache read, KQL Query 8 50% hit rate), issues & fixes |
| `docs/notes/Day-007/02-completion-checklist.md` | drift-fix | Path fixes: `Models/ChatRequest.cs`→`Models/AI/ChatRequest.cs`, `Models/ChatResponse.cs`→`Models/AI/ChatResponse.cs`, `Providers/ClaudeChatModelProvider.cs`→`Services/AI/ClaudeChatModelProvider.cs` (logged under Day 009 originally — folded in here as part of the changelog migration since it's Day 007's own file) |

## Day 008

| File | Step | Change |
|---|---|---|
| `docs/adr/ADR-010-introduce-parallel-batch-provider-abstraction.md` | populate | Status set to Accepted; resolved ⟨confirm⟩ placeholder |
| `docs/adr/ADR-010-introduce-parallel-batch-provider-abstraction.md` | drift-fix | ISP cost note: `AiChatController` corrected to actual controller name `AiController` |
| `docs/notes/Day-008/01-summary.md` | populate | Full 13-section summary written |
| `docs/notes/Day-008/02-completion-checklist.md` | populate | Phase A–E items defined; docs pass: all verified items marked [x] |
| `src/lab-observability-api/Models/AI/BatchProcessingStatus.cs` | build | Created — enum: InProgress, Canceling, Ended |
| `src/lab-observability-api/Models/AI/BatchJob.cs` | build | Created — submit return type |
| `src/lab-observability-api/Models/AI/BatchJobStatus.cs` | build | Created — poll return type with per-bucket counts |
| `src/lab-observability-api/Models/AI/BatchResult.cs` | build | Created — per-request result |
| `src/lab-observability-api/Services/AI/IBatchChatModelProvider.cs` | build | Created — batch seam interface |
| `src/lab-observability-api/Services/Claude/ClaudeBatchApiClient.cs` | build | Created — Anthropic Batch API HTTP client, no resilience on submit |
| `src/lab-observability-api/Services/AI/ClaudeBatchChatModelProvider.cs` | build | Created — implements IBatchChatModelProvider |
| `src/lab-observability-api/Controllers/AiBatchController.cs` | build | Created — POST/GET/GET batch endpoints |
| `src/lab-observability-api/Telemetry/GatewayTelemetry.cs` | build | Added BatchJobsSubmitted counter, BatchJobsCompleted counter, BatchResultCount histogram |
| `src/lab-observability-api/Program.cs` | build | Registered ClaudeBatchApiClient HttpClient (no resilience) and IBatchChatModelProvider |
| `docs/standards/kql-cookbook.md` | docs pass | Added Query 10 (batch activity + cost vs sync equivalent); batch latency alerting note |
| `docs/architecture/day-008-batch-api-cost-controls.md` | docs pass | Populated — architecture delta, sequence flow, telemetry delta |
| `docs/notes/Day-008/03-architect-thinking.md` | docs pass | Written — seam decision, ADR-009 symmetry, no-retry reasoning, stateless principle |
| `docs/notes/_index.md` | docs pass | Day 008 status → Complete |
| `CLAUDE.md` | docs pass | Days 1–8 complete; cost controls item 3 updated with ADR-010 |
| `src/lab-observability-api/Options/AnthropicOptions.cs` | build | Added MaxBatchSize (default 100) |
| `src/lab-observability-api/Controllers/AiBatchController.cs` | build | Added budget cap check: rejects batches over MaxBatchSize with batch_size_exceeded error |
| `docs/notes/Day-008/04-posture-check.md` | docs pass | Filled — four posture questions answered with honest graveyard-ready admissions |
| `docs/standards/_principles.md` | docs pass | Appended 4 Day 8 graveyard rows: contract divergence, accepted ADR with placeholder, missing blast-radius cap, build lock |
| `docs/notes/Day-008/01-summary.md` | docs pass → collab-lens | Corrected ADR filename; added files-changed to artifacts list; collab-lens block inserted under "Whose Problem Am I Solving?" — primary: Eng Manager / Tech Lead |
| `.claude/instructions/daily-workflow.md` | docs pass | Added auto-print handoff section and step transition prompts |
| `docs/notes/Day-005/02-completion-checklist.md` | alignment | Removed template code-block wrapper; updated two missing file refs to their promoted locations |
| `src/lab-observability-api/CLAUDE.md` | alignment | Updated "Day 6 Complete" → "Days 6–8 Complete"; removed (forthcoming) from observability-architecture.md link |
| `src/lab-observability-api/Program.cs` | alignment | Replaced stale "Day 7 will replace this" comment with final-state note per ADR-006 |
| `src/lab-observability-api/Options/AnthropicOptions.cs` | alignment | Updated default Model from stale `claude-opus-4-7` to `claude-sonnet-4-6` |
| `docs/certifications/ai-102/domains/001-plan-manage/concepts.md` | cert-update | Day 8 additions: cost-per-token attribution, operationalizing generative AI, quota management, model selection governance, responsible AI |
| `docs/certifications/ai-102/domains/001-plan-manage/practice-q.md` | cert-update | Q1–Q5: cost attribution, quota isolation, operationalization, model selection, responsible AI / content safety |
| `docs/certifications/ai-102/domains/001-plan-manage/day-mapping.md` | cert-update | Day-008 row added |
| `docs/certifications/ai-102/domains/002-generative-ai/concepts.md` | cert-update | Batch API processing pattern, batch vs. synchronous routing, budget controls, Azure OpenAI Global Batch quota model |
| `docs/certifications/ai-102/domains/002-generative-ai/practice-q.md` | cert-update | Q1–Q5: choosing processing path, batch lifecycle, budget enforcement, provider abstraction, cost tradeoff |
| `docs/certifications/ai-102/domains/002-generative-ai/day-mapping.md` | cert-update | Day-008 row added |
| `docs/certifications/az-305/domains/004-infrastructure/concepts.md` | cert-update | Async/job-shaped workload pattern, WAF compute selection for batch, cost optimization pillar — deferrable workload routing, latency SLA as architectural constraint |
| `docs/certifications/az-305/domains/004-infrastructure/practice-q.md` | cert-update | Q1–Q5: batch compute selection, WAF cost optimization, async HTTP contract, latency SLA routing, over-engineered compute |
| `docs/certifications/az-305/domains/004-infrastructure/day-mapping.md` | cert-update | Day-008 row added |
| `docs/certifications/domain-coverage.md` | cert-update | AI-102 Domain 1 and Domain 2 marked Day 8; AZ-305 Domain 4 marked Day 8 |
| `src/lab-observability-api/Controllers/AiBatchController.cs` | audit | Added MaxPromptLength guard on batch submit — S3/S4/C2 RED fix; 400 prompt_too_long returned if any prompt exceeds MaxPromptLength |
| `docs/notes/Day-008/05-audit-log.md` | audit | Created — full retroactive pillars audit; S3/S4/C2 RED fixed → GREEN; R4/RA2/RA3/RA4 YELLOW accepted debt |
| `docs/notes/Day-008/06-deployment-log.md` | deployment-log | Created — retroactive deployment record: 7 local tests (batch submit/poll/results/savings/MaxBatchSize guard/MaxPromptLength guard), 5 Azure tests (health, batch submit with real ID, status poll, App Insights batch counter, sync regression), issues & fixes |
| `docs/notes/Day-008/01-summary.md` | drift-fix | Full rewrite of "What I Will Build" / "Step-by-Step Execution" / "Artifacts" / "Portfolio Value" sections — pre-build planning names (`IBatchJobProvider`, `BatchController`, `Models/Batch/...`, `batch.job.*` metrics) replaced throughout with the actual built names (`IBatchChatModelProvider`, `AiBatchController`, `Models/AI/...`, `ai.provider.batch.*`); this is the divergence `graveyard.md`'s Day 8 entry already documents — added a note pointing to it rather than erasing the history. User confirmed rewrite-over-annotate via AskUserQuestion. (logged under Day 009 originally — folded in here since it's Day 008's own file) |

## Day 009

| File | Step | Change |
|---|---|---|
| `.claude/commands/new-day.md` | scaffold | Expanded posture-check Q4 to all four levels (10yo / CEO / Engineer / Architect); added architect-thinking.md scaffold template with required CEO Framing section; added Q5 (pillars) |
| `.claude/skills/pillars-audit/SKILL.md` | scaffold | New skill — 6-pillar pre-deploy audit (5 WAF pillars + Responsible AI); GREEN/YELLOW/RED per pillar; 34 codebase-specific checks |
| `.claude/instructions/daily-workflow.md` | scaffold | STEP 8 renamed TEST + AUDIT; audit sub-step added; STEP 0 updated to five questions; STEP 11 updated to include Q5 |
| `docs/standards/_principles.md` | scaffold | Q5 added to Daily Posture Check section; pillars-audit skill referenced |
| `docs/notes/Day-009/04-posture-check.md` | scaffold | Q4 updated to four-level template; Q5 added |
| `docs/notes/Day-009/03-architect-thinking.md` | scaffold | CEO Framing section added after §4 |
| `src/lab-observability-api/Models/AI/ChatChunk.cs` | build (Phase A) | New file — ChatChunk record (TextDelta, StopReason?, Usage?); ChatChunkUsage record (InputTokens, OutputTokens, CacheReadTokens, CacheCreationTokens) |
| `src/lab-observability-api/Services/AI/IChatModelProvider.cs` | build (Phase A) | StreamAsync added with default degrade implementation (yields buffered response as single terminal chunk; Liskov guarantee for non-streaming providers) |
| `src/lab-observability-api/Services/Claude/ClaudeApiClient.cs` | build (Phase B) | StreamChatAsync added — POST stream:true, ResponseHeadersRead, SSE parse (message_start/content_block_delta/message_delta/message_stop), nested Claude 4 cache_creation format, CancellationToken propagated, response disposed in finally |
| `src/lab-observability-api/Services/AI/ClaudeChatModelProvider.cs` | build (Phase B) | StreamAsync override — ai.chat.stream outer span, TTFT stopwatch, StreamTtftMs recorded on first chunk, usage logged on final chunk |
| `src/lab-observability-api/Telemetry/GatewayTelemetry.cs` | build (Phase D) | StreamTtftMs histogram added (ai.provider.stream.ttft_ms) |
| `src/lab-observability-api/Controllers/AiController.cs` | build (Phase C) | `POST /api/ai/chat/stream` added — validation before SSE headers, IHttpResponseBodyFeature.DisableBuffering, X-Accel-Buffering:no, per-chunk FlushAsync, mid-stream error event, client disconnect swallowed |
| `docs/standards/kql-cookbook.md` | audit | Queries 11 (TTFT p50/p95/p99) and 12 (TTFT by model) added; Day 9+ dimension added to conventions |
| `CLAUDE.md` | docs pass | Day status updated (1–8 → 1–9 complete, Day 10 next); streaming added to Phase 1 completed items with ADR-011 |
| `docs/notes/Day-009/02-completion-checklist.md` | docs pass | Deploy and Azure verification items marked [x] with evidence |
| `docs/notes/_index.md` | docs pass | Day 009 status updated to Complete |
| `src/lab-observability-api/Services/AI/ClaudeChatModelProvider.cs` | posture gap fix | try/finally added around await foreach — logs WRN with CorrelationId+DurationMs when stream ends without final usage data (client disconnect or mid-stream error); closes RA6 audit-trail gap |
| `docs/adr/ADR-011-place-streaming-on-the-interactive-provider-seam.md` | posture gap fix | Side-by-side reconciliation table added to Decision section: batch vs streaming on all four decision variables (lifecycle, Liskov, ISP, verdict) |
| `src/lab-observability-api/Telemetry/GatewayTelemetry.cs` | metric name fix | StreamTtftMs metric renamed from ai.chat.stream.ttft_ms → ai.provider.stream.ttft_ms to align with ai.provider.* namespace convention |
| `docs/adr/ADR-011-place-streaming-on-the-interactive-provider-seam.md` | metric name fix | Implementation Notes updated to reflect corrected metric name ai.provider.stream.ttft_ms |
| `src/lab-observability-api/Services/AI/ClaudeChatModelProvider.cs` | metric name fix | finally block: client disconnect (ct.IsCancellationRequested) downgraded to LogDebug; unexpected stream end kept as LogWarning |
| `docs/notes/Day-009/02-completion-checklist.md` | metric name fix | Checklist item updated — deviation note removed, metric correctly named ai.provider.stream.ttft_ms |
| `docs/notes/Day-009/04-posture-check.md` | close (STEP 12) | All five posture questions answered; template placeholders resolved from code inspection; RA6 gap documented as YELLOW (real but not fully closed — no automated fault-injection test yet) |
| `docs/standards/_principles.md` | close (STEP 12) | Four Day 9 graveyard entries added: p95/p99 tail measurement, CS1626 error-propagation design, ADR trilogy reconciliation, RA6 streaming disconnect gap |
| `docs/notes/Day-009/02-completion-checklist.md` | close (STEP 12) | posture-check and final git commit items marked [x] |
| `docs/certifications/ai-102/domains/002-generative-ai/concepts.md` | cert-update | Day 9 additions: SSE streaming, TTFT as latency SLO, batch API pattern, batch vs. sync routing, budget controls, Azure OpenAI Global Batch quota model |
| `docs/certifications/ai-102/domains/002-generative-ai/practice-q.md` | cert-update | Q6–Q10: App Service SSE buffering, TTFT vs. total latency, mid-stream error handling, client disconnect resource management, provider interface design for streaming |
| `docs/certifications/ai-102/domains/002-generative-ai/day-mapping.md` | cert-update | Day-009 row added |
| `docs/certifications/az-305/domains/004-infrastructure/concepts.md` | cert-update | Day 9 additions: streaming vs. buffered response pattern, TTFT SLO design, proxy buffering on App Service, client disconnect cost governance, Liskov test for interface extension |
| `docs/certifications/az-305/domains/004-infrastructure/practice-q.md` | cert-update | Q6–Q10: streaming driver, TTFT SLO definition, proxy buffering fix, client disconnect cost, LSP interface design |
| `docs/certifications/az-305/domains/004-infrastructure/day-mapping.md` | cert-update | Day-009 row added |
| `docs/certifications/az-900/domains/003-azure-management-governance/concepts.md` | cert-update | Day 9 additions: Azure Monitor overview, custom metrics/histograms via OpenTelemetry, App Service as streaming compute host |
| `docs/certifications/az-900/domains/003-azure-management-governance/practice-q.md` | cert-update | Q1–Q5: Azure Monitor data types, alert rules, custom metrics, action groups, monitor scope |
| `docs/certifications/az-900/domains/003-azure-management-governance/day-mapping.md` | cert-update | Day-009 row added |
| `docs/certifications/domain-coverage.md` | cert-update | AI-102 Domain 2 marked Day 9; AZ-305 Domain 4 marked Day 9; AZ-900 Domain 3 marked Day 9 |
| `docs/notes/Day-009/01-summary.md` | collab-lens | Collaboration Lens block inserted under "Whose Problem Am I Solving?" — primary: DevOps / SRE |
| `docs/notes/Day-009/05-audit-log.md` | audit | Created — full retroactive pillars audit; no RED items; R4/RA3/RA6 YELLOW accepted debt |
| `docs/notes/Day-009/06-deployment-log.md` | deployment-log | Created — retroactive deployment record: 6 local tests (streaming incremental delivery, MaxPromptLength guard, mid-stream error frame, sync regression, TTFT log), 4 Azure tests (health, SSE not buffered with timestamps, sync regression, KQL Query 11 p50=1354ms), issues & fixes |
| `src/lab-observability-api/Program.cs` | post-close | Added `public partial class Program { }` at bottom — required for `WebApplicationFactory<Program>` in test project |
| `cloud-architecture-lab.sln` | post-close | Test project added via `dotnet sln add` |
| `src/lab-observability-api.Tests/lab-observability-api.Tests.csproj` | post-close | New test project — xUnit + Microsoft.AspNetCore.Mvc.Testing 8.0.0, ProjectReference to main API |
| `src/lab-observability-api.Tests/GatewayWebApplicationFactory.cs` | post-close | New — `WebApplicationFactory<Program>` with ConfigureTestServices; virtual TestApiKey property; EmptyApiKeyWebApplicationFactory subclass |
| `src/lab-observability-api.Tests/IntegrationTestCollection.cs` | post-close | New — [CollectionDefinition("Integration")] + `ICollectionFixture<GatewayWebApplicationFactory>`; shared factory across all integration tests |
| `src/lab-observability-api.Tests/AssemblyInfo.cs` | post-close | New — [assembly: CollectionBehavior(DisableTestParallelization = true)]; prevents factory race between Integration collection and HealthReadyMisconfiguredTests |
| `src/lab-observability-api.Tests/Fakes/FakeChatModelProvider.cs` | post-close | New — IChatModelProvider fake with ExceptionToThrow property, StreamAsync support, Reset() |
| `src/lab-observability-api.Tests/Fakes/FakeBatchChatModelProvider.cs` | post-close | New — IBatchChatModelProvider fake with canned responses for SubmitAsync, GetStatusAsync, GetResultsAsync |
| `src/lab-observability-api.Tests/Controllers/HealthTests.cs` | post-close | New — 3 tests: `/health`, `/health/live`, `/health/ready` (200 when configured) |
| `src/lab-observability-api.Tests/Controllers/AiControllerTests.cs` | post-close | New — 13 tests: input validation (empty/whitespace/too-long), happy path, ClaudeProviderException → 502/503, generic exception → 500 no stack trace, SSE headers, SSE chunk JSON shape |
| `src/lab-observability-api.Tests/Controllers/AiBatchControllerTests.cs` | post-close | New — 7 tests: batch input validation and happy paths for submit/status/results endpoints |
| `src/lab-observability-api.Tests/Controllers/MiddlewareTests.cs` | post-close | New — 2 tests: x-correlation-id auto-set, correlation ID round-trip echo |
| `src/lab-observability-api.Tests/Controllers/HealthReadyMisconfiguredTests.cs` | post-close | New — 1 test: `/health/ready` → 503 when ApiKey is empty (uses EmptyApiKeyWebApplicationFactory) |
| `.claude/instructions/daily-workflow.md` | post-close | STEP 7: added test coverage requirement table (CEO/Architect/AI Engineer lenses); added `dotnet test` gate before STEP 8 |
| `.claude/commands/new-day.md` | post-close | Added Test Gate section in completion-checklist scaffold template; skip condition for pure-docs days |
| `.claude/commands/deploy.md` | post-close | Step 0 renamed to "Pre-deploy gates"; added 0a: `dotnet test` must pass before pillars audit; deployment-log.md template updated with `dotnet test` result line |
| `docs/certifications/az-104/objectives.md` | post-close | Expanded stub to full domain map with sub-objectives across all 5 domains |
| `CLAUDE.md` | post-close | Test count updated from 19 → 25; then hardcoded count removed entirely (self-maintaining) |
| `src/lab-observability-api.Tests/Fakes/FakeBatchChatModelProvider.cs` | regression-tests | Added ExceptionToThrow/Reset()/ThrowIfSet() matching FakeChatModelProvider pattern |
| `src/lab-observability-api.Tests/Controllers/AiBatchControllerTests.cs` | regression-tests | 3 new tests: null element → 400; batch status transient → 503; batch results transient → 503 |
| `docs/certifications/az-305/objectives.md` | cert-expansion | Created — full 4-domain map with sub-objectives (Domain 1: identity/governance/monitoring; 2: data storage; 3: business continuity; 4: infrastructure) |
| `docs/certifications/az-900/domains/001-cloud-concepts/day-mapping.md` | cert-expansion | Populated from Days 1–2: shared responsibility, CapEx/OpEx, service types, economies of scale |
| `docs/certifications/az-900/domains/002-azure-architecture-services/day-mapping.md` | cert-expansion | Populated from Days 1–2, 6: regions, resource groups, subscriptions, ARM, App Service, App Insights |
| `.gitignore` | cleanup | Removed duplicate bin/obj/publish/zip entries; added section comments |
| `docs/standards/azure-environment.md` | sync-audit | `claude-opus-4-7` corrected to `claude-opus-4-6` — the actual invalid model ID discovered Day 7 |
| `docs/certifications/README.md` | sync-audit | Cert path updated to reflect AI-102 retired; AI-102 relabeled "historical record only" |
| `docs/certifications/ai-102/README.md` | sync-audit | Retirement notice added at top — do not schedule |
| `docs/certifications/az-104/README.md` | sync-audit | Domain 5 name: "Monitor and back up" → "Monitor and maintain" (current exam wording) |
| `docs/certifications/az-305/README.md` | sync-audit | Build→Cert mapping table: added Day 7, 8, 9 rows (were missing despite domain-level entries existing) |
| `docs/certifications/az-900/README.md` | sync-audit | Files section updated to reference actual domains/ structure (was referencing non-existent study-notes/, practice/) |
| `docs/certifications/domain-coverage.md` | sync-audit | "Last updated" header corrected from Day-007 to Day-009 |
| `.claude/skills/dotnet-api-conventions/SKILL.md` | sync-audit | Line 154: future tense "Day 6 will add Serilog" corrected to past tense |
| `.claude/skills/adr-writer/SKILL.md` | sync-audit | ADR-006 example filename corrected to actual ADR-006 name |
| `src/lab-observability-api/Controllers/AiBatchController.cs` | bug-fix | Null element in request list causes NRE before whitespace check — added `r is null` guard |
| `src/lab-observability-api/Services/Claude/ClaudeApiClient.cs` | bug-fix | Null responseText falls through to return raw provider JSON — now returns empty string with warning log |
| `src/lab-observability-api/Services/Claude/ClaudeApiClient.cs` | bug-fix | JsonException in SSE loop silently swallowed — now logs warning before continuing |
| `src/lab-observability-api/Services/Claude/ClaudeBatchApiClient.cs` | bug-fix | GetStatusAsync and GetResultsAsync hardcode isTransient:false even for 429/503 — fixed to correct transient detection |
| `.claude/hooks/cert-tag.json` | cert-cleanup | Removed retired AI-102 tags; updated to AZ-104/AZ-305 only |
| `.gitignore` | cert-cleanup | Added `.claude/cert-tags-today.txt` (hook-generated, machine-local) |
| `docs/certifications/az-104/objectives.md` | cert-expansion | Expanded stub to full domain map with sub-objectives across all 5 domains |
| `docs/architecture/day-009-streaming-responses-on-interactive-path.md` | drift-fix | Telemetry table: `StreamFirstTokenMs` corrected to actual `StreamTtftMs` (`ai.provider.stream.ttft_ms`); `StreamDurationMs` row annotated as deferred, not built |
| `docs/notes/Day-009/03-architect-thinking.md` | drift-fix | §4 first-token latency metric reference corrected from `StreamFirstTokenMs` to actual `StreamTtftMs` (`ai.provider.stream.ttft_ms`) |
| `docs/certifications/az-104/README.md` | drift-fix | Activation timeline corrected from "Day 10-15" to restructured "Day 035 (target: Day 070)" |
| `docs/certifications/az-104/objectives.md` | drift-fix | Footer activation note corrected from "Day 10-15" to restructured "Day 035 (target: Day 070)" |
| `docs/notes/Day-009/01-summary.md` | drift-fix | Cert Reinforcement AZ-104 line updated to reflect restructured start (~Day 035) instead of stale "~Day 10-15" |
| `.claude/skills/dotnet-api-conventions/SKILL.md` | drift-fix | Error Handling example replaced a fictional `ExceptionHandlingMiddleware` class with the actual implementation — inline `app.Use(...)` global exception pipeline in `Program.cs` catching `ClaudeProviderException` then `Exception` |
| `docs/standards/error-handling-standard.md` | drift-fix | Rewrote error response contract, taxonomy, classification rules, retry policy, and logging table to match actual code — fictional exception hierarchy (`ProviderRateLimitException` etc.), fictional `ExceptionHandlingMiddleware`, SCREAMING_CASE codes (`VALIDATION_FAILED`, `STREAM_INTERRUPTED`, etc.), fictional 499 client-disconnect convention, and a false batch-retry claim (ADR-010 explicitly has no resilience pipeline on the batch client) all replaced with the real `ClaudeProviderException`/`invalid_request`/`stream_error`-style taxonomy and the real `Program.cs` resilience config |
| `docs/standards/security-standard.md` | drift-fix | "No stack traces" row: removed fictional `ExceptionHandlingMiddleware` reference; prompt validation section corrected from "automatic `[ApiController]` model-state validation + `VALIDATION_FAILED`" (never built — `ChatRequest` has no data annotations) to the actual manual guard-clause pattern; Phase 2 rate-limiting code renamed from fictional `PROVIDER_RATE_LIMITED` to taxonomy-consistent `rate_limited` (not yet implemented) |
| `docs/standards/testing-standard.md` | drift-fix | Corrected the integration-test description: `GatewayWebApplicationFactory` fakes at the `IChatModelProvider`/`IBatchChatModelProvider` seam (`FakeChatModelProvider`/`FakeBatchChatModelProvider`), not via a `FakeHttpMessageHandler` (no such class exists; there is no `HttpClient`-level test coverage today) |
| `docs/standards/provider-onboarding.md` | drift-fix | Removed reference to a nonexistent `FakeHttpMessageHandler`/`<Provider>ApiClientTests.cs` test pattern; replaced with the actual provider-seam fake pattern used by existing controller tests |
| `scripts/symbol-drift-check.js` | tooling | New — scans `src/lab-observability-api*/**/*.cs` for type/member identifiers and `ai.*` telemetry string literals, then flags backtick-wrapped doc references and `src/` paths that don't match current source. Excludes `docs/adr/`, `docs/architecture/`, `*-log.md`, `commit-convention.md`, `graveyard.md`, and explicit Phase-2/future-scope stubs (`agent-patterns.md`, `rag-patterns.md`, `responsible-ai.md`, `multi-turn-context.md`) since those genres intentionally name rejected/not-yet-built designs |
| `docs/certifications/ai-102/domains/002-generative-ai/concepts.md` | drift-fix | `IBatchProvider` (×3) → `IBatchChatModelProvider` — found by `symbol-drift-check.js` |
| `docs/certifications/ai-102/domains/002-generative-ai/day-mapping.md` | drift-fix | `IBatchProvider` → `IBatchChatModelProvider` |
| `docs/certifications/ai-102/domains/002-generative-ai/practice-q.md` | drift-fix | `IBatchProvider` (×2) → `IBatchChatModelProvider` |
| `docs/certifications/az-305/domains/001-identity-governance-monitoring/concepts.md` | drift-fix | `IBatchProvider` → `IBatchChatModelProvider` |
| `docs/notes/Day-006/01-summary.md` | drift-fix | Phase E plan line annotated with actual implementation (inline `app.Use(...)`, no `ExceptionHandlingMiddleware.cs` class); Artifacts list corrected: dropped fictional `ExceptionHandlingMiddleware.cs`/`Providers/ClaudeChatModelProvider.cs` paths, fixed wrong ADR-006 and day-006 architecture doc filenames (logged here, the day the edit happened, not under Day 006) |
| `docs/notes/Day-006/02-completion-checklist.md` | drift-fix | Checklist item label corrected from "`ExceptionHandlingMiddleware` created and registered" to "Global exception handling registered" (no such class exists) — logged here, the day the edit happened |
| `docs/notes/Day-008/01-summary.md` | drift-fix | Full rewrite of "What I Will Build" / "Step-by-Step Execution" / "Artifacts" / "Portfolio Value" sections — pre-build planning names (`IBatchJobProvider`, `BatchController`, `Models/Batch/...`, `batch.job.*` metrics) replaced throughout with the actual built names (`IBatchChatModelProvider`, `AiBatchController`, `Models/AI/...`, `ai.provider.batch.*`); this is the divergence `graveyard.md`'s Day 8 entry already documents — added a note pointing to it rather than erasing the history. User confirmed rewrite-over-annotate via AskUserQuestion. (logged here, the day the edit happened) |
| `docs/standards/slo-performance.md` | drift-fix | Span names corrected: `ai.chat`→`ai.chat.complete` (×2); `ai.batch` span (never built) replaced with an honest note that only `ai.provider.batch.*` counters exist today, no duration span |
| `.claude/instructions/daily-workflow-steps.md` | tooling | STEP 8 now runs `node scripts/symbol-drift-check.js` after the pillars audit, before STEP 9 — drift caught the same day it's introduced, not just at the next `/repo-audit` |
| `.claude/instructions/daily-workflow.md` | tooling | STEP 8 one-line overview updated to mention the symbol-drift-check gate |
| `.claude/commands/repo-audit.md` | tooling | Replaced the git-diff-range-based Check 9 (broken in this repo — bulk consolidation commits make "oldest commit for Day NNN" an unreliable diff base, confirmed when it spanned nearly the whole repo history) with `node scripts/symbol-drift-check.js`; replaced Check 6's "row added" behavior to scope strictly to the current day (no scattering into historical folders); fixed 2 pre-existing MD033 violations (`"Day-<NNN>"` in plain quotes read as inline HTML); fixed Check 3's markdownlint command — `**/*.md` alone silently skips `.claude/` (dotfolder), giving a false "0 violations" for all skills/commands this whole session; now `markdownlint "**/*.md" ".claude/**/*.md" ...` |
| `docs/notes/changelog.md` | tooling | New — consolidated all 9 `Day-NNN/07-files-changed.md` files into this single running changelog with `## Day NNN` sections; the per-day files and the per-day-location convention are retired. Driven by repeated friction this session: cross-cutting drift fixes required looking up "which day owns this file" before logging, and rows ended up scattered across closed historical day folders. One file, one `grep`, no lookup. |
| `docs/notes/Day-001/07-files-changed.md` through `Day-009/07-files-changed.md` | tooling | Deleted — content migrated into this file's `## Day 001`–`## Day 009` sections |
| `CLAUDE.md` | tooling | `07-files-changed.md` convention block rewritten for `docs/notes/changelog.md`; architecture map line added |
| `README.md` | tooling | File-tree comment updated to reference `docs/notes/changelog.md` |
| `.claude/commands/new-day.md` | tooling | Scaffold no longer creates a per-day `07-files-changed.md`; now appends a `## Day NNN` section to `docs/notes/changelog.md` |
| `.claude/commands/collab-lens.md` | tooling | Upsert target updated to the current day's section in `docs/notes/changelog.md` |
| `.claude/skills/collaboration-lens/SKILL.md` | tooling | Upsert target updated; dropped the now-meaningless "add a row for 07-files-changed.md itself" self-reference step |
| `.claude/skills/pillars-audit/SKILL.md` | tooling | All `07-files-changed.md` references (inputs, O4 check, YELLOW-debt upsert) updated to `docs/notes/changelog.md`, current day's section |
| `docs/standards/testing-standard.md` | tooling | Reference updated |
| `docs/standards/security-standard.md` | tooling | Reference updated (security-scan log location) |
| `docs/standards/monitoring-runbook.md` | tooling | Reference updated |
| `docs/standards/portfolio-strategy.md` | tooling | Reference updated |
| `docs/standards/architect-thinking-template.md` | tooling | Reference updated |
| `docs/standards/dependency-policy.md` | tooling | Reference updated (×2) |
| `docs/standards/cost-governance.md` | tooling | Reference updated |
| `docs/standards/provider-onboarding.md` | tooling | Reference updated (×2) |
| `scripts/symbol-drift-check.js` | tooling | Exclude-pattern updated from `07-files-changed\.md$` to `docs/notes/changelog\.md$` |
| `docs/standards/claude-tooling-reference.html` | docs pass | TOC restructured — `docs/standards/` section split into a dedicated file-by-file breakdown plus a new "other key files" section |
| `docs/standards/claude-tooling-reference.pdf` | docs pass | Regenerated from the updated HTML |
| `docs/standards/csharp-codebase-reference.html` | docs pass | New — C# codebase reference doc, companion to the Claude Code tooling reference |
| `docs/standards/csharp-codebase-reference.pdf` | docs pass | New — PDF render of the C# codebase reference |

## Day 010 (2026-06-26)

| File | Step | Change |
|---|---|---|
| `docs/certifications/ai-102/objectives.md` | name-audit | Retirement notice replaces TODO stub — exam retired June 30, 2026 |
| `docs/standards/azure-environment.md` | sync-check | Added Compute table with App Service Plan (`asp-ai-lab-dev-eastus-gio`) — gap surfaced by name-check.js first run |
| `scripts/name-check.js` | tooling | New — naming consistency companion to symbol-drift-check.js; checks broader C# suffixes, Azure resource names, slash command refs, ADR-NNN refs |
| `.claude/commands/sync-check.md` | tooling | New — day-agnostic reference integrity + naming check; replaces inline symbol-drift-check.js call in STEP 8 |
| `.claude/commands/name-audit.md` | tooling | New — periodic AI-driven cross-doc naming consistency audit; STEP 12 optional |
| `.claude/instructions/daily-workflow.md` | tooling | STEP 8 updated to /sync-check; /sync-check and /name-audit added to Command Reference table |
| `.claude/instructions/daily-workflow-steps.md` | tooling | STEP 8 replaced inline script call with /sync-check; STEP 12 added optional /name-audit pass |
| `docs/adr/ADR-010-introduce-parallel-batch-provider-abstraction.md` | name-audit | Implementation Notes: 7 stale file paths corrected — Models/ → Models/AI/, Providers/ → Services/AI/; BatchJobStatus description corrected (not an enum); BatchProcessingStatus added to new-files list |
