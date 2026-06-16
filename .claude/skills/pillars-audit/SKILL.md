# Pillars Audit Skill

**TRIGGER:** user says "pillars audit / security review / architecture review / is this ready to ship / audit the changes", or STEP 8 gate (full audit), `/deploy` gate (halts on RED), or STEP 12 close gate (targeted re-audit, O4/O5 + newly touched source files only).

**Purpose:** Check day's changes against 5 WAF pillars + Responsible AI. Catch design gaps, security holes, cost traps, and observability blind spots the build and tests cannot catch.

**Inputs to read before running:**

1. `docs/notes/Day-NNN/01-summary.md` — what was built
2. `docs/notes/Day-NNN/07-files-changed.md` — every file touched
3. Each changed source file listed in `07-files-changed.md`
4. `src/lab-observability-api/Program.cs` — DI registrations and pipeline
5. `src/lab-observability-api/Options/AnthropicOptions.cs` — config contract

---

## Output format

Append this block to `docs/notes/Day-NNN/05-audit-log.md` under a dated run header.
Use `N/A` (not GREEN) for checks that do not apply to today's changes.

````markdown
## Run: <STEP 8 pre-deploy | STEP 12 close-audit | deploy gate> (YYYY-MM-DD)

### Reliability
| Check | Status | Finding |
|---|---|---|
| R1 — HttpClient timeouts set | GREEN/YELLOW/RED/N/A | <finding or —> |
| R2 — No retry on non-idempotent calls | | |
| R3 — Circuit breaker not on batch/streaming | | |
| R4 — ValidateOnStart() for IOptions<T> | | |
| R5 — New exceptions caught by global handler | | |
| R6 — CancellationToken threaded through async | | |

**Pillar: GREEN/YELLOW/RED**

### Security
| Check | Status | Finding |
|---|---|---|
| S1 — No secrets in source files | | |
| S2 — Error paths return ApiError only | | |
| S3 — New endpoints validate input | | |
| S4 — Paid LLM fanout has upper bound | | |
| S5 — CorrelationId in all error responses | | |
| S6 — System prompt not echo-able | | |
| S7 — No internal details in response headers | | |

**Pillar: GREEN/YELLOW/RED**

### Cost Optimization
| Check | Status | Finding |
|---|---|---|
| C1 — Prompt caching active on interactive path | | |
| C2 — New endpoints have cost ceiling | | |
| C3 — No retry loop multiplying token spend | | |
| C4 — Streaming token usage captured from final chunk | | |
| C5 — Cost savings logged for cost-control features | | |
| C6 — Model ID is a valid current model | | |

**Pillar: GREEN/YELLOW/RED**

### Performance Efficiency
| Check | Status | Finding |
|---|---|---|
| P1 — Streaming sets X-Accel-Buffering: no | | |
| P2 — Streaming sets Cache-Control: no-cache | | |
| P3 — Streaming calls FlushAsync() after each chunk | | |
| P4 — No .Result or .Wait() blocking | | |
| P5 — TTFT instrumented for new streaming paths | | |
| P6 — Streaming HttpClient has InfiniteTimeSpan | | |

**Pillar: GREEN/YELLOW/RED**

### Operational Excellence
| Check | Status | Finding |
|---|---|---|
| O1 — New paths log structured event with CorrelationId | | |
| O2 — New metric names follow ai.provider.* convention | | |
| O3 — New env vars in appsettings-template.md | | |
| O4 — 07-files-changed.md has row for every file touched | | |
| O5 — KQL cookbook updated for new signals | | |
| O6 — /health/ready reflects new required config | | |

**Pillar: GREEN/YELLOW/RED**

### Responsible AI
| Check | Status | Finding |
|---|---|---|
| RA1 — Prompt/completion content NOT logged | | |
| RA2 — Error responses don't expose provider errors | | |
| RA3 — Content policy violations handled explicitly | | |
| RA4 — Every AI call has audit trail | | |
| RA5 — No PII in hardcoded system prompt | | |
| RA6 — Streaming audit trail preserved | | |

**Pillar: GREEN/YELLOW/RED**

---

### Summary
| Pillar | Result | Key finding |
|---|---|---|
| Reliability | GREEN/YELLOW/RED | <one-line or "all checks pass or N/A"> |
| Security | GREEN/YELLOW/RED | |
| Cost | GREEN/YELLOW/RED | |
| Performance | GREEN/YELLOW/RED | |
| Ops Excellence | GREEN/YELLOW/RED | |
| Responsible AI | GREEN/YELLOW/RED | |

### RED items → fixes
- **<ID>** <finding>: <fix applied> → re-audit → **GREEN / still RED**

### YELLOW items → fixes
- **<ID>** <finding>: <fix applied or deferred reason> → **GREEN / still YELLOW — accepted debt**

---
````

**RAG definitions:** GREEN = no issues. YELLOW = known gap, accepted debt, documented. RED = must fix before deploy.

---

## Check reference

