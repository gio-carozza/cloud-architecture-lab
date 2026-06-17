# Security Standard

**Phase:** 2 entry requirement (~Day 21) — foundational rules active now
**Applies to:** all code in `src/lab-observability-api/` and all Azure infrastructure

---

## Foundational rules (active Phase 1)

These are non-negotiable from Day 1. `/repo-audit` will flag violations.

| Rule | Enforcement |
|---|---|
| No secrets in source code | `dotnet user-secrets` locally; App Service env vars in Azure. Never `appsettings.json`. |
| No stack traces in API responses | The global exception pipeline in `Program.cs` returns `ApiError` only. See `error-handling-standard.md`. |
| No raw prompt content in logs | See `responsible-ai.md`. |
| HTTPS only | App Service enforces HTTPS redirect. Never disable. |
| Correlation ID on every response | `CorrelationIdMiddleware` sets `X-Correlation-Id` header. Never remove. |

---

## Input validation

Every controller action that accepts user input must validate it before processing:

```csharp
// Use model attributes — not manual if-checks
public class ChatRequest
{
    [Required]
    [StringLength(10000, MinimumLength = 1)]
    public string Prompt { get; set; } = string.Empty;
}
```

Actual implementation: manual guard clauses in the controller action (`AiController.Chat`,
`AiBatchController.Submit`) — null/whitespace prompt, length over `AnthropicOptions.MaxPromptLength`,
batch count over `MaxBatchSize` — each returning `BadRequest(new ApiError(Code: "invalid_request" | "prompt_too_long" | "batch_size_exceeded", ...))`.
`ChatRequest` carries no data annotations today, so `[ApiController]` automatic
model-state validation does not fire for prompt checks. Never expose raw `ModelStateDictionary` to callers regardless of which validation path is used.

### Prompt injection mitigation (Phase 2)

Implement `IContentFilter` with a prompt injection detection pass before forwarding to the provider. Log injection attempts as `Warning` with correlation ID. Do not return the detected pattern to the caller.

---

## Secret management

| Environment | Mechanism | Format |
|---|---|---|
| Local dev | `dotnet user-secrets` | `Anthropic:ApiKey` (colon separator) |
| Azure App Service | Application Settings | `Anthropic__ApiKey` (double underscore) |
| Phase 2+ (production) | Azure Key Vault reference | `@Microsoft.KeyVault(SecretUri=...)` |

Rotation: API keys must be rotatable without code redeploy. The `AnthropicOptions` pattern satisfies this — the key lives in config, not code.

Never commit:

- `.env` files
- `appsettings.Production.json`
- `*.pfx`, `*.p12`, `*.key`
- Any file containing a real API key, connection string, or certificate

The deny list in `.claude/settings.json` enforces this for Claude Code sessions.

---

## Dependency vulnerability policy

| Action | Cadence |
|---|---|
| Run `dotnet list package --vulnerable` | Every 10 days (end of each Day NNN divisible by 10) |
| Review output | Flag any HIGH or CRITICAL severity package |
| Update vulnerable package | Within the same day if HIGH/CRITICAL; within 5 days if MEDIUM |
| Log the check | One row under the current day's section in `docs/notes/changelog.md` (step: `security-scan`, change: result) |

Do not add packages with known HIGH/CRITICAL vulnerabilities even if they are transitive. If a transitive dependency is vulnerable and cannot be updated, document the risk in `docs/notes/changelog.md` and open a tracking ADR.

---

## Authentication and authorization (Phase 2)

Phase 1: the gateway has no authentication. This is acceptable for a lab environment with no real user data.

Phase 2 entry requirement: before any Phase 2 feature accepts real user data:

- [ ] Azure AD / Entra ID token validation on the API (use `Microsoft.Identity.Web`)
- [ ] All endpoints require `[Authorize]` except `/health`
- [ ] Token audience and issuer validated — not just signature
- [ ] Service-to-service calls use Managed Identity — no service account passwords

---

## Rate limiting (Phase 2)

Add ASP.NET Core rate limiting middleware (`Microsoft.AspNetCore.RateLimiting`) before Phase 2 goes live:

| Endpoint | Limit | Window |
|---|---|---|
| `POST /api/ai/chat` | 20 requests | 60 seconds per client IP |
| `POST /api/ai/chat/stream` | 10 requests | 60 seconds per client IP |
| `POST /api/ai/batch` | 5 submits | 60 seconds per client IP |
| `GET /api/ai/batch/{id}` | 60 polls | 60 seconds per client IP |

Return `429` with `ApiError(Code: "rate_limited")` and a `Retry-After` header when
limit exceeded — `rate_limited` to be added to the taxonomy in `error-handling-standard.md`
when this ships; it does not exist today.

---

## OWASP Top 10 checklist (evaluate per feature)

| Risk | Status | Mitigation |
|---|---|---|
| A01 Broken Access Control | Phase 2 | Auth gate in Phase 2 |
| A02 Cryptographic Failures | Phase 1 ✅ | HTTPS enforced, no secrets in code |
| A03 Injection | Phase 1 ✅ | No SQL; prompt injection detection Phase 2 |
| A04 Insecure Design | Phase 1 ✅ | Provider abstraction, error contract |
| A05 Security Misconfiguration | Phase 1 ✅ | `ValidateOnStart()`, no stack traces |
| A06 Vulnerable Components | Ongoing | `dotnet list package --vulnerable` |
| A07 Auth Failures | Phase 2 | Entra ID auth in Phase 2 |
| A08 Software Integrity Failures | Phase 1 ✅ | NuGet packages from official feed only |
| A09 Logging Failures | Phase 1 ✅ | Structured logging, correlation ID, no PII |
| A10 SSRF | Evaluate | If any user-supplied URL is ever fetched — add allowlist |

---

## Threat model (current Phase 1 scope)

**Assets:** Anthropic API key, system prompt content, telemetry data.

**Threats:**

| Threat | Likelihood | Impact | Mitigation |
|---|---|---|---|
| API key leaked via logs | Medium | High | No secret logging enforced |
| API key leaked via git | Low | High | `.gitignore` + deny list in settings |
| Prompt injection causing unintended output | Medium | Medium | Phase 2 content filter |
| Cost explosion from uncapped requests | Medium | Medium | `MaxTokens`, budget alert |
| Unauthorized access to chat endpoint | Low (lab env) | Medium | Auth gate Phase 2 |

Review and update this threat model at each phase transition.
