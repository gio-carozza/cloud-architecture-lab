# Day 008 — Files Changed

| File | Step | Change |
|---|---|---|
| `docs/adr/ADR-010-introduce-parallel-batch-provider-abstraction.md` | populate | Status set to Accepted; resolved ⟨confirm⟩ placeholder |
| `docs/notes/Day-008/summary.md` | populate | Full 13-section summary written |
| `docs/notes/Day-008/completion-checklist.md` | populate | Phase A–E items defined; docs pass: all verified items marked [x] |
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
| `docs/notes/Day-008/architect-thinking.md` | docs pass | Written — seam decision, ADR-009 symmetry, no-retry reasoning, stateless principle |
| `docs/notes/_index.md` | docs pass | Day 008 status → Complete |
| `CLAUDE.md` | docs pass | Days 1–8 complete; cost controls item 3 updated with ADR-010 |
| `docs/notes/Day-008/files-changed.md` | docs pass | This file — full audit log upsert |
| `src/lab-observability-api/Options/AnthropicOptions.cs` | build | Added MaxBatchSize (default 100) |
| `src/lab-observability-api/Controllers/AiBatchController.cs` | build | Added budget cap check: rejects batches over MaxBatchSize with batch_size_exceeded error |
| `docs/notes/Day-008/posture-check.md` | docs pass | Filled — four posture questions answered with honest graveyard-ready admissions |
| `docs/standards/_principles.md` | docs pass | Appended 4 Day 8 graveyard rows: contract divergence, accepted ADR with placeholder, missing blast-radius cap, build lock |
| `docs/notes/Day-008/summary.md` | docs pass | Corrected ADR filename; added files-changed to artifacts list |
| `.claude/instructions/daily-workflow.md` | docs pass | Added auto-print handoff section and step transition prompts |
| `docs/notes/Day-005/completion-checklist.md` | alignment | Removed template code-block wrapper; updated two missing file refs to their promoted locations |
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
