# Audit Log — Day 006

> Append-only. Each run adds a dated section. Never edit or delete prior runs.

---

## Run: retroactive audit — current code state (2026-06-08)

> NOTE: audits current file state, not Day 006 snapshot. Code has progressed through Day 009.
> Sources read: Program.cs, CorrelationIdMiddleware.cs, ClaudeChatModelProvider.cs,
> AiController.cs, AnthropicOptions.cs, GatewayTelemetry.cs, ClaudeProviderException.cs,
> HttpContextExtensions.cs, ApiError.cs, Infra/Day-006/appsettings-template.md, kql-cookbook.md

### Reliability
| Check | Status | Finding |
|---|---|---|
| R1 — HttpClient timeouts set | GREEN | ClaudeApiClient: InfiniteTimeSpan (correct — streaming + CancellationToken); ClaudeBatchApiClient: 30s |
| R2 — No retry on non-idempotent calls | GREEN | ShouldHandle = false disables retry on interactive path; batch client has no resilience handler |
| R3 — Circuit breaker not on batch/streaming | GREEN | AddStandardResilienceHandler on ClaudeApiClient only; ClaudeBatchApiClient excluded |
| R4 — ValidateOnStart() for IOptions<T> | YELLOW | Configure<AnthropicOptions> not chained with .ValidateDataAnnotations().ValidateOnStart(); /health/ready is partial substitute but fails at first request, not startup |
| R5 — New exceptions caught by global handler | GREEN | ClaudeProviderException has explicit handler; generic Exception has catch-all; both return ApiError |
| R6 — CancellationToken threaded through async | GREEN | SendAsync and StreamAsync both accept CancellationToken; Chat passes it through; StreamChat uses RequestAborted |

**Pillar: YELLOW**

### Security
| Check | Status | Finding |
|---|---|---|
| S1 — No secrets in source files | GREEN | No sk-, InstrumentationKey=, or bearer token strings; ApiKey defaults to string.Empty |
| S2 — Error paths return ApiError only | GREEN | All catch blocks return generic messages; ClaudeProviderException handler never returns ex.Message |
| S3 — New endpoints validate input | GREEN | Both chat and stream endpoints check null/whitespace before any SSE headers are committed |
| S4 — Paid LLM fanout has upper bound | RED | AiController.Chat and AiController.StreamChat: no upper bound on request.Prompt.Length; AnthropicOptions has MaxBatchSize but no MaxPromptLength |
| S5 — CorrelationId in all error responses | GREEN | All ApiError usages pass CorrelationId from HttpContext.GetCorrelationId() |
| S6 — System prompt not echo-able | GREEN | ChatResponse contains Provider, Model, Response only; system prompt not included |
| S7 — No internal details in response headers | GREEN | Only x-correlation-id header added, by design |

**Pillar: RED**

### Cost Optimization
| Check | Status | Finding |
|---|---|---|
| C1 — Prompt caching active on interactive path | N/A | Day 007 scope; EnablePromptCaching header flag present but caching in request body is ClaudeApiClient concern |
| C2 — New endpoints have cost ceiling | RED | Same root as S4: unbounded prompt input = unbounded input token spend |
| C3 — No retry loop multiplying token spend | GREEN | Retry disabled via ShouldHandle = false |
| C4 — Streaming token usage from final chunk | N/A | Day 009 scope; StreamAsync logs usage from message_delta ✓ |
| C5 — Cost savings logged for cost-control features | N/A | No cost-control feature in Day 006 scope |
| C6 — Model ID is a valid current model | GREEN | AnthropicOptions.Model defaults to "claude-sonnet-4-6" — valid |

**Pillar: RED**

### Performance Efficiency
| Check | Status | Finding |
|---|---|---|
| P1 — Streaming sets X-Accel-Buffering: no | N/A | Day 009 scope; current code: X-Accel-Buffering: no present ✓ |
| P2 — Streaming sets Cache-Control: no-cache | N/A | Day 009 scope; current code: Cache-Control: no-cache present ✓ |
| P3 — Streaming calls FlushAsync() after each chunk | N/A | Day 009 scope; current code: Response.Body.FlushAsync() after each write ✓ |
| P4 — No .Result or .Wait() blocking | GREEN | No blocking calls found in any audited file |
| P5 — TTFT instrumented for new streaming paths | N/A | Day 009 scope; current code: GatewayTelemetry.StreamTtftMs.Record() on first chunk ✓ |
| P6 — Streaming HttpClient has InfiniteTimeSpan | GREEN | ClaudeApiClient: Timeout.InfiniteTimeSpan |

