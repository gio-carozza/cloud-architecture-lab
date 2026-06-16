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

All error responses use `ApiError` (from `Contracts/ApiError.cs`):

```json
{
  "correlationId": "abc-123-def",
  "error": "The request could not be processed.",
  "code": "PROVIDER_UNAVAILABLE"
}
```

| Field | Required | Notes |
|---|---|---|
| `correlationId` | Always | From `X-Correlation-Id` header (set by `CorrelationIdMiddleware`) |
| `error` | Always | Human-readable, client-safe. No internal details. |
| `code` | Always | Machine-readable error code (see taxonomy below) |
| `details` | Never in prod | May be added in local dev via `ASPNETCORE_ENVIRONMENT=Development` |

---

## Error code taxonomy

| Code | HTTP status | Meaning | Retry? |
|---|---|---|---|
| `VALIDATION_FAILED` | 400 | Request failed model validation | No — fix the request |
| `PROMPT_REQUIRED` | 400 | Prompt field is null or empty | No |
| `PROVIDER_UNAUTHORIZED` | 401 | API key missing or invalid | No — ops issue |
| `PROVIDER_RATE_LIMITED` | 429 | Provider returned 429 | Yes — after backoff |
| `PROVIDER_UNAVAILABLE` | 503 | Provider unreachable or circuit open | Yes — after delay |
| `PROVIDER_TIMEOUT` | 504 | Provider did not respond within timeout | Yes — with backoff |
| `STREAM_INTERRUPTED` | 500 | SSE stream ended abnormally mid-response | No — inform user |
| `INTERNAL_ERROR` | 500 | Unclassified exception | No — investigate |

---

## Classification rules per layer

### `ClaudeApiClient` (transport layer)

| Anthropic HTTP status | Maps to |
|---|---|
| 401 | Throw `ProviderAuthException` — do not retry |
| 429 | Throw `ProviderRateLimitException` — Polly retries with backoff |
| 529 (overloaded) | Treat as 429 |
| 400 with body | Throw `ProviderBadRequestException` — log body, do not surface it |
| 500/503 | Throw `ProviderUnavailableException` — Polly circuit breaker |
| Timeout | `TaskCanceledException` propagates — Polly timeout policy catches it |

Never let raw `HttpRequestException` or `HttpResponseMessage` escape `ClaudeApiClient`.

### `ClaudeChatModelProvider` (orchestration layer)

Catches provider-specific exceptions, maps to `ChatProviderException` with the error code attached. Never catches `OperationCanceledException` — let it propagate (client disconnect is not an error).

### `ExceptionHandlingMiddleware` (global)

Catches all unhandled exceptions. Maps:

```csharp
ChatProviderException  → ApiError with the embedded code
OperationCanceledException → 499 (client closed request) — log at Information, not Error
ValidationException    → 400 VALIDATION_FAILED
Everything else        → 500 INTERNAL_ERROR — log at Error with full exception
```

---

## Retry policy

| Path | Retry? | Policy |
|---|---|---|
| `POST /api/ai/chat` | No auto-retry | POST to external AI — idempotency not guaranteed; cost risk |
| `POST /api/ai/chat/stream` | No auto-retry | Same reasons; mid-stream retry not meaningful |
| `POST /api/ai/batch` (submit) | Yes — 2 retries | Submit is safe to retry before Anthropic accepts the job |
| `GET /api/ai/batch/{id}` (poll) | Yes — 3 retries | Read operation, always idempotent |

Circuit breaker trips at 5 consecutive failures within 30 seconds. Recovery probe after 15 seconds. These settings live in `Program.cs` resilience pipeline registration — do not duplicate them in individual clients.

---

## Logging requirements per error type

| Severity | When | Required structured fields |
|---|---|---|
| `LogError` | Unhandled exception, 5xx response | `CorrelationId`, `ExceptionType`, `Message`, `Path` |
| `LogWarning` | 429, circuit open, timeout | `CorrelationId`, `StatusCode`, `RetryAttempt` |
| `LogInformation` | 4xx (validation, auth) | `CorrelationId`, `StatusCode`, `ErrorCode` |
| `LogInformation` | Client disconnect (499) | `CorrelationId`, `Path` |

Never log prompt content or API keys at any level in production. Never log `Exception.ToString()` as a log message field — use `LogError(ex, "message")` so Serilog serializes the exception properly.

---

## Streaming-specific error handling

Mid-stream errors (after the first chunk has been sent) cannot change the HTTP status code — it's already 200. Use SSE error events:

```text
event: error
data: {"correlationId":"abc-123","code":"STREAM_INTERRUPTED","error":"The stream ended unexpectedly."}
```

The client must handle `event: error` frames. Document this in the API contract (Swagger annotation on the stream endpoint).

Always flush the error event before closing the response. Always cancel the upstream Anthropic HTTP read via `CancellationToken` when the client disconnects.
