# Day 009 — Files Changed

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
| `src/lab-observability-api/Controllers/AiController.cs` | build (Phase C) | POST /api/ai/chat/stream added — validation before SSE headers, IHttpResponseBodyFeature.DisableBuffering, X-Accel-Buffering:no, per-chunk FlushAsync, mid-stream error event, client disconnect swallowed |
| `docs/standards/kql-cookbook.md` | audit | Queries 11 (TTFT p50/p95/p99) and 12 (TTFT by model) added; Day 9+ dimension added to conventions |
| `docs/notes/Day-009/07-files-changed.md` | audit | This file — Phase A–D code rows and audit rows upserted |
| `CLAUDE.md` | docs pass | Day status updated (1–8 → 1–9 complete, Day 10 next); streaming added to Phase 1 completed items with ADR-011 |
| `docs/notes/Day-009/02-completion-checklist.md` | docs pass | Deploy and Azure verification items marked [x] with evidence |
| `docs/notes/_index.md` | docs pass | Day 009 status updated to Complete |
| `docs/notes/Day-009/07-files-changed.md` | docs pass | This file — docs pass rows upserted |
| `src/lab-observability-api/Services/AI/ClaudeChatModelProvider.cs` | posture gap fix | try/finally added around await foreach — logs WRN with CorrelationId+DurationMs when stream ends without final usage data (client disconnect or mid-stream error); closes RA6 audit-trail gap |
| `docs/adr/ADR-011-place-streaming-on-the-interactive-provider-seam.md` | posture gap fix | Side-by-side reconciliation table added to Decision section: batch vs streaming on all four decision variables (lifecycle, Liskov, ISP, verdict) |
| `src/lab-observability-api/Telemetry/GatewayTelemetry.cs` | metric name fix | StreamTtftMs metric renamed from ai.chat.stream.ttft_ms → ai.provider.stream.ttft_ms to align with ai.provider.* namespace convention |
| `docs/adr/ADR-011-place-streaming-on-the-interactive-provider-seam.md` | metric name fix | Implementation Notes updated to reflect corrected metric name ai.provider.stream.ttft_ms |
| `src/lab-observability-api/Services/AI/ClaudeChatModelProvider.cs` | metric name fix | finally block: client disconnect (ct.IsCancellationRequested) downgraded to LogDebug; unexpected stream end kept as LogWarning |
| `docs/notes/Day-009/02-completion-checklist.md` | metric name fix | Checklist item updated — deviation note removed, metric correctly named ai.provider.stream.ttft_ms |
| `docs/notes/Day-009/04-posture-check.md` | close (STEP 12) | All five posture questions answered; template placeholders resolved from code inspection; RA6 gap documented as YELLOW (real but not fully closed — no automated fault-injection test yet) |
| `docs/standards/_principles.md` | close (STEP 12) | Four Day 9 graveyard entries added: p95/p99 tail measurement, CS1626 error-propagation design, ADR trilogy reconciliation, RA6 streaming disconnect gap |
| `docs/notes/Day-009/02-completion-checklist.md` | close (STEP 12) | posture-check and final git commit items marked [x] |
| `docs/notes/Day-009/07-files-changed.md` | close (STEP 12) | This file — close pass rows upserted |
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
| `docs/notes/Day-009/07-files-changed.md` | collab-lens | This file — collab-lens rows upserted |
| `docs/notes/Day-009/05-audit-log.md` | audit | Created — full retroactive pillars audit; no RED items; R4/RA3/RA6 YELLOW accepted debt |
| `docs/notes/Day-009/06-deployment-log.md` | deployment-log | Created — retroactive deployment record: 6 local tests (streaming incremental delivery, MaxPromptLength guard, mid-stream error frame, sync regression, TTFT log), 4 Azure tests (health, SSE not buffered with timestamps, sync regression, KQL Query 11 p50=1354ms), issues & fixes |
| `src/lab-observability-api/Program.cs` | post-close | Added `public partial class Program { }` at bottom — required for WebApplicationFactory<Program> in test project |
| `cloud-architecture-lab.sln` | post-close | Test project added via `dotnet sln add` |
| `src/lab-observability-api.Tests/lab-observability-api.Tests.csproj` | post-close | New test project — xUnit + Microsoft.AspNetCore.Mvc.Testing 8.0.0, ProjectReference to main API |
| `src/lab-observability-api.Tests/GatewayWebApplicationFactory.cs` | post-close | New — WebApplicationFactory<Program> with ConfigureTestServices; virtual TestApiKey property; EmptyApiKeyWebApplicationFactory subclass |
| `src/lab-observability-api.Tests/IntegrationTestCollection.cs` | post-close | New — [CollectionDefinition("Integration")] + ICollectionFixture<GatewayWebApplicationFactory>; shared factory across all integration tests |
| `src/lab-observability-api.Tests/AssemblyInfo.cs` | post-close | New — [assembly: CollectionBehavior(DisableTestParallelization = true)]; prevents factory race between Integration collection and HealthReadyMisconfiguredTests |
| `src/lab-observability-api.Tests/Fakes/FakeChatModelProvider.cs` | post-close | New — IChatModelProvider fake with ExceptionToThrow property, StreamAsync support, Reset() |
| `src/lab-observability-api.Tests/Fakes/FakeBatchChatModelProvider.cs` | post-close | New — IBatchChatModelProvider fake with canned responses for SubmitAsync, GetStatusAsync, GetResultsAsync |
| `src/lab-observability-api.Tests/Controllers/HealthTests.cs` | post-close | New — 3 tests: /health, /health/live, /health/ready (200 when configured) |
| `src/lab-observability-api.Tests/Controllers/AiControllerTests.cs` | post-close | New — 13 tests: input validation (empty/whitespace/too-long), happy path, ClaudeProviderException → 502/503, generic exception → 500 no stack trace, SSE headers, SSE chunk JSON shape |
| `src/lab-observability-api.Tests/Controllers/AiBatchControllerTests.cs` | post-close | New — 7 tests: batch input validation and happy paths for submit/status/results endpoints |
| `src/lab-observability-api.Tests/Controllers/MiddlewareTests.cs` | post-close | New — 2 tests: x-correlation-id auto-set, correlation ID round-trip echo |
| `src/lab-observability-api.Tests/Controllers/HealthReadyMisconfiguredTests.cs` | post-close | New — 1 test: /health/ready → 503 when ApiKey is empty (uses EmptyApiKeyWebApplicationFactory) |
| `.claude/instructions/daily-workflow.md` | post-close | STEP 7: added test coverage requirement table (CEO/Architect/AI Engineer lenses); added `dotnet test` gate before STEP 8 |
| `.claude/commands/new-day.md` | post-close | Added Test Gate section in completion-checklist scaffold template; skip condition for pure-docs days |
| `.claude/commands/deploy.md` | post-close | Step 0 renamed to "Pre-deploy gates"; added 0a: `dotnet test` must pass before pillars audit; deployment-log.md template updated with dotnet test result line |
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
| `.gitignore` | cert-cleanup | Added .claude/cert-tags-today.txt (hook-generated, machine-local) |
| `docs/certifications/az-104/objectives.md` | cert-expansion | Expanded stub to full domain map with sub-objectives across all 5 domains |