**Pillar: GREEN**

### Operational Excellence
| Check | Status | Finding |
|---|---|---|
| O1 — New paths log structured event with CorrelationId | GREEN | Serilog LogContext enriches CorrelationId on every request via middleware; all controller actions log it explicitly |
| O2 — New metric names follow ai.provider.* convention | GREEN | All GatewayTelemetry metrics follow convention: ai.provider.latency.ms, ai.provider.requests, ai.provider.failures, ai.provider.cache.*, ai.provider.batch.*, ai.provider.stream.ttft_ms |
| O3 — New env vars in appsettings-template.md | YELLOW | APPLICATIONINSIGHTS_CONNECTION_STRING documented ✓; stale: Anthropic:Model suggestion was "claude-opus-4-7" (retired) |
| O4 — files-changed.md has row for every file touched | YELLOW | Only collab-lens entries present; Day 006 source files (Program.cs, CorrelationIdMiddleware.cs, GatewayTelemetry.cs, HttpContextExtensions.cs, ClaudeProviderException.cs) untracked — pre-dates files-changed enforcement |
| O5 — KQL cookbook updated for new signals | GREEN | kql-cookbook.md has latency p50/p95/p99, error rate, and token-spend queries covering Day 006 telemetry |
| O6 — /health/ready reflects new required config | GREEN | Checks ApiKey, Model, BaseUrl; APPLICATIONINSIGHTS_CONNECTION_STRING intentionally optional |

**Pillar: YELLOW**

### Responsible AI
| Check | Status | Finding |
|---|---|---|
| RA1 — Prompt/completion content NOT logged | GREEN | Provider and controller log PromptLength only, never prompt text or response text |
| RA2 — Error responses don't expose provider errors | GREEN | All error paths return generic messages; ex.Message never surfaces to caller |
| RA3 — Content policy violations handled explicitly | YELLOW | Global handler switch has no explicit case for Anthropic 400/content_policy; falls to default → 502 instead of 422; ClaudeApiClient not read — full exception path unverified |
| RA4 — Every AI call has audit trail | YELLOW | SendAsync success log has Model + ResponseLength; CorrelationId present via Serilog enrichment ✓; InputTokens/OutputTokens not logged on non-streaming path |
| RA5 — No PII in hardcoded system prompt | GREEN | SystemPrompt defaults to ""; no hardcoded strings in audited files |
| RA6 — Streaming audit trail preserved | N/A | Day 009 scope; current code: finally block logs warning if usage data not received ✓ |

**Pillar: YELLOW**

---

### Summary
| Pillar | Result | Key finding |
|---|---|---|
| Reliability | YELLOW | R4: ValidateOnStart() not wired for AnthropicOptions |
| Security | ~~RED~~ → GREEN | S4: no prompt length cap on interactive endpoints — fixed |
| Cost | ~~RED~~ → GREEN | C2: same root as S4 — fixed |
| Performance | GREEN | All checks pass or N/A |
| Ops Excellence | YELLOW | O3 stale model ID (fixed); O4 files-changed.md gap (accepted debt) |
| Responsible AI | YELLOW | RA3: content policy → 502 not 422; RA4: token counts absent from non-streaming audit trail |

### RED items → fixes
- **S4/C2** Both interactive endpoints lacked prompt length upper bound → added `MaxPromptLength = 32_000` to `AnthropicOptions`; added guard in `AiController.Chat` (returns 400 `prompt_too_long`) and `AiController.StreamChat` (writes 400 before SSE headers) → build clean → **GREEN**

### YELLOW items → fixes
- **R4** ValidateOnStart() not wired: minimum fix = add `[Required]` to `ApiKey`, `Model`, `BaseUrl` in `AnthropicOptions` and chain `.ValidateDataAnnotations().ValidateOnStart()` in Program.cs → **accepted debt** — /health/ready provides runtime guard; dedicated fix warranted as separate change
- **O3** Stale `claude-opus-4-7` in `Infra/Day-006/appsettings-template.md` → updated to `claude-opus-4-8` → **GREEN**
- **O4** files-changed.md incomplete: pre-dates enforcement convention; retroactive row reconstruction speculative → **accepted debt — acknowledged historical gap**
- **RA3** Content policy 400 → 502 mismatch: requires reading ClaudeApiClient exception mapper and updating global handler switch → **accepted debt** — tracked for Day 010+
- **RA4** Token counts absent from non-streaming audit trail: likely captured in ClaudeApiClient layer (unread); verify and add InputTokens/OutputTokens to SendAsync success log → **accepted debt** — tracked for Day 010+

---
