# Pillars Audit Skill

**TRIGGER when:** user says "pillars audit", "security review", "architecture review",
"is this ready to ship", "audit the changes", or when the STEP 8 audit gate is reached
in the daily workflow.

**Purpose:** Before every deploy, check the day's changes against 6 pillars:
5 Azure Well-Architected Framework pillars + Responsible AI. Catch what the
build and tests can't catch — design gaps, security holes, cost traps, and
observability blind spots.

**Output format:** One line per pillar with a RAG status and specific evidence.
Finish with a block of any RED items that must be fixed before deploy.

```
RELIABILITY     [GREEN|YELLOW|RED] — <finding or "all checks pass">
SECURITY        [GREEN|YELLOW|RED] — <finding>
COST            [GREEN|YELLOW|RED] — <finding>
PERFORMANCE     [GREEN|YELLOW|RED] — <finding>
OPS EXCELLENCE  [GREEN|YELLOW|RED] — <finding>
RESPONSIBLE AI  [GREEN|YELLOW|RED] — <finding>

RED ITEMS (fix before deploy):
- <item> → <minimum fix>
```

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
2. For each pillar, work through the checks. Skip checks that don't apply to today's changes (note them as N/A, not GREEN).
3. Assign RAG per pillar based on the most severe finding in that pillar.
4. List all RED items with the minimum fix required.
5. If any RED items exist: stop. Fix them. Re-run only the affected pillar checks. Do not proceed to STEP 9 (deploy) with any open RED.
6. YELLOW items: document in `docs/notes/Day-NNN/files-changed.md` as known debt with the step label `audit`.

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
