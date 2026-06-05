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
| `src/lab-observability-api/Telemetry/GatewayTelemetry.cs` | build (Phase D) | StreamTtftMs histogram added (ai.provider.stream.ttft_ms) |
| `src/lab-observability-api/Controllers/AiController.cs` | build (Phase C) | POST /api/ai/chat/stream added — validation before SSE headers, IHttpResponseBodyFeature.DisableBuffering, X-Accel-Buffering:no, per-chunk FlushAsync, mid-stream error event, client disconnect swallowed |
| `docs/standards/kql-cookbook.md` | audit | Queries 11 (TTFT p50/p95/p99) and 12 (TTFT by model) added; Day 9+ dimension added to conventions |
| `docs/notes/Day-009/files-changed.md` | audit | This file — Phase A–D code rows and audit rows upserted |
| `CLAUDE.md` | docs pass | Day status updated (1–8 → 1–9 complete, Day 10 next); streaming added to Phase 1 completed items with ADR-011 |
| `docs/notes/Day-009/completion-checklist.md` | docs pass | Deploy and Azure verification items marked [x] with evidence |
| `docs/notes/_index.md` | docs pass | Day 009 status updated to Complete |
| `docs/notes/Day-009/files-changed.md` | docs pass | This file — docs pass rows upserted |
| `src/lab-observability-api/Services/AI/ClaudeChatModelProvider.cs` | posture gap fix | try/finally added around await foreach — logs WRN with CorrelationId+DurationMs when stream ends without final usage data (client disconnect or mid-stream error); closes RA6 audit-trail gap |
| `docs/adr/ADR-011-place-streaming-on-the-interactive-provider-seam.md` | posture gap fix | Side-by-side reconciliation table added to Decision section: batch vs streaming on all four decision variables (lifecycle, Liskov, ISP, verdict) |
| `src/lab-observability-api/Telemetry/GatewayTelemetry.cs` | metric name fix | StreamTtftMs metric renamed from ai.chat.stream.ttft_ms → ai.provider.stream.ttft_ms to align with ai.provider.* namespace convention |
| `docs/adr/ADR-011-place-streaming-on-the-interactive-provider-seam.md` | metric name fix | Implementation Notes updated to reflect corrected metric name ai.provider.stream.ttft_ms |
| `src/lab-observability-api/Services/AI/ClaudeChatModelProvider.cs` | metric name fix | finally block: client disconnect (ct.IsCancellationRequested) downgraded to LogDebug; unexpected stream end kept as LogWarning |
| `docs/notes/Day-009/completion-checklist.md` | metric name fix | Checklist item updated — deviation note removed, metric correctly named ai.provider.stream.ttft_ms |
| `docs/notes/Day-009/posture-check.md` | close (STEP 12) | All five posture questions answered; template placeholders resolved from code inspection; RA6 gap documented as YELLOW (real but not fully closed — no automated fault-injection test yet) |
| `docs/standards/_principles.md` | close (STEP 12) | Four Day 9 graveyard entries added: p95/p99 tail measurement, CS1626 error-propagation design, ADR trilogy reconciliation, RA6 streaming disconnect gap |
| `docs/notes/Day-009/completion-checklist.md` | close (STEP 12) | posture-check and final git commit items marked [x] |
| `docs/notes/Day-009/files-changed.md` | close (STEP 12) | This file — close pass rows upserted |
