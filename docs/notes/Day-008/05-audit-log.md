# Audit Log — Day 008

> Append-only. Each run adds a dated section. Never edit or delete prior runs.

---

## Run: retroactive audit — current code state (2026-06-08)

> NOTE: audits current file state, not Day 008 snapshot. Code has progressed through Day 009.
> Sources read: AiBatchController.cs, ClaudeBatchApiClient.cs, ClaudeBatchChatModelProvider.cs,
> IBatchChatModelProvider.cs, AnthropicOptions.cs, GatewayTelemetry.cs, Program.cs,
> Infra/Day-008/appsettings-template.md, docs/notes/Day-008/07-files-changed.md

### Reliability
| Check | Status | Finding |
|---|---|---|
| R1 — HttpClient timeouts set | GREEN | ClaudeBatchApiClient registered with `Timeout = TimeSpan.FromSeconds(30)`; MaxBatchSize=100 keeps result payload bounded within that window |
| R2 — No retry on non-idempotent calls | GREEN | `SubmitAsync` uses `PostAsync` with no resilience pipeline — intentional, documented in code comment ("NO retry — a network error on submit may have succeeded server-side") |
| R3 — Circuit breaker not on batch/streaming | GREEN | No `AddStandardResilienceHandler` on `ClaudeBatchApiClient`; circuit breaker applies to interactive `ClaudeApiClient` only |
| R4 — ValidateOnStart() for IOptions<T> | YELLOW | `MaxBatchSize` added to `AnthropicOptions` without `ValidateOnStart` — carry-forward from Day 006 |
| R5 — New exceptions caught by global handler | GREEN | `ClaudeProviderException` thrown with codes `batch_submit_failed`, `batch_status_failed`, `batch_results_failed`; all caught by existing global `ClaudeProviderException` handler in Program.cs |
| R6 — CancellationToken threaded through async | GREEN | All three `ClaudeBatchApiClient` methods accept and thread `CancellationToken`; `AiBatchController` passes `CancellationToken` through all three actions |

**Pillar: YELLOW**

### Security
| Check | Status | Finding |
|---|---|---|
| S1 — No secrets in source files | GREEN | No hardcoded API keys or secrets; `ApiKey` bound from configuration |
| S2 — Error paths return ApiError only | GREEN | `ClaudeBatchApiClient` error messages include only HTTP status codes; global handler uses hardcoded generic strings — `ex.Message` never reaches caller |
| S3 — New endpoints validate input | RED | `AiBatchController.Submit` validates null/empty/whitespace on each prompt but **no `MaxPromptLength` check** — batch path was created Day 008 without the guard added to `AiController` in Day 006 retroactive fix |
| S4 — Paid LLM fanout has upper bound | RED | `MaxBatchSize` caps request count but individual prompt length uncapped — 100 × uncapped prompt = 100 × unbounded token spend; same root as S3 |
| S5 — CorrelationId in all error responses | GREEN | All `ApiError` usages in `AiBatchController` pass `HttpContext.GetCorrelationId()` |
| S6 — System prompt not echo-able | GREEN | Batch payload constructs messages from `r.Prompt` only; no system prompt included, none returned |
| S7 — No internal details in response headers | GREEN | No new response headers beyond `x-correlation-id` |

**Pillar: ~~RED~~ → GREEN** (S3/S4 fixed — see RED items below)

### Cost Optimization
| Check | Status | Finding |
|---|---|---|
| C1 — Prompt caching active on interactive path | N/A | Batch path does not use `BuildAnthropicRequest`; caching is interactive path scope only |
| C2 — New endpoints have cost ceiling | RED | Same root as S3/S4 — `MaxBatchSize` count ceiling present but per-prompt length ceiling absent on batch path |
| C3 — No retry loop multiplying token spend | GREEN | No resilience pipeline on batch client; no retry on `SubmitAsync` |
| C4 — Streaming token usage from final chunk | N/A | Day 009 scope |
| C5 — Cost savings logged for cost-control features | GREEN | `GetResultsAsync` logs `EstimatedSavingsUsd` = `resultCount × avgInputTokens × 0.5 × (price/1M)` |
| C6 — Model ID is a valid current model | GREEN | `AnthropicOptions.Model` defaults to `claude-sonnet-4-6` — valid |

**Pillar: ~~RED~~ → GREEN** (C2 fixed — same fix as S3/S4)

