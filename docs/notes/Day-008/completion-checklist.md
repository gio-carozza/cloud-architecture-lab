# Day 8 — Completion Checklist

## Code

### Contracts (Phase A)
- [ ] `Models/Batch/BatchJobRequest.cs` — provider-agnostic batch request contract
- [ ] `Models/Batch/BatchJobStatus.cs` — status enum: `InProgress`, `Ended`, `Canceling`, `Expired`
- [ ] `Models/Batch/BatchJobResult.cs` — per-request result within a batch
- [ ] `Services/Batch/IBatchJobProvider.cs` — `SubmitAsync`, `GetStatusAsync`, `GetResultsAsync`

### Anthropic implementation (Phase B)
- [ ] `Services/Batch/ClaudeBatchApiClient.cs` — submit (`POST /v1/messages/batches`), status (`GET /{id}`), results (`GET /{id}/results` JSONL)
- [ ] `Services/Batch/ClaudeBatchJobProvider.cs` — implements `IBatchJobProvider`, wraps `ClaudeBatchApiClient`
- [ ] DI registration: `IBatchJobProvider` keyed as `"claude-batch"` in `Program.cs`
- [ ] No resilience pipeline on submit (duplicate batch on retry = billing error)

### API surface (Phase C)
- [ ] `Controllers/BatchController.cs` — `POST /api/ai/batch` (submit), `GET /api/ai/batch/{id}` (status), `GET /api/ai/batch/{id}/results` (retrieve)
- [ ] All endpoints return `ApiError` with `correlationId` on failure (no stack traces)
- [ ] `IChatModelProvider` / `ChatRequest` / `ChatResponse` — NOT modified

### Telemetry (Phase D)
- [ ] `GatewayTelemetry.BatchJobsSubmitted` counter added
- [ ] `GatewayTelemetry.BatchJobsCompleted` counter added
- [ ] `batch.job.result_count` histogram recorded on retrieval
- [ ] Savings log: `resultCount * avgInputTokens * 0.50 * pricePerToken` on retrieval

## Build & Local Verification (Phase E)

- [ ] `dotnet build` succeeds — 0 errors, 0 warnings
- [ ] `dotnet run` starts without errors
- [ ] `POST /api/ai/batch` with 3 requests returns `{ batchId, submittedAt, requestCount }`
- [ ] `GET /api/ai/batch/{id}` returns status; polling shows progression to `Ended`
- [ ] `GET /api/ai/batch/{id}/results` returns all 3 results
- [ ] Console logs show batch savings metric on retrieval

## ADR & Docs

- [ ] `docs/adr/ADR-010-introduce-batch-job-provider-seam.md` — Accepted
- [ ] `docs/standards/kql-cookbook.md` — Query 10 (batch cost vs. sync equivalent) added

## Infra & Config

- [ ] `Infra/Day-008/appsettings-template.md` confirms no new App Service settings (batch uses existing `Anthropic__ApiKey` and `Anthropic__BaseUrl`)

## Deploy & Azure Verification

- [ ] Deploy via `/deploy` slash command
- [ ] `GET /health` returns 200 from Azure
- [ ] `POST /api/ai/batch` returns 200 from Azure with valid batch ID
- [ ] `GET /api/ai/batch/{id}` status polling works from Azure
- [ ] App Insights: `batch.job.submitted` counter visible in `customMetrics` table

## Documentation

- [ ] `docs/architecture/day-008-batch-api-cost-controls.md` written
- [ ] `docs/notes/Day-008/architect-thinking.md` written
- [ ] `docs/notes/Day-008/posture-check.md` filled (end of day, before commit)
- [ ] Git commit: `feat(day-008): batch api cost controls`
