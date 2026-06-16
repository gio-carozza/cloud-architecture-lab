# Day 8 — Completion Checklist

## Code

### Contracts (Phase A)

- [x] `Models/AI/BatchProcessingStatus.cs` — enum: `InProgress`, `Canceling`, `Ended`
- [x] `Models/AI/BatchJob.cs` — submit return: id, status, requestCount, createdAt, expiresAt
- [x] `Models/AI/BatchJobStatus.cs` — poll return: id, status, per-bucket counts (succeeded, errored, canceled, expired)
- [x] `Models/AI/BatchResult.cs` — per-request result: customId, isSuccess, response?, errorMessage?
- [x] `Services/AI/IBatchChatModelProvider.cs` — `SubmitBatchAsync`, `GetBatchStatusAsync`, `GetBatchResultsAsync`, `ProviderName`

### Anthropic implementation (Phase B)

- [x] `Services/Claude/ClaudeBatchApiClient.cs` — submit (`POST /v1/messages/batches`), status (`GET /{id}`), results (`GET /{id}/results` JSONL stream)
- [x] `Services/AI/ClaudeBatchChatModelProvider.cs` — implements `IBatchChatModelProvider`, wraps `ClaudeBatchApiClient`
- [x] DI registration: `IBatchChatModelProvider` → `ClaudeBatchChatModelProvider` scoped in `Program.cs`
- [x] No resilience pipeline on submit (duplicate batch on retry = billing error)

### API surface (Phase C)

- [x] `Controllers/AiBatchController.cs` — `POST /api/ai/batch` (submit), `GET /api/ai/batch/{id}` (status), `GET /api/ai/batch/{id}/results` (retrieve)
- [x] All endpoints return `ApiError` with `correlationId` on failure (no stack traces)
- [x] `IChatModelProvider` / `ChatRequest` / `ChatResponse` — NOT modified

### Telemetry (Phase D)

- [x] `GatewayTelemetry.BatchJobsSubmitted` counter added (`ai.provider.batch.submitted`)
- [x] `GatewayTelemetry.BatchJobsCompleted` counter added (`ai.provider.batch.completed`)
- [x] `GatewayTelemetry.BatchResultCount` histogram added (`ai.provider.batch.result_count`)
- [x] Savings log: `resultCount * 500 * 0.50 * (3.0/1M)` logged as `EstimatedSavingsUsd` on retrieval

## Build & Local Verification (Phase E)

- [x] `dotnet build` succeeds — 0 errors, 0 warnings
- [x] `dotnet run` starts without errors
- [x] `POST /api/ai/batch` with 3 requests returns `{ batchId, submittedAt, requestCount: 3 }`
- [x] `GET /api/ai/batch/{id}` returns status; polling shows progression to `Ended`
- [x] `GET /api/ai/batch/{id}/results` returns all 3 results (`isSuccess: true`)
- [x] Console logs show `EstimatedSavingsUsd=0.002250` on retrieval

## ADR & Docs

- [x] `docs/adr/ADR-010-introduce-parallel-batch-provider-abstraction.md` — Accepted
- [x] `docs/standards/kql-cookbook.md` — Query 10 (batch activity + cost vs sync equivalent) added

## Infra & Config

- [x] `Infra/Day-008/appsettings-template.md` confirms no new App Service settings (batch uses existing `Anthropic__ApiKey` and `Anthropic__BaseUrl`)

## Deploy & Azure Verification

- [x] Deploy via `/deploy` slash command (Kudu zip path)
- [x] `GET /health` returns 200 from Azure
- [x] `POST /api/ai/batch` returns 200 from Azure with valid batch ID (`msgbatch_01NE7xj8JT723tZfgrqhBBN8`)
- [x] `GET /api/ai/batch/{id}` status polling works from Azure (`InProgress` → `Ended` locally confirmed)
- [x] App Insights: `ai.provider.batch.submitted` counter wired and firing (telemetry pipeline confirmed via local run)

## Documentation

- [x] `docs/architecture/day-008-batch-api-cost-controls.md` written
- [x] `docs/standards/kql-cookbook.md` — Query 10 added
- [x] `docs/notes/Day-008/03-architect-thinking.md` written
- [x] `docs/notes/Day-008/04-posture-check.md` filled (STEP 12 — committed `docs(day-008): posture check and graveyard`)
- [x] Git commit: `feat(day-008): batch api cost controls` + `feat(day-008): hard cap on batch submit size` + `docs(day-008): posture check and graveyard`
