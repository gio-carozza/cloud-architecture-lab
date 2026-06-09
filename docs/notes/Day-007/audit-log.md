# Audit Log — Day 007

> Append-only. Each run adds a dated section. Never edit or delete prior runs.

---

## Run: retroactive audit — current code state (2026-06-08)

> NOTE: audits current file state, not Day 007 snapshot. Code has progressed through Day 009.
> Sources read: ClaudeApiClient.cs, AnthropicOptions.cs, GatewayTelemetry.cs, Program.cs,
> AiController.cs, ClaudeChatModelProvider.cs, Infra/Day-007/appsettings-template.md,
> docs/notes/Day-007/files-changed.md, docs/standards/kql-cookbook.md (queries 8–9)

### Reliability
| Check | Status | Finding |
|---|---|---|
| R1 — HttpClient timeouts set | GREEN | No HttpClient changes this day; InfiniteTimeSpan and 30s values unchanged |
| R2 — No retry on non-idempotent calls | GREEN | No retry changes |
| R3 — Circuit breaker not on batch/streaming | GREEN | Circuit breaker scope unchanged |
| R4 — ValidateOnStart() for IOptions<T> | YELLOW | EnablePromptCaching and SystemPrompt added to AnthropicOptions without ValidateOnStart — carry-forward from Day 006, not new |
| R5 — New exceptions caught by global handler | GREEN | BrokenCircuitException, TimeoutRejectedException, TaskCanceledException, HttpRequestException all caught in ClaudeApiClient and re-thrown as ClaudeProviderException; global handler covers ClaudeProviderException |
| R6 — CancellationToken threaded through async | GREEN | No new async paths without CancellationToken |

**Pillar: YELLOW**

### Security
| Check | Status | Finding |
|---|---|---|
| S1 — No secrets in source files | GREEN | SystemPrompt is config-bound, not hardcoded; no secrets in any source file |
| S2 — Error paths return ApiError only | GREEN | ClaudeProviderException.Message holds provider error text for logging; global handler uses hardcoded generic strings in ApiError, never ex.Message |
| S3 — New endpoints validate input | GREEN | No new endpoints this day; MaxPromptLength guard in place from Day 006 fix |
| S4 — Paid LLM fanout has upper bound | GREEN | MaxPromptLength guard in place |
| S5 — CorrelationId in all error responses | GREEN | Unchanged |
| S6 — System prompt not echo-able | GREEN | TryExtractText reads content[].text from Anthropic response only; system prompt not in response; BuildAnthropicRequest logs SystemPromptLength (count), not content |
| S7 — No internal details in response headers | GREEN | No new response headers |

**Pillar: GREEN**

### Cost Optimization
| Check | Status | Finding |
|---|---|---|
| C1 — Prompt caching active on interactive path | GREEN | BuildAnthropicRequest emits cache_control: {"type":"ephemeral","ttl":"1h"} when EnablePromptCaching=true and SystemPrompt non-empty; TTL included per Claude 4 requirement |
| C2 — New endpoints have cost ceiling | GREEN | MaxPromptLength ceiling in place; no new endpoints |
| C3 — No retry loop multiplying token spend | GREEN | No retry changes |
| C4 — Streaming token usage from final chunk | N/A | Day 009 scope; streaming path also extracts cache tokens from message_start |
| C5 — Cost savings logged for cost-control features | GREEN | Cache activity log: CacheReadTokens + CacheCreationTokens; KQL queries 8 and 9 compute estimated savings |
| C6 — Model ID is a valid current model | GREEN | claude-sonnet-4-6 — valid |

**Pillar: GREEN**

### Performance Efficiency
| Check | Status | Finding |
|---|---|---|
| P1 — Streaming sets X-Accel-Buffering: no | N/A | Day 009 scope |
| P2 — Streaming sets Cache-Control: no-cache | N/A | Day 009 scope |
| P3 — Streaming calls FlushAsync() after each chunk | N/A | Day 009 scope |
| P4 — No .Result or .Wait() blocking | GREEN | No blocking calls in ClaudeApiClient or any Day 007 change |
| P5 — TTFT instrumented for new streaming paths | N/A | Day 009 scope |
| P6 — Streaming HttpClient has InfiniteTimeSpan | GREEN | Unchanged |

**Pillar: GREEN**

### Operational Excellence
| Check | Status | Finding |
|---|---|---|
| O1 — New paths log structured event with CorrelationId | GREEN | Cache activity log includes CacheReadTokens/CacheCreationTokens; CorrelationId flows via Serilog enrichment on all paths |
| O2 — New metric names follow ai.provider.* convention | GREEN | ai.provider.cache.hits, ai.provider.cache.misses — both follow convention |
| O3 — New env vars in appsettings-template.md | GREEN | Infra/Day-007/appsettings-template.md documents Anthropic__EnablePromptCaching and Anthropic__SystemPrompt with TTL note and 1024-token minimum warning |
| O4 — files-changed.md has row for every file touched | GREEN | 25+ rows covering all phases: build, verification, docs pass, audit, deploy, cert-update |
| O5 — KQL cookbook updated for new signals | GREEN | Queries 8 (cache hit rate) and 9 (estimated token savings) added to kql-cookbook.md |
| O6 — /health/ready reflects new required config | GREEN | Both new options optional by design — empty SystemPrompt skips caching gracefully; gateway functional either way |

**Pillar: GREEN**

### Responsible AI
| Check | Status | Finding |
|---|---|---|
| RA1 — Prompt/completion content NOT logged | GREEN | BuildAnthropicRequest logs SystemPromptLength (count only); cache activity log shows token counts only; no prompt or completion text in any log statement |
| RA2 — Error responses don't expose provider errors | GREEN | Provider error message stored in ClaudeProviderException.Message for server-side logging only; ApiError uses hardcoded generic strings |
| RA3 — Content policy violations handled explicitly | YELLOW | ProviderErrorCode extracted and stored; global handler switch uses only ProviderStatusCode — 400/content_policy falls to default → 502 instead of 422; carry-forward from Day 006 |
| RA4 — Every AI call has audit trail | YELLOW | inputTokens/outputTokens set as Activity tags (llm.tokens.input, llm.tokens.output) — queryable in App Insights dependencies table; not in structured log events (traces table); cache tokens added this day but same gap for base token counts |
| RA5 — No PII in hardcoded system prompt | GREEN | SystemPrompt defaults to ""; no hardcoded content in source |
| RA6 — Streaming audit trail preserved | N/A | Day 009 scope |

**Pillar: YELLOW**

---

### Summary
| Pillar | Result | Key finding |
|---|---|---|
| Reliability | YELLOW | R4: ValidateOnStart() carry-forward from Day 006 |
| Security | GREEN | All checks pass |
| Cost | GREEN | Caching active with TTL; savings logged via KQL |
| Performance | GREEN | All checks pass or N/A |
| Ops Excellence | GREEN | appsettings template, files-changed.md, and KQL all complete |
| Responsible AI | YELLOW | RA3: content policy → 502 carry-forward; RA4: token counts in Activity tags, not log events |

### RED items → fixes
None.

### YELLOW items → fixes
- **R4** ValidateOnStart() not wired: carry-forward from Day 006 — **accepted debt**
- **RA3** Content policy 400 → 502 instead of 422: carry-forward from Day 006 — **accepted debt — tracked for Day 010+**
- **RA4** Token counts in Activity tags (dependencies table) not in structured log events (traces table): queryable in App Insights but split across telemetry tables; minimum fix = add `InputTokens`/`OutputTokens` to the "Claude provider request completed" log line — **accepted debt — tracked for Day 010+**

---
