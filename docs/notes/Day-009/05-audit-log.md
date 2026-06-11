# Audit Log — Day 009

> Append-only. Each run adds a dated section. Never edit or delete prior runs.

---

## Run: retroactive audit — current code state (2026-06-08)

> NOTE: audits current file state, not Day 009 snapshot. Code is at Day 009 completion.
> Sources read: AiController.cs, ClaudeApiClient.cs (StreamChatAsync), ClaudeChatModelProvider.cs
> (StreamAsync + try/finally posture fix), IChatModelProvider.cs, ChatChunk.cs,
> GatewayTelemetry.cs, Infra/Day-009/appsettings-template.md, docs/notes/Day-009/07-files-changed.md

### Reliability
| Check | Status | Finding |
|---|---|---|
| R1 — HttpClient timeouts set | GREEN | `ClaudeApiClient` retains `InfiniteTimeSpan` — correct for streaming; `StreamChatAsync` uses `HttpCompletionOption.ResponseHeadersRead` with `CancellationToken` through every read; no new HttpClients added |
| R2 — No retry on non-idempotent calls | GREEN | Resilience pipeline on `ClaudeApiClient` has `ShouldHandle = false` disabling retry — unchanged; applies to streaming path since both sync and async paths share the same client |
| R3 — Circuit breaker not on batch/streaming | GREEN | Pre-stream connection errors throw `ClaudeProviderException` and trip the circuit breaker correctly; mid-stream `ReadLineAsync` errors are not exposed to `SendAsync` circuit breaker logic — no spurious trips from partial delivery |
| R4 — ValidateOnStart() for IOptions<T> | YELLOW | No new `IOptions<T>` bindings added; carry-forward from Day 006 |
| R5 — New exceptions caught by global handler | GREEN | Pre-stream `ClaudeProviderException` caught by controller's own `catch (Exception ex)` → SSE error frame; global handler not involved for streaming errors; all pre-header 400 validations return before SSE headers committed |
| R6 — CancellationToken threaded through async | GREEN | `HttpContext.RequestAborted` threaded through `StreamAsync` → `StreamChatAsync` → `SendAsync` and every `ReadLineAsync` call |

**Pillar: YELLOW**

### Security
| Check | Status | Finding |
|---|---|---|
| S1 — No secrets in source files | GREEN | No hardcoded API keys or secrets |
| S2 — Error paths return ApiError only | GREEN | Mid-stream SSE error frame: `ApiError(Code:"stream_error", Message:"An error occurred during streaming.", CorrelationId:...)` — `ex.Message` logged server-side only, never in the SSE frame |
| S3 — New endpoints validate input | GREEN | `AiController.StreamChat` validates null/whitespace and `MaxPromptLength` before `DisableBuffering()` and SSE headers — 400 can still be returned at that point |
| S4 — Paid LLM fanout has upper bound | GREEN | `MaxPromptLength` guard on streaming endpoint before any SSE header is written |
| S5 — CorrelationId in all error responses | GREEN | Pre-header 400 responses include `CorrelationId`; mid-stream SSE error frame includes `CorrelationId` |
| S6 — System prompt not echo-able | GREEN | `StreamChatAsync` calls `BuildAnthropicRequest(payload)` — system prompt sent to Anthropic, never in SSE stream; `ChatChunk` contains only `TextDelta`, `StopReason`, `Usage` |
| S7 — No internal details in response headers | GREEN | `Cache-Control: no-cache` and `X-Accel-Buffering: no` are operational — no implementation detail leaked |

**Pillar: GREEN**

### Cost Optimization
| Check | Status | Finding |
|---|---|---|
| C1 — Prompt caching active on interactive path | GREEN | `StreamChatAsync` calls `BuildAnthropicRequest(payload)` — same function that adds `cache_control: {"type":"ephemeral","ttl":"1h"}` on sync path; caching fully active on streaming |
| C2 — New endpoints have cost ceiling | GREEN | `MaxPromptLength` ceiling on streaming endpoint |
| C3 — No retry loop multiplying token spend | GREEN | No retry on streaming (same as R2) |
| C4 — Streaming token usage captured from final chunk | GREEN | `message_delta.usage.output_tokens` extracted in `StreamChatAsync`, bundled into `ChatChunkUsage`, yielded as terminal `ChatChunk`; `ClaudeChatModelProvider.StreamAsync` logs `InputTokens`, `OutputTokens`, `CacheReadTokens` on `chunk.Usage is not null` |
| C5 — Cost savings logged for cost-control features | GREEN | Cache read tokens parsed from `message_start.usage`, Activity-tagged (`llm.cache.read_tokens`), included in usage log line on streaming completion |
| C6 — Model ID is a valid current model | GREEN | `claude-sonnet-4-6` — valid |

**Pillar: GREEN**

