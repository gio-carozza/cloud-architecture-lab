# Error Handling Standard

**Phase:** 1 (active now)
**Applies to:** all error paths in `src/lab-observability-api/`

---

## Principles

1. **Never return stack traces to clients.** Correlation ID + safe message only.
2. **Classify at the boundary.** Provider errors are classified inside the provider; they never reach controllers as raw HTTP exceptions.
3. **Log everything, surface nothing.** Log the full exception with structured context; return only what the caller can act on.
4. **Correlation ID is mandatory on every error response.** It is the only link between the client's report and the server's log.

---

## Error response contract

All error responses use `ApiError` (`Contracts/ApiError.cs` — a 3-field record:
`Code`, `Message`, `CorrelationId`), serialized with `JsonNamingPolicy.CamelCase`:

```json
{
  "code": "claude_provider_error",
  "message": "The AI provider request failed.",
  "correlationId": "abc-123-def"
}
```

| Field | Required | Notes |
|---|---|---|
| `correlationId` | Always | From `X-Correlation-Id` header (set by `CorrelationIdMiddleware`) |
| `message` | Always | Human-readable, client-safe. No internal details. |
| `code` | Always | Machine-readable error code (see taxonomy below) — lowercase `snake_case`, not an enum |

There is no `details` field — the contract is intentionally 3 fields only.

---

## Error code taxonomy

Codes actually emitted today (`Code:` literals in `Controllers/*.cs` and `Program.cs`):

| Code | HTTP status | Meaning | Retry? |
|---|---|---|---|
| `invalid_request` | 400 | Empty/whitespace prompt, null batch element, empty batch | No — fix the request |
| `prompt_too_long` | 400 | Prompt exceeds `AnthropicOptions.MaxPromptLength` | No |
| `batch_size_exceeded` | 400 | Batch request count exceeds `AnthropicOptions.MaxBatchSize` | No |
| `stream_error` | n/a (SSE `event: error` frame, not an HTTP status — stream already returned 200) | Mid-stream exception after the first chunk was sent | No — inform user |
| `claude_provider_error` | 502 / 503 / 504 (see classification below) | `ClaudeProviderException` reached the global handler | Depends on `IsTransient` |
| `internal_error` | 500 | Unclassified exception reached the global handler | No — investigate |

This taxonomy is intentionally small. There is no dedicated code for auth failure,
rate limiting, or timeout today — they all collapse into `claude_provider_error` with
a status code chosen by the switch in the classification rules below. Add a new code
only when a caller needs to branch on it differently than `claude_provider_error`.

---

## Classification rules per layer

### `ClaudeApiClient` (transport layer)

Throws a single exception type, `ClaudeProviderException` (`Services/Claude/ClaudeProviderException.cs`),
with `ProviderStatusCode`, `ProviderErrorCode`, and `IsTransient` carried as properties —
there is no exception subclass hierarchy.

| Condition | `ProviderStatusCode` | `IsTransient` |
|---|---|---|
| Anthropic 429, 408, 502, 503, 504 | the actual status returned | `true` |
| Anthropic non-success, any other status | the actual status returned | `false` |
| `HttpRequestException` (network failure) | 503 | `true` |
| Per-attempt or total timeout (`TaskCanceledException`) | 408 | `true` |

Never let raw `HttpRequestException` or `HttpResponseMessage` escape `ClaudeApiClient`.

### `ClaudeChatModelProvider` (orchestration layer)

Does not catch `ClaudeProviderException` — lets it propagate to the global handler.
Never catches `OperationCanceledException` — let it propagate (client disconnect is
not an error).

### Global exception pipeline (`Program.cs`, inline `app.Use(...)`)

Not a separate middleware class. Catches `ClaudeProviderException` first, then a
generic `Exception` fallback:

```csharp
catch (ClaudeProviderException ex)
{
    // logs Provider/ProviderStatusCode/ProviderErrorCode/IsTransient at Warning
    StatusCode = ex.ProviderStatusCode switch
    {
        HttpStatusCode.TooManyRequests => 503,
        HttpStatusCode.RequestTimeout  => 504,
        _ when ex.IsTransient         => 503,
        _                              => 502
    };
    // ApiError(Code: "claude_provider_error", ...)
}
catch (Exception ex)
{
    // logs at Error; ApiError(Code: "internal_error", ...), status 500
}
```

---

## Retry policy

| Path | HTTP client | Retry? | Why |
|---|---|---|---|
| `POST /api/ai/chat` | `ClaudeApiClient` via `AddStandardResilienceHandler` | No — retry stage present but disabled (`MaxRetryAttempts=1`, `ShouldHandle=false`) per ADR-006 | POST to external AI — idempotency not guaranteed; cost risk |
| `POST /api/ai/chat/stream` | Same client as above | No | Same reasons; mid-stream retry not meaningful |
| `POST /api/ai/batch` (submit) | `ClaudeBatchApiClient` — **no resilience pipeline at all** | No | Per ADR-010: retrying a submit may duplicate a batch job; orchestration-layer retry is explicitly deferred, not yet built |
| `GET /api/ai/batch/{id}` (poll/results) | Same client as above | No | Same client, same no-pipeline decision |

The interactive (`ClaudeApiClient`) pipeline has a circuit breaker even though retry
is disabled: opens when 20% of requests fail within a 120s window (minimum throughput
5), breaks for 15s. The batch client has neither retry nor circuit breaker — see
`Program.cs` HTTP client registration. Per-item retry of failed batch requests is
named as deferred orchestration work in ADR-010, not implemented today.

---

## Logging requirements per error type

| Severity | When | Required structured fields |
|---|---|---|
| `LogError` | Unhandled exception (`internal_error`, 500) | `CorrelationId`, full exception via `LogError(ex, ...)` |
| `LogWarning` | `ClaudeProviderException` reaches the global handler | `CorrelationId`, `Provider`, `ProviderStatusCode`, `ProviderErrorCode`, `IsTransient` |
| `LogInformation` | First streamed token (TTFT) | `Model`, `TtftMs` |
| `LogDebug` | Client disconnect mid-stream (`OperationCanceledException` + `RequestAborted`) | No status code is set — the response is simply abandoned; there is no `499` convention in this codebase |

There is no `429`/circuit-open/timeout-specific log row — those all fall under the
`ClaudeProviderException` → `LogWarning` row above, distinguished by `ProviderStatusCode`
and `IsTransient` rather than separate log statements.

Never log prompt content or API keys at any level in production. Never log `Exception.ToString()` as a log message field — use `LogError(ex, "message")` so Serilog serializes the exception properly.

---

## Streaming-specific error handling

Mid-stream errors (after the first chunk has been sent) cannot change the HTTP status code — it's already 200. Use SSE error events:

```text
event: error
data: {"code":"stream_error","message":"An error occurred during streaming.","correlationId":"abc-123"}
```

The client must handle `event: error` frames. Document this in the API contract (Swagger annotation on the stream endpoint).

Always flush the error event before closing the response. Always cancel the upstream Anthropic HTTP read via `CancellationToken` when the client disconnects.