| # | Check | How to verify |
|---|---|---|
| R1 | `HttpClient` timeout set | Read client registration in `Program.cs`; `InfiniteTimeSpan` only for streaming clients with explicit cancellation |
| R2 | No retry on non-idempotent calls | Grep new client registrations for `AddStandardResilienceHandler`; streaming/batch clients must use no-resilience path |
| R3 | Circuit breaker not on batch/streaming | Verify circuit breaker is only on interactive `ClaudeApiClient` |
| R4 | `ValidateOnStart()` wired for new `IOptions<T>` | Grep `Configure<` in `Program.cs`; each binding should chain `.ValidateDataAnnotations().ValidateOnStart()` |
| R5 | New exceptions caught by global handler | Read `Program.cs` global handler; confirm new exception class is caught or falls through safely |
| R6 | `CancellationToken` threaded through new async methods | Check method signatures on new services and controllers |
| S1 | No secrets in source | Grep for `sk-`, `InstrumentationKey=`, bearer tokens in changed files |
| S2 | Error paths return `ApiError` only | Read every new `catch` block; no raw exception messages, no stack traces |
| S3 | New endpoints validate input | Read new controller actions; `POST` bodies must reject null/empty/oversized inputs |
| S4 | Paid LLM fanout has upper bound | Search new controller actions for size/count cap; absent = RED |
| S5 | `CorrelationId` in all error responses | Read `ApiError` usages in new controller code |
| S6 | System prompt not echo-able | Confirm system prompt not included in `ChatResponse` |
| S7 | No internal details in response headers | Check any new `Response.Headers.Add(...)` calls |
| C1 | Prompt caching active on interactive path | Read `ClaudeApiClient.BuildAnthropicRequest`; confirm `cache_control: {"type":"ephemeral","ttl":"1h"}` when `EnablePromptCaching=true` |
| C2 | New endpoint has cost ceiling | `MaxBatchSize` pattern; absence is RED |
| C3 | No retry loop multiplying token spend | Cross-check R2; any retry on a paying call path = cost RED |
| C4 | Streaming token usage captured from final chunk | `usage.output_tokens` from `message_delta` must be logged |
| C5 | Cost savings logged | Check batch retrieval and async paths for savings logging |
| C6 | Model ID is valid | Valid: `claude-opus-4-8`, `claude-sonnet-4-6`, `claude-haiku-4-5-20251001` |
| P1 | Streaming sets `X-Accel-Buffering: no` | Read streaming controller action |
| P2 | Streaming sets `Cache-Control: no-cache` | Same controller action |
| P3 | Streaming calls `FlushAsync()` after each chunk | Read the streaming write loop |
| P4 | No `.Result` or `.Wait()` blocking | Grep changed files; should be zero |
| P5 | TTFT instrumented for new streaming path | Confirm `GatewayTelemetry.StreamTtftMs.Record(...)` called on first chunk |
| P6 | Streaming `HttpClient` has `InfiniteTimeSpan` | Read streaming client registration |
| O1 | New paths log structured event with `CorrelationId` | Read new controller and service methods |
| O2 | New metric names follow `ai.provider.*` | Grep new `Meter.Create*` calls in `GatewayTelemetry.cs` |
| O3 | New env vars in `appsettings-template.md` | Read the template; "No new settings" only if genuinely true |
| O4 | `07-files-changed.md` has row for every file touched | Cross-check against git diff |
| O5 | KQL cookbook updated for new signals | If new telemetry added → `kql-cookbook.md` should have a query |
| O6 | `/health/ready` reflects new required config | Read readiness handler in `Program.cs` |
| RA1 | Prompt/completion NOT logged | Grep changed files for `Log*` near `request.Prompt`, `response.Text`, user content fields |
| RA2 | Error responses don't expose provider errors | Read `ClaudeProviderException` handling; `ex.Message` must not appear in `ApiError.Message` |
| RA3 | Content policy violations handled explicitly | Read `ClaudeApiClient` error parsing; content policy → 422, not 500 |
| RA4 | Every AI call has audit trail | Structured log after each call must include `InputTokens`, `OutputTokens`, `CorrelationId` |
| RA5 | No PII in system prompt | Read `AnthropicOptions.SystemPrompt` default and hardcoded prompt strings |
| RA6 | Streaming audit trail preserved | `message_delta` usage logged before stream closes |

---

## Execution steps

1. Read all inputs listed above.
2. Work through every check. Mark `N/A` for checks that don't apply to today's changes.
3. Assign RAG per pillar based on the most severe finding.
4. Populate the output format table and fix sections.
5. Append to `05-audit-log.md` under the appropriate run header.
6. RED items: stop, fix, re-run only the affected pillar, update `### RED items → fixes`. Do not deploy with any open RED.
7. YELLOW items: record finding and disposition. Upsert debt row in `07-files-changed.md` (step: `audit`).

---

**Security is a binary.** A stack trace in a 500 is not "low severity" — it is a shipped vulnerability. GREEN means zero exposure.

**Cost RED items are invisible until the bill arrives.** An uncapped ingress that fans out to paid LLM calls is a ticking billing event. Every new fanout endpoint needs a ceiling before deploy.

**Responsible AI is not a Phase 3 concern.** Logging user prompts "just for debugging" is a GDPR incident waiting to happen. Log what the model did (tokens, latency); never log what the user said.

**YELLOW debt is real debt.** An audit that produces only GREENs on a real day of changes is almost always a superficial audit.
