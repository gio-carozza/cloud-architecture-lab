# Pillars Audit Skill

**TRIGGER when:**
- User says "pillars audit", "security review", "architecture review", "is this ready to ship", or "audit the changes"
- **STEP 8 gate** — pre-deploy audit in the daily workflow (full audit against all changed files)
- **`/deploy` gate** — Step 0 of `.claude/commands/deploy.md`; if no passing audit row exists in `files-changed.md`, this runs automatically before any build step (RED items halt the deploy)
- **STEP 12 close gate** — targeted re-audit covering only files changed after STEP 8 (docs pass edits, posture-gap fixes); focuses on O4, O5, and any newly touched source files

**Purpose:** Before every deploy, check the day's changes against 6 pillars:
5 Azure Well-Architected Framework pillars + Responsible AI. Catch what the
build and tests can't catch — design gaps, security holes, cost traps, and
observability blind spots.

**Output format:** Per-check table for every pillar, then RED and YELLOW fix sections.
Append the full block to `docs/notes/Day-NNN/audit-log.md` under a dated run header.
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
| O4 — files-changed.md has row for every file touched | | |
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

### RED items → fixes
- **<ID>** <finding>: <fix applied> → re-audit → **GREEN / still RED**

### YELLOW items → fixes
- **<ID>** <finding>: <fix applied or deferred reason> → **GREEN / still YELLOW — accepted debt**

---
````

For STEP 12 (close-audit) runs, include only the re-checked pillars — not all six.

**RAG definitions:**
- GREEN — no issues found
- YELLOW — known gap, accepted as debt, documented
- RED — must fix before deploy; shipping this is the wrong answer

---

## Inputs to read before running checks

1. `docs/notes/Day-NNN/summary.md` — what was built and the contract
2. `docs/notes/Day-NNN/files-changed.md` — every file touched this day
3. The actual changed source files (read each one listed in files-changed.md)
4. `src/lab-observability-api/Program.cs` — DI registrations and pipeline
5. `src/lab-observability-api/Options/AnthropicOptions.cs` — config contract

---

## Pillar 1: Reliability

Check all that apply to today's changes:

| # | Check | How to verify |
|---|---|---|
| R1 | Every new `HttpClient` has a timeout set — no hanging calls | Read client registration in `Program.cs`; `Timeout.InfiniteTimeSpan` is only correct for streaming/long-running clients that explicitly manage cancellation |
| R2 | No retry on non-idempotent calls — streaming and batch submits must not retry | Grep new client registrations for `AddStandardResilienceHandler`; streaming/batch clients must use no-resilience path |
| R3 | Circuit breaker not applied to paths where "failure" is normal (batch `in_progress`, streaming partial) | Verify circuit breaker is only on the interactive `ClaudeApiClient`, not batch or streaming clients |
| R4 | `ValidateOnStart()` wired for any new `IOptions<T>` binding | Grep `Configure<` in `Program.cs`; each binding should chain `ValidateDataAnnotations().ValidateOnStart()` or equivalent |
| R5 | New exception types caught by the global exception handler | Read `Program.cs` global `app.Use()` handler; confirm it catches the new exception class or falls through safely to the generic handler |
| R6 | `CancellationToken` threaded through all new async methods | Check method signatures on new services and controllers |

---

## Pillar 2: Security

| # | Check | How to verify |
|---|---|---|
| S1 | No secrets in any `.cs`, `appsettings.json`, `appsettings.*.json`, or committed Infra files | Grep for `sk-`, `InstrumentationKey=`, bearer token strings in changed files |
| S2 | All API error paths return `ApiError` contract — no raw exception messages, no stack traces, no provider-internal error text passed to callers | Read every new `catch` block and error return in changed controllers and services |
| S3 | New ingress endpoints validate input — null checks, length bounds, required fields | Read new controller action bodies; `POST` bodies must reject null/empty prompt, oversized inputs |
| S4 | Any endpoint that fans out to paid LLM calls has an upper bound | Search new controller actions for a size/count cap; if absent, this is RED |
| S5 | Correlation ID present in all error responses | Read `ApiError` usages in new controller code |
| S6 | System prompt not echo-able via any API response field | Read `ClaudeApiClient` and `ClaudeChatModelProvider` — confirm system prompt is not included in `ChatResponse` |
| S7 | New HTTP response headers don't leak internal implementation details | Check any new `Response.Headers.Add(...)` calls |

---

## Pillar 3: Cost Optimization

| # | Check | How to verify |
|---|---|---|
| C1 | Prompt caching active on the interactive path | Read `ClaudeApiClient.BuildAnthropicRequest`; confirm `cache_control: {"type":"ephemeral","ttl":"1h"}` emitted when `EnablePromptCaching=true` |
| C2 | Every new endpoint that fans out to LLM calls has a documented cost ceiling | MaxBatchSize pattern — check new endpoints for equivalent guard; absence is RED |
| C3 | No retry loop that could multiply token spend | Cross-check R2; any retry on a paying call path is a cost RED |
| C4 | New streaming endpoint: token usage captured from final chunk and logged | Read streaming implementation; `usage.output_tokens` from `message_delta` must be logged |
| C5 | `EstimatedSavingsUsd` or equivalent cost-benefit logged for cost-control features | Check batch retrieval and any new async path for savings logging |
| C6 | Model in `AnthropicOptions` default is a current valid model ID | Valid IDs: `claude-opus-4-8`, `claude-sonnet-4-6`, `claude-haiku-4-5-20251001` — wrong default = silent wrong billing |