### Performance Efficiency
| Check | Status | Finding |
|---|---|---|
| P1 — Streaming sets X-Accel-Buffering: no | GREEN | `Response.Headers.Append("X-Accel-Buffering", "no")` set before first write |
| P2 — Streaming sets Cache-Control: no-cache | GREEN | `Response.Headers.Append("Cache-Control", "no-cache")` set before first write |
| P3 — Streaming calls FlushAsync() after each chunk | GREEN | `await Response.Body.FlushAsync(HttpContext.RequestAborted)` after every `WriteAsync` in the chunk loop |
| P4 — No .Result or .Wait() blocking | GREEN | No blocking calls in any Day 009 file |
| P5 — TTFT instrumented for new streaming paths | GREEN | `GatewayTelemetry.StreamTtftMs.Record(ttftStopwatch.Elapsed.TotalMilliseconds, ...)` on `firstChunk` flag; `TtftMs` also in structured log |
| P6 — Streaming HttpClient has InfiniteTimeSpan | GREEN | `ClaudeApiClient` registered with `Timeout.InfiniteTimeSpan` — correct for the streaming path it now serves |

**Pillar: GREEN**

### Operational Excellence
| Check | Status | Finding |
|---|---|---|
| O1 — New paths log structured event with CorrelationId | GREEN | `AiController.StreamChat` logs `CorrelationId` + `PromptLength` at start and mid-stream error; `ClaudeChatModelProvider.StreamAsync` logs TTFT on first chunk and full usage on completion; `CorrelationId` enriched via Serilog on all events |
| O2 — New metric names follow ai.provider.* convention | GREEN | `ai.provider.stream.ttft_ms` follows convention; corrected from initial `ai.chat.stream.ttft_ms` during metric name fix pass |
| O3 — New env vars in appsettings-template.md | GREEN | `Infra/Day-009/appsettings-template.md` correctly states no new app settings; streaming reuses existing `Anthropic__*` config |
| O4 — files-changed.md has row for every file touched | GREEN | 44 rows covering all phases from scaffold through collab-lens |
| O5 — KQL cookbook updated for new signals | GREEN | Queries 11 (TTFT p50/p95/p99) and 12 (TTFT by model) added per files-changed.md |
| O6 — /health/ready reflects new required config | GREEN | No new required config; `/health/ready` unchanged |

**Pillar: GREEN**

### Responsible AI
| Check | Status | Finding |
|---|---|---|
| RA1 — Prompt/completion content NOT logged | GREEN | `AiController.StreamChat` logs `PromptLength` (count only); `ClaudeChatModelProvider.StreamAsync` logs model, TTFT, token counts — no prompt or completion text at any log level |
| RA2 — Error responses don't expose provider errors | GREEN | SSE error frame contains only `"An error occurred during streaming."` and `CorrelationId`; `LogError` server-side includes exception but never returns `ex.Message` to caller |
| RA3 — Content policy violations handled explicitly | YELLOW | Content-policy error arrives as pre-stream `ClaudeProviderException` (before SSE chunks yielded) but SSE headers already committed in controller before `StreamAsync` call — structurally impossible to return HTTP 422; falls to generic SSE error frame; carry-forward gap with new structural dimension on streaming path |
| RA4 — Every AI call has audit trail | GREEN | TTFT log on first chunk; full `InputTokens` + `OutputTokens` + `CacheReadTokens` log on `message_delta`; `CorrelationId` via Serilog enrichment on all events |
| RA5 — No PII in hardcoded system prompt | GREEN | System prompt config-bound; streaming payload uses `request.Prompt` from caller |
| RA6 — Streaming audit trail preserved | YELLOW | `try/finally` in `ClaudeChatModelProvider.StreamAsync` guarantees `LogDebug` (client disconnect) or `LogWarning` (unexpected end) when usage never arrives; accepted YELLOW per posture-check: no automated fault-injection test confirms audit trail survives under load |

**Pillar: YELLOW**

---

### Summary
| Pillar | Result | Key finding |
|---|---|---|
| Reliability | YELLOW | R4: ValidateOnStart() carry-forward from Day 006 |
| Security | GREEN | All checks pass |
| Cost | GREEN | Caching active on streaming path; TTFT and usage logged |
| Performance | GREEN | All streaming checks pass — X-Accel-Buffering, FlushAsync, TTFT histogram, InfiniteTimeSpan all in place |
| Ops Excellence | GREEN | All checks pass |
| Responsible AI | YELLOW | RA3: content policy → generic SSE error (structural gap); RA6: audit trail present but not fault-injection tested |

### RED items → fixes
None.

### YELLOW items → fixes
- **R4** ValidateOnStart() not wired: carry-forward from Day 006 — **accepted debt — tracked for Day 010+**
- **RA3** Content policy violation on streaming path → generic SSE error frame, not 422: SSE headers committed before `StreamAsync` call; 422 status structurally unreachable once `text/event-stream` is written; minimum fix = include `providerErrorCode` in the SSE error frame so callers can distinguish content policy from transient errors without changing HTTP status — **accepted debt — tracked for Day 010+**
- **RA6** Streaming audit trail not fault-injection tested: `try/finally` path is correct by inspection but unverified under client disconnect at token 1, token N, or on network drop — **accepted per posture-check — tracked for Day 010+**

---
