# Day 8 — Batch API Cost Controls

## Change Summary

Day 8 adds an async batch processing path to the AI gateway, implemented as a
parallel provider abstraction (`IBatchChatModelProvider`) that is explicitly
independent of the interactive `IChatModelProvider` seam established in Day 5.
The batch path proxies the Anthropic Message Batches API: callers submit a list
of `ChatRequest` objects, receive a batch ID, poll for status, and retrieve results.
Every batch token costs 50% of the synchronous rate — unconditionally, with no
cache warm-up dependency. Three new telemetry counters make savings visible from
the first request.

## Architecture Delta from Day 7

### Added

| Component | Location | Role |
|---|---|---|
| `IBatchChatModelProvider` | `Services/AI/` | Sibling seam — submit/poll/retrieve contract |
| `ClaudeBatchChatModelProvider` | `Services/AI/` | Implements the seam; delegates to client |
| `ClaudeBatchApiClient` | `Services/Claude/` | HTTP transport to Anthropic Batch API; no resilience pipeline on submit |
| `AiBatchController` | `Controllers/` | `POST /api/ai/batch`, `GET /api/ai/batch/{id}`, `GET /api/ai/batch/{id}/results` |
| `BatchJob` | `Models/AI/` | Submit return type (id, status, timestamps) |
| `BatchJobStatus` | `Models/AI/` | Poll return type (status + per-bucket counts) |
| `BatchResult` | `Models/AI/` | Per-request result (customId, isSuccess, response?) |
| `BatchProcessingStatus` | `Models/AI/` | Enum: `InProgress`, `Canceling`, `Ended` |
| 3 telemetry instruments | `Telemetry/GatewayTelemetry.cs` | `BatchJobsSubmitted`, `BatchJobsCompleted`, `BatchResultCount` |

### Not Modified

- `IChatModelProvider` — interactive seam unchanged
- `ChatRequest` / `ChatResponse` — reused as batch element type, not modified
- `ClaudeChatModelProvider` — interactive orchestration unchanged
- `ClaudeApiClient` — interactive HTTP client unchanged
- All Day 6/7 resilience and observability configuration

## Sequence Flow

```
Caller                  Gateway                   Anthropic Batch API
  |                        |                               |
  |-- POST /api/ai/batch ->|                               |
  |  [ChatRequest[]]       |-- POST /v1/messages/batches ->|
  |                        |<-- { id, processing_status } -|
  |<-- { batchId, ... } ---|                               |
  |                        |                               |
  |  (wait minutes–hours)  |                               |
  |                        |                               |
  |-- GET /api/ai/batch/id |                               |
  |  [poll]                |-- GET /v1/messages/batches/id>|
  |                        |<-- { processing_status: ended}|
  |<-- { status: Ended } --|                               |
  |                        |                               |
  |-- GET /{id}/results -> |                               |
  |                        |-- GET /v1/messages/batches/   |
  |                        |   {id}/results (JSONL) -----> |
  |                        |<-- stream of result objects --|
  |<-- [BatchResult[]] ----|                               |
  |                        | log: EstimatedSavingsUsd      |
```

## Telemetry Delta

| Instrument | Name | Recorded when |
|---|---|---|
| Counter | `ai.provider.batch.submitted` | After successful submit |
| Counter | `ai.provider.batch.completed` | After successful results retrieval |
| Histogram | `ai.provider.batch.result_count` | After results retrieval (value = count) |
| Log field | `EstimatedSavingsUsd` | On retrieval: `resultCount × 500 × 0.5 × (3.0/1M)` |

Activity spans: `batch.submit`, `batch.poll`, `batch.retrieve` (tagged with
`llm.provider`, `batch.job_id`, `batch.request_count`).

**Alerting note:** batch spans have latency in the minutes-to-hours range. The
`alert-ai-gateway-5xx-rate` rule and interactive latency SLOs must not be
applied to batch spans. `InProgress` polling responses are HTTP 200 successes.

## Related

- `docs/adr/ADR-010-introduce-parallel-batch-provider-abstraction.md`
- `docs/notes/Day-008/01-summary.md`
- `docs/notes/Day-008/architect-thinking.md`
- `docs/standards/kql-cookbook.md` — Query 10
- `Infra/Day-008/appsettings-template.md` — no new settings
