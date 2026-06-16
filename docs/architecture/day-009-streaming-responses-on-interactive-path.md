# Day 009 — Streaming Responses on the Interactive Path

## Change Summary

Adds SSE streaming to the interactive chat path. The existing synchronous `POST /api/ai/chat` is unchanged. A new `POST /api/ai/chat/stream` endpoint returns `text/event-stream` and yields `ChatChunk` events as the model generates tokens.

## Architecture Delta

### Before (Day 8 state)

```text
Client → POST /api/ai/chat → AiController → IChatModelProvider.ChatAsync()
                                                      ↓
                                           ClaudeApiClient (POST, waits for full body)
                                                      ↓
                              ← 200 ChatResponse (full response, all tokens)
```

### After (Day 9)

```text
Client → POST /api/ai/chat/stream → AiController → IChatModelProvider.StreamAsync()
                                                             ↓
                                              ClaudeApiClient.StreamAsync()
                                              (POST stream:true, reads SSE line-by-line)
                                                             ↓
                              ← 200 text/event-stream
                                  data: {"textDelta":"Hello",...}   ← first token ~200ms
                                  data: {"textDelta":", world",...}
                                  ...
                                  data: {"stopReason":"end_turn","usage":{...}}

Client → POST /api/ai/chat (sync, unchanged)
```

## New Components

| Component | Location | Role |
|---|---|---|
| `ChatChunk` | `Models/AI/ChatChunk.cs` | Streaming delta: text fragment, stop reason (final), usage (final) |
| `StreamAsync` | `Services/AI/IChatModelProvider.cs` | Extends the existing seam — same operation, incremental delivery |
| `ClaudeApiClient.StreamAsync` | `Services/Claude/ClaudeApiClient.cs` | POSTs with `stream:true`; parses SSE event stream |
| `ClaudeChatModelProvider.StreamAsync` | `Services/AI/ClaudeChatModelProvider.cs` | Delegates to client; records first-token latency |
| `/api/ai/chat/stream` | `Controllers/AiController.cs` | New endpoint; sets SSE headers; flushes each chunk |
| `StreamFirstTokenMs` | `Telemetry/GatewayTelemetry.cs` | Histogram: ms to first token |
| `StreamDurationMs` | `Telemetry/GatewayTelemetry.cs` | Histogram: total stream duration |

## Seam Decision

`StreamAsync` extends `IChatModelProvider` (not a new sibling seam). Streaming is the same operation as synchronous chat with a different transport — same request in, same content out, different delivery timing. This is explicitly different from the batch seam (ADR-010), which has a genuinely distinct lifecycle (submit/poll/retrieve). See `architect-thinking.md` §1 and ADR-011.

## SSE Wire Format

Anthropic SSE events relevant to this implementation:

```text
event: message_start      → extract input_tokens from usage
event: content_block_delta → extract text_delta → yield ChatChunk(TextDelta)
event: message_delta       → extract stop_reason + output_tokens → yield final ChatChunk
event: message_stop        → end stream
```

## Azure App Service Notes

Requires `X-Accel-Buffering: no` header on the response to disable nginx proxy buffering. Without it, chunks accumulate and the client receives them all at once instead of incrementally. See `architect-thinking.md` §2.

## KQL

No new KQL queries this day. Streaming metrics (`ai.provider.stream.first_token_ms`, `ai.provider.stream.duration_ms`) are available in the `customMetrics` table via the existing OpenTelemetry pipeline.
