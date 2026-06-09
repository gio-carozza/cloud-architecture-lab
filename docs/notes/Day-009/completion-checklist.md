# Day 9 — Completion Checklist

## Contracts (Phase A)

- [x] `Models/AI/ChatChunk.cs` — streaming delta: `TextDelta` (string), `StopReason` (nullable string), `Usage` (nullable `ChatChunkUsage` — final chunk only: InputTokens, OutputTokens, CacheReadTokens, CacheCreationTokens)
- [x] `Services/AI/IChatModelProvider.cs` — `StreamAsync(ChatRequest request, CancellationToken ct)` returning `IAsyncEnumerable<ChatChunk>` added with default degrade implementation
- [x] `IChatModelProvider` / `ChatRequest` / `ChatResponse` — NOT broken (existing sync path and SendAsync unchanged)

## Anthropic Implementation (Phase B)

- [x] `Services/Claude/ClaudeApiClient.cs` — `StreamChatAsync`: POST with `"stream": true`, reads `text/event-stream`, parses `message_start` (input/cache tokens) / `content_block_delta/text_delta` (yield ChatChunk) / `message_delta` (yield final ChatChunk with stop reason + usage) / `message_stop` (yield break); CancellationToken propagated
- [x] `Services/AI/ClaudeChatModelProvider.cs` — `StreamAsync` override delegates to `ClaudeApiClient.StreamChatAsync`, records TTFT on first chunk, logs token counts on final chunk

## API Surface (Phase C)

- [x] `Controllers/AiController.cs` — `POST /api/ai/chat/stream` added
- [x] Endpoint sets `Content-Type: text/event-stream; charset=utf-8`, `Cache-Control: no-cache`, `X-Accel-Buffering: no` — confirmed in live test
- [x] Each chunk emitted as `data: <json>\n\n` with explicit `FlushAsync` — confirmed 12 distinct timestamps in incremental delivery test
- [x] On mid-stream exception: emits `event: error\ndata: <ApiError json>\n\n`; secondary write failure suppressed
- [x] `SendAsync` / sync path — NOT modified; `IChatModelProvider` extended, not replaced

## Telemetry (Phase D)

- [x] `GatewayTelemetry.StreamTtftMs` histogram (`ai.provider.stream.ttft_ms`) added — metric name aligns with `ai.provider.*` convention
- [x] Token counts from final usage chunk logged via `LogInformation` in `ClaudeChatModelProvider.StreamAsync`
- [ ] `GatewayTelemetry.StreamDurationMs` — NOT built (not in approved Day 9 summary; deferred)

## Build & Local Verification (Phase E)

- [x] `dotnet build` — 0 errors, 0 warnings
- [x] `dotnet run` starts without errors
- [x] `POST /api/ai/chat/stream` returns `200 text/event-stream; charset=utf-8` — confirmed
- [x] Tokens arrive incrementally — confirmed: 12 distinct timestamps, first token at 6ms, chunks ~370ms apart on a 138-token response
- [x] Final chunk carries `stopReason: "end_turn"` and `usage` with token counts — confirmed
- [x] First-token latency (`StreamTtftMs`) recorded and logged — confirmed via code path (TTFT stopwatch in ClaudeChatModelProvider.StreamAsync)
- [x] Prompt caching active on stream path — confirmed `cacheReadTokens: 1488` in live test
- [x] Existing `POST /api/ai/chat` (sync path) still works — confirmed: `{"provider":"anthropic","model":"claude-sonnet-4-6","response":"Hi!"}`

## ADR & Docs

- [x] `docs/adr/ADR-011-place-streaming-on-the-interactive-provider-seam.md` — Accepted; seam-test table present; explicitly contrasts with ADR-010 batch reasoning; "opposite verdict, identical test" framing

## Infra & Config

- [x] `Infra/Day-009/appsettings-template.md` confirms no new App Service settings

## KQL

- [x] Query 11 — TTFT p50/p95/p99 added to `docs/standards/kql-cookbook.md`
- [x] Query 12 — TTFT by model added to `docs/standards/kql-cookbook.md`

## Deploy & Azure Verification

- [x] Deploy via `/deploy` slash command (Kudu zip path) — Kudu zip deploy to app-ai-lab-api-dev-eastus-gio
- [x] `GET /health` returns 200 from Azure
- [x] `POST /api/ai/chat/stream` streams tokens from Azure — confirmed: 200 text/event-stream, X-Accel-Buffering:no, incremental delivery (18ms/219ms/250ms), cacheReadTokens=1488
- [x] `POST /api/ai/chat` sync path still returns 200 from Azure (regression check)
- [x] KQL Query 11 returns TTFT data — p50=1354ms, count=3

## Documentation

- [x] `docs/architecture/day-009-streaming-responses-on-interactive-path.md` written
- [x] `docs/notes/Day-009/architect-thinking.md` written
- [x] `docs/notes/Day-009/posture-check.md` filled (STEP 11 — before marking complete)
- [x] Git commit: `feat(day-009): streaming responses on interactive path`