### Performance Efficiency
| Check | Status | Finding |
|---|---|---|
| P1 — Streaming sets X-Accel-Buffering: no | N/A | Day 009 scope |
| P2 — Streaming sets Cache-Control: no-cache | N/A | Day 009 scope |
| P3 — Streaming calls FlushAsync() after each chunk | N/A | Day 009 scope |
| P4 — No .Result or .Wait() blocking | GREEN | No blocking calls in any Day 008 file |
| P5 — TTFT instrumented for new streaming paths | N/A | Day 009 scope |
| P6 — Streaming HttpClient has InfiniteTimeSpan | N/A | Batch client uses 30s — correct; streaming client is Day 009 scope |

**Pillar: GREEN**

### Operational Excellence
| Check | Status | Finding |
|---|---|---|
| O1 — New paths log structured event with CorrelationId | GREEN | All three batch endpoints log `CorrelationId` + `BatchJobId`; `CorrelationId` flows via Serilog enrichment |
| O2 — New metric names follow ai.provider.* convention | GREEN | `ai.provider.batch.submitted`, `ai.provider.batch.completed`, `ai.provider.batch.result_count` — all follow convention |
| O3 — New env vars in appsettings-template.md | GREEN | `Infra/Day-008/appsettings-template.md` correctly states "No new app settings this day" — `MaxBatchSize` has a default; no Azure env var required |
| O4 — files-changed.md has row for every file touched | GREEN | 43 rows covering all phases from populate through cert-update |
| O5 — KQL cookbook updated for new signals | GREEN | Query 10 (batch activity + cost vs sync equivalent) added per files-changed.md |
| O6 — /health/ready reflects new required config | GREEN | `MaxBatchSize` has a default value; `/health/ready` unchanged |

**Pillar: GREEN**

### Responsible AI
| Check | Status | Finding |
|---|---|---|
| RA1 — Prompt/completion content NOT logged | GREEN | Batch logs `RequestCount`, `BatchJobId`, `ResultCount`, `EstimatedSavingsUsd` only; no prompt or completion text anywhere |
| RA2 — Error responses don't expose provider errors | YELLOW | `ParseBatchResult` stores `error.message` from Anthropic per-request JSONL results in `BatchResult.ErrorMessage`; this is returned to callers via `GetResults → Ok(results)`; raw provider per-request error text surfaced |
| RA3 — Content policy violations handled explicitly | YELLOW | Batch requests with `result.type = "errored"` propagate Anthropic error codes through `BatchResult` to caller — no explicit content_policy mapping; carry-forward from Day 006 + new batch surface |
| RA4 — Every AI call has audit trail | YELLOW | Submit logs `RequestCount`; results logs `ResultCount` and `BatchJobId`; `CorrelationId` flows via enrichment ✓; per-request token counts not available in basic Anthropic batch JSONL results; carry-forward gap |
| RA5 — No PII in hardcoded system prompt | GREEN | Batch payload uses `r.Prompt` from caller; no hardcoded system prompt |
| RA6 — Streaming audit trail preserved | N/A | Day 009 scope |

**Pillar: YELLOW**

---

### Summary
| Pillar | Result | Key finding |
|---|---|---|
| Reliability | YELLOW | R4: ValidateOnStart() carry-forward from Day 006 |
| Security | ~~RED~~ → GREEN | S3/S4: batch endpoint missing per-prompt MaxPromptLength guard — fixed |
| Cost | ~~RED~~ → GREEN | C2: same root as S3/S4 — fixed |
| Performance | GREEN | All checks pass or N/A |
| Ops Excellence | GREEN | All checks pass |
| Responsible AI | YELLOW | RA2: raw provider per-request error text in BatchResult; RA3: content policy carry-forward; RA4: token counts unavailable in batch JSONL |

### RED items → fixes
- **S3/S4/C2** `AiBatchController.Submit` had no `MaxPromptLength` check on individual prompts — `MaxBatchSize` capped request count but not per-prompt size, leaving cost ceiling incomplete. Added guard after the whitespace check: `requests.Any(r => r.Prompt.Length > _options.MaxPromptLength)` returns 400 `prompt_too_long`. Build clean. → **GREEN**

### YELLOW items → fixes
- **R4** ValidateOnStart() not wired: carry-forward from Day 006 — **accepted debt — tracked for Day 010+**
- **RA2** `BatchResult.ErrorMessage` exposes raw Anthropic per-request error text: minimum fix = normalize per-request errors to a gateway-controlled enum (`"content_filtered"`, `"provider_error"`, `"invalid_request"`) before returning in results — **accepted debt — tracked for Day 010+**
- **RA3** Content policy 400 → no explicit mapping on batch path: carry-forward from Day 006 — **accepted debt — tracked for Day 010+**
- **RA4** Per-request token counts not in batch JSONL: Anthropic does not include usage in batch result JSONL (unlike streaming message_delta); cannot fix without a separate usage API call per request — **accepted as structural gap — tracked for Day 010+**

---