---

## Pillar 4: Performance Efficiency

| # | Check | How to verify |
|---|---|---|
| P1 | Streaming response sets `X-Accel-Buffering: no` | Read streaming controller action; nginx on App Service will buffer without this |
| P2 | Streaming response sets `Cache-Control: no-cache` | Same controller action |
| P3 | Streaming endpoint calls `FlushAsync()` after each chunk write | Read the streaming write loop |
| P4 | No `.Result` or `.Wait()` blocking on async calls (deadlock risk in ASP.NET Core) | Grep changed files for `.Result` and `.Wait()`; should be zero |
| P5 | First-token latency instrumented for any new streaming path | Confirm `GatewayTelemetry.StreamFirstTokenMs.Record(...)` called on first chunk |
| P6 | HttpClient for streaming has `Timeout = InfiniteTimeSpan` | Read streaming client registration; a short timeout will kill long responses mid-stream |

---

## Pillar 5: Operational Excellence

| # | Check | How to verify |
|---|---|---|
| O1 | Every new code path logs at least one structured event with `CorrelationId` | Read new controller and service methods; `_logger.LogInformation(...)` with correlation ID via Serilog enrichment (automatic via middleware, but verify nothing swallows it) |
| O2 | New telemetry metric names follow `ai.provider.*` convention | Grep new `Meter.CreateCounter` / `CreateHistogram` calls in `GatewayTelemetry.cs` |
| O3 | New App Service environment variables documented in `Infra/Day-NNN/appsettings-template.md` | Read the template; "No new settings" is correct only if genuinely true |
| O4 | `files-changed.md` has a row for every file touched | Cross-check files-changed.md against git diff |
| O5 | KQL cookbook updated if new observable signals were added | If new telemetry metrics added → kql-cookbook.md should have a query or a note |
| O6 | `/health/ready` still correctly reflects readiness (new required config not missing from the check) | Read `/health/ready` handler in `Program.cs`; if a new required option was added, the readiness check must test it |

---

## Pillar 6: Responsible AI

| # | Check | How to verify |
|---|---|---|
| RA1 | Prompt and completion content NOT logged at any level | Grep changed files for `LogInformation`, `LogDebug`, `LogWarning` near `request.Prompt`, `response.Text`, or any field that could contain user content or model output |
| RA2 | Error responses don't expose raw provider error messages to callers | Read `ClaudeProviderException` handling; `ex.Message` must not appear in `ApiError.Message` returned to callers |
| RA3 | Content policy violations from Anthropic (400 with `content_policy` type) handled explicitly — not swallowed as 500 | Read `ClaudeApiClient` error parsing; content policy errors should map to 422, not generic 502/500 |
| RA4 | Every AI call has an audit trail: correlation ID + token usage in server-side logs | Read `ClaudeChatModelProvider` and any new provider; structured log after each call must include `InputTokens`, `OutputTokens`, `CorrelationId` |
| RA5 | No PII-shaped data in system prompt hardcoded in source | Read `AnthropicOptions.SystemPrompt` default and any hardcoded system prompt strings |
| RA6 | Streaming path preserves the audit trail — final usage event logged, not just discarded | For streaming implementations, verify the `message_delta` usage is logged before the stream closes |

---

## Audit Execution Steps

1. Read the inputs listed above.
2. For each pillar, work through every check. Mark N/A (not GREEN) for checks that don't apply to today's changes.
3. Assign RAG per pillar based on the most severe finding in that pillar.
4. Populate the full per-check table and RED/YELLOW fix sections from the output format above.
5. Append the completed block to `docs/notes/Day-NNN/audit-log.md` under the appropriate run header.
6. If any RED items exist: stop. Fix them. Re-run only the affected pillar checks. Update the `### RED items → fixes` section with the fix applied and re-audit result. Do not proceed to STEP 9 (deploy) with any open RED.
7. YELLOW items: record the finding and disposition in the `### YELLOW items → fixes` section. Also upsert a debt row in `docs/notes/Day-NNN/files-changed.md` with step label `audit`.

---

## Legendary Architect Notes (read before every audit)

**Security is a binary.** A stack trace in a 500 response is not "low severity" —
it is a shipped vulnerability. GREEN means zero exposure, not "probably fine."

**Cost RED items are often invisible until the bill arrives.** An uncapped ingress
that fans out to paid LLM calls is not a "nice to have" fix — it is a ticking
billing event. Every new endpoint that multiplies spend needs a ceiling before
it deploys.

**Responsible AI is not a Phase 3 concern.** Logging user prompts "just for
debugging" is a GDPR incident waiting to happen. The gateway processes text
that may contain names, health data, financial details. Log what the model did
(tokens, latency, provider); never log what the user said.

**YELLOW debt is real debt.** Mark it. Name the risk. Write the minimum fix.
An audit that produces only GREENs on a real day of changes is almost always
a superficial audit.
