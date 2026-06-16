# Responsible AI Standard

**Phase:** 2 entry requirement (~Day 21) — scaffold now, enforce at Phase 2
**Applies to:** all LLM-facing endpoints and any feature that processes user-supplied content

---

## Why this exists

Phase 2 (Forward-Deployed Engineer) means applying AI to real business problems and communicating at CEO level. The moment this gateway touches real business data or real users, responsible AI stops being a checkbox and becomes a liability gate. This standard defines the minimum bar before any Phase 2 feature ships.

---

## Content filtering

### Input filtering (pre-LLM)

Every prompt submitted to the gateway must pass through a content check before reaching the provider. Minimum requirements:

| Check | Enforcement |
|---|---|
| Max prompt length | `MaxPromptLength` in `AnthropicOptions` (default: 10,000 chars); return `400 VALIDATION_FAILED` if exceeded |
| PII detection (Phase 2) | Detect and redact or reject prompts containing SSN, credit card, email patterns before forwarding |
| Prompt injection detection (Phase 2) | Flag prompts that attempt to override system instructions (`ignore previous instructions`, `you are now`, etc.) |
| Blocked content categories (Phase 2) | Configurable deny-list via `ContentFilterOptions`; return `400 CONTENT_POLICY_VIOLATION` |

For Phase 1: max length check is the only enforced control. All others are scaffolded as future `IContentFilter` implementations.

### Output filtering (post-LLM)

Anthropic's safety systems apply at the provider level. For Phase 2, add a post-response filter that:

- Detects if the model returned a refusal (`stop_reason: "max_tokens"` with incomplete safety patterns)
- Logs the event as `ai.safety.refusal` with correlation ID (never log the prompt or response content)
- Returns `503 PROVIDER_CONTENT_FILTERED` to the caller

---

## PII handling

**Never log prompt content** at any log level in production. Prompts may contain user PII that was not explicitly provided — inferred PII is still PII.

| Data | Allowed in logs | Allowed in telemetry tags | Allowed in App Insights |
|---|---|---|---|
| Full prompt text | No | No | No |
| Response text | No | No | No |
| Prompt length (token count) | Yes | Yes | Yes |
| User ID (if present) | Hashed only | Hashed only | Hashed only |
| Correlation ID | Yes | Yes | Yes |

When Phase 2 introduces user identity: hash user IDs with a one-way function before any logging. Never store raw user identifiers in telemetry.

---

## Audit logging

Every LLM call must produce an audit record. In Phase 1, this is satisfied by the OpenTelemetry span on `ai.chat.*`. In Phase 2, add a dedicated audit log with:

| Field | Notes |
|---|---|
| `timestamp` | UTC |
| `correlationId` | Links to the request span |
| `path` | Which endpoint was called |
| `inputTokens` | From `TryExtractUsage` |
| `outputTokens` | From `TryExtractUsage` |
| `model` | From `AnthropicOptions.Model` |
| `cacheHit` | Boolean — was this a cache read? |
| `durationMs` | Total round-trip |
| `outcome` | `success`, `provider_error`, `content_filtered`, `client_cancelled` |

Audit logs must be retained for 90 days minimum (configure in Log Analytics workspace retention). Audit logs must NOT be queryable by prompt content — only metadata.

---

## Bias and fairness

For Phase 2 features that produce decisions or recommendations:

- Document the use case and the population it serves in the feature's ADR
- Include a "Bias Risk" section in the ADR: what groups might be disadvantaged, what the mitigation is
- If the feature is used in hiring, lending, medical, or legal contexts: escalate to human review before shipping

This gateway is currently a general-purpose routing layer. Bias risk is low. Flag this section when Phase 2 introduces domain-specific prompts.

---

## Human-in-the-loop requirements

| Feature type | Human review required before output is acted on |
|---|---|
| Informational response (chat) | No |
| Automated action (agent triggers tool) | Yes — Phase 2 |
| Decision with material impact (loan, hire, medical) | Yes — always |
| Batch output used to train another model | Yes — always |

Phase 1 and current Phase 2 scope: informational responses only. No automated actions yet.

---

## Incident response for AI failures

If a safety or content incident occurs (model produced harmful output, PII was logged, prompt injection succeeded):

1. Disable the affected endpoint immediately (`ASPNETCORE_URLS` override or App Service stop)
2. Capture the correlation ID from the report
3. Query App Insights for the full span (inputs, outputs, timing) using that correlation ID
4. Do NOT share prompt/response content outside of the investigation
5. File a post-incident note in `docs/notes/Day-NNN/` (even if Day NNN is already closed — create a new entry)
6. Open an ADR if the fix requires a design change

---

## Checklist: Phase 2 feature gate

Before any Phase 2 feature that touches real user data ships:

- [ ] PII detection implemented or explicitly deferred with documented risk acceptance
- [ ] Audit log fields verified in App Insights
- [ ] Prompt length limit enforced
- [ ] Bias risk section in the ADR
- [ ] Human-in-the-loop requirement assessed and documented
- [ ] Responsible AI section added to the day's `03-architect-thinking.md`
