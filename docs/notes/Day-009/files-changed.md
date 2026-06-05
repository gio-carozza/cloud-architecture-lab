# Day 009 — Files Changed

| File | Step | Change |
|---|---|---|
| `.claude/commands/new-day.md` | scaffold | Expanded posture-check Q4 to all four levels (10yo / CEO / Engineer / Architect); added architect-thinking.md scaffold template with required CEO Framing section; added Q5 (pillars) |
| `.claude/skills/pillars-audit/SKILL.md` | scaffold | New skill — 6-pillar pre-deploy audit (5 WAF pillars + Responsible AI); GREEN/YELLOW/RED per pillar; 34 codebase-specific checks |
| `.claude/instructions/daily-workflow.md` | scaffold | STEP 8 renamed TEST + AUDIT; audit sub-step added; STEP 0 updated to five questions; STEP 11 updated to include Q5 |
| `docs/standards/_principles.md` | scaffold | Q5 added to Daily Posture Check section; pillars-audit skill referenced |
| `docs/notes/Day-009/posture-check.md` | scaffold | Q4 updated to four-level template; Q5 added |
| `docs/notes/Day-009/architect-thinking.md` | scaffold | CEO Framing section added after §4 |
| `src/lab-observability-api/Models/AI/ChatChunk.cs` | build (Phase A) | New file — ChatChunk record (TextDelta, StopReason?, Usage?); ChatChunkUsage record (InputTokens, OutputTokens, CacheReadTokens, CacheCreationTokens) |
| `src/lab-observability-api/Services/AI/IChatModelProvider.cs` | build (Phase A) | StreamAsync added with default degrade implementation (yields buffered response as single terminal chunk; Liskov guarantee for non-streaming providers) |
| `src/lab-observability-api/Services/Claude/ClaudeApiClient.cs` | build (Phase B) | StreamChatAsync added — POST stream:true, ResponseHeadersRead, SSE parse (message_start/content_block_delta/message_delta/message_stop), nested Claude 4 cache_creation format, CancellationToken propagated, response disposed in finally |
| `src/lab-observability-api/Services/AI/ClaudeChatModelProvider.cs` | build (Phase B) | StreamAsync override — ai.chat.stream outer span, TTFT stopwatch, StreamTtftMs recorded on first chunk, usage logged on final chunk |
| `src/lab-observability-api/Telemetry/GatewayTelemetry.cs` | build (Phase D) | StreamTtftMs histogram added (ai.chat.stream.ttft_ms) |
| `src/lab-observability-api/Controllers/AiController.cs` | build (Phase C) | POST /api/ai/chat/stream added — validation before SSE headers, IHttpResponseBodyFeature.DisableBuffering, X-Accel-Buffering:no, per-chunk FlushAsync, mid-stream error event, client disconnect swallowed |
| `docs/standards/kql-cookbook.md` | audit | Queries 11 (TTFT p50/p95/p99) and 12 (TTFT by model) added; Day 9+ dimension added to conventions |
| `docs/notes/Day-009/files-changed.md` | audit | This file — Phase A–D code rows and audit rows upserted |
| `CLAUDE.md` | docs pass | Day status updated (1–8 → 1–9 complete, Day 10 next); streaming added to Phase 1 completed items with ADR-011 |
| `docs/notes/Day-009/completion-checklist.md` | docs pass | Deploy and Azure verification items marked [x] with evidence |
| `docs/notes/_index.md` | docs pass | Day 009 status updated to Complete |
| `docs/notes/Day-009/files-changed.md` | docs pass | This file — docs pass rows upserted |
