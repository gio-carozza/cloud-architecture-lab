# Day 6 — Observability & Resilience for the AI Gateway

## Track

Hybrid (Build primary, Cert reinforcement secondary)

## Focus

Transform the Day-5 Claude-backed AI Gateway from "it works" to
"it works under failure, at scale, with full visibility."

## Why This Matters (enterprise context)

Day 5 proved the gateway can call Claude. That's table stakes. In production,
the questions an architect must answer are:

1. **When something breaks, how fast do I know?**
2. **When Anthropic returns a 503, what happens to my caller?**
3. **What does this request cost me in tokens, right now?**
4. **Can I prove SLA compliance from telemetry alone?**
5. **If the same correlation ID appears in three systems, can I stitch the trace?**

A Claude integration that lacks structured logging, distributed tracing,
correlation IDs, retry/timeout policy, and cost telemetry is a liability —
not a platform. Day 6 closes this gap. This is the work that separates
"developer who can call an LLM" from "architect who can run an LLM platform."

## Whose Problem Am I Solving?

### Collaboration Lens (Day 006)

**Primary — DevOps / SRE**
Posture: good citizen — brief new failure modes at deploy time, before the 3am page
Today's question: what would help you tell "correlation ID missing" from "service down" without escalating?

**10yo:** Today we built a diary for the gateway so when it misbehaves, whoever's on-call can read what happened instead of guessing.
**CEO:** Structured logs and a KQL starter pack cut mean-time-to-diagnose from "call the developer" to "read the dashboard" — that's the cost of every future incident we never have to escalate.
**Engineer:** Serilog → App Insights, correlation ID middleware, token-spend KQL — the on-call runbook can now answer "is this a provider timeout or an app error?" without reading code.
**Architect:** Observability is the contract the SRE signs off on before anything deploys; without it, every future feature ships a black box into production regardless of its own quality.

**Also in frame:**

- Security/AppSec/CISO — redaction and the no-stack-trace contract are P1 self-audit items that address the question they'd ask in review
- Eng Manager/Tech Lead — two ADRs accepted in one day (006, 008) is a scope signal worth communicating proactively

Primary: the on-call engineer at 3 AM who gets paged when the gateway
starts misbehaving. Today's work is the difference between "stares at a
black box" and "diagnoses in minutes."

Secondary: the FinOps person who needs to know what the gateway costs in
tokens per hour, and the architect (future-me) reviewing this system in
six months without remembering how it works.

## What I Will Build

A production-grade observability and resilience layer on the existing AI Gateway:

1. **Structured logging** with Serilog → Console + Application Insights
2. **Correlation ID middleware** that propagates `X-Correlation-Id` end-to-end
3. **Request/response logging** with redaction (no full prompt bodies in logs)
4. **LLM-specific telemetry**: tokens-in, tokens-out, provider latency, model
5. **Resilience pipeline** (Microsoft.Extensions.Http.Resilience): retry with
   jitter, attempt timeout, circuit breaker
6. **Production error contract**: never leak stack traces; return correlationId
7. **App Insights workspace** provisioned and wired to App Service
8. **KQL starter pack**: latency p95, error rate, token spend per hour
9. **ADR-006** documenting the observability stack decision

## Step-by-Step Execution

### Phase A — Provision App Insights (15 min)

```powershell
$RG = "rg-ai-lab-dev-eastus"
$LOC = "eastus"
$LAW = "law-ai-lab-dev-eastus-gio"
$AI  = "appi-ai-lab-api-dev-eastus-gio"
$APP = "app-ai-lab-api-dev-eastus-gio"

# Log Analytics workspace (the storage backend)
az monitor log-analytics workspace create -g $RG -n $LAW -l $LOC

# Application Insights (workspace-based — the modern pattern)
az monitor app-insights component create `
  -g $RG -a $AI -l $LOC `
  --workspace $LAW --kind web

# Connection string → app setting on the API
$CS = az monitor app-insights component show -g $RG -a $AI --query connectionString -o tsv
az webapp config appsettings set -g $RG -n $APP `
  --settings APPLICATIONINSIGHTS_CONNECTION_STRING="$CS"
```

**Note:** Day 4 already provisioned `appi-ai-lab-api-dev-eastus-gio`. Day 6
verifies it is workspace-based and reuses it instead of creating a duplicate.
A Log Analytics workspace `law-ai-lab-dev-eastus-gio` is created if one does
not already exist.

### Phase B — Add packages (5 min)

Per ADR-006, the stack is OpenTelemetry-first with Serilog as a logging
library. The `.csproj` already has OTel and resilience packages from
Day 5. Day 6 adds Serilog only.

In `lab-observability-api.csproj`, add:

```xml
<PackageReference Include="Serilog.AspNetCore" Version="8.0.*" />
<PackageReference Include="Microsoft.Extensions.Http.Resilience" Version="8.*" />
```

DO NOT install `Microsoft.ApplicationInsights.AspNetCore` or
`Serilog.Sinks.ApplicationInsights`. See ADR-006 for the rationale.

### Phase C — Wire Serilog into the OTel pipeline in Program.cs (20 min)

Serilog replaces the default `ILoggerFactory`. The OpenTelemetry logging
provider remains the single export path to Application Insights. Serilog
output (Console) and OTel export (Azure Monitor) are two sinks for the
same log stream — not two pipelines.

See `.claude/skills/observability-net/SKILL.md` for the exact wiring snippet.

### Phase D — Correlation middleware (15 min)

Create `Middleware/CorrelationIdMiddleware.cs`. Register before request logging
so the correlation ID is enriched into every log line.

### Phase E — Exception handling middleware (15 min)

Create `Middleware/ExceptionHandlingMiddleware.cs`. Returns:

```json
{ "error": "An unexpected error occurred.", "correlationId": "0HN..." }
```

No stack traces. Log the full exception with correlation ID server-side.

### Phase F — LLM telemetry in ClaudeChatModelProvider (30 min)

- Wrap the HTTP call in an `Activity` (distributed tracing)
- Tag with `llm.provider`, `llm.model`, `llm.tokens.input`, `llm.tokens.output`, `llm.latency_ms`
- Log structured event on success and failure
- Classify errors: 4xx (client), 5xx (transient), timeout, throttle

### Phase G — Resilience pipeline (20 min)

Replace ad-hoc HttpClient with named/typed client + standard resilience:

- 3 retries with jitter
- 30s attempt timeout
- Circuit breaker at 50% failure ratio over 30s
- Do NOT retry on 401/403

### Phase H — Local verification (15 min)

- `dotnet run`
- Hit `/api/ai/chat` 5x; check console logs for structured output + correlation IDs
- Force a failure (bad API key) → verify safe error response, full detail in logs

### Phase I — Deploy and verify (15 min)

Use `/deploy` slash command. Then:

- `GET /health` → 200
- `POST /api/ai/chat` → 200 with completion
- App Insights → Logs → run starter KQL queries (see kql.md)

### Phase J — Document (30 min)

- Write `ADR-006-adopt-serilog-with-application-insights-sink.md`
- Update `docs/architecture/day-006-observability-and-resilience.md` with new sequence diagram
- Fill out `02-completion-checklist.md`

## Architect Thinking

### Tradeoffs explicitly chosen

**Serilog over built-in `ILogger` only:** Built-in logging is fine for hello-world.
Serilog gives us first-class structured logging, sinks (App Insights, Seq, files),
and enrichers. The cost is one more dependency. Worth it for any system you'll
operate, not just demo.

**Workspace-based App Insights over classic:** Classic AI is being deprecated.
Workspace-based AI shares storage with Log Analytics, enables cross-resource
KQL, and is the path Azure is investing in. No reason to build new on classic.

**Microsoft.Extensions.Http.Resilience over raw Polly:** This package is the
modern wrapper Microsoft maintains. It bakes in best-practice defaults and
integrates with `IHttpClientFactory`. Choosing raw Polly means re-implementing
boilerplate. Choose the abstraction Microsoft owns.

**Correlation ID via middleware, not header parsing in controllers:** Cross-cutting
concern → middleware. If a controller has to know about correlation IDs to log,
the abstraction is wrong.

### Alternatives rejected

- **OpenTelemetry-first instead of Serilog+AppInsights:** OTel is the future and
  the right answer at scale. But for a single .NET service hitting one Azure
  backend, the App Insights SDK gives us 90% of the value with 20% of the wiring.
  Future ADR will migrate to OTel when we add a second service.
- **Console-only logging:** Fine in dev, useless in prod. App Service log streams
  rotate; you cannot run KQL on `Console.WriteLine`.
- **Returning exception messages to clients:** Common beginner mistake. Information
  disclosure risk + brittle client code that depends on internal text.

### What elite architects do differently

- They treat **token counts as a metric**, not a log field. (Metrics aggregate
  cheaply; logs do not.)
- They define **error CLASSES**, not error messages. "Provider transient",
  "provider auth", "client validation", "internal" — each routes to a different
  retry/alert/SLO bucket.
- They write the **runbook before the deploy**. "If circuit breaker opens, do X."

## Artifacts

### Code

- `src/lab-observability-api/Program.cs` (Serilog + App Insights + resilience)
- `src/lab-observability-api/Middleware/CorrelationIdMiddleware.cs`
- `src/lab-observability-api/Middleware/ExceptionHandlingMiddleware.cs`
- `src/lab-observability-api/Providers/ClaudeChatModelProvider.cs` (telemetry tags)

### Docs

- `docs/adr/ADR-006-adopt-serilog-with-application-insights-sink.md`
- `docs/architecture/day-006-observability-and-resilience.md`
- `docs/notes/Day-006/01-summary.md` (this file)
- `docs/notes/Day-006/02-completion-checklist.md`
- `docs/notes/Day-006/03-architect-thinking.md`
- `docs/notes/Day-006/kql.md`

### Infra

- `Infra/Day-006/appinsights.bicep` (optional but recommended for portfolio)
- `Infra/Day-006/appsettings-template.md` (updated with App Insights connection string)

## Portfolio Value

"Designed and implemented production observability for an enterprise AI gateway:
structured logging via Serilog, distributed tracing with W3C correlation,
LLM-specific cost telemetry (tokens, latency, model), and a Polly-based resilience
pipeline (retry with jitter, attempt timeout, circuit breaker). Wired to
workspace-based Application Insights with KQL dashboards for SLO tracking."

This is interview gold. You can speak to:

- Why workspace-based AI vs classic
- How retry jitter prevents thundering herd
- Why token cost is a metric, not a log
- How circuit breakers protect upstream

## Completion Checklist

See `02-completion-checklist.md`.

## Certification Reinforcement

### AZ-900 — **Secondary**

Concepts surfaced: Azure Monitor service family, Log Analytics, App Insights as
a category of Azure monitoring. Worth a 10-min review of the "Manage and govern
Azure resources" domain section on monitoring tools.

### AZ-104 — **Primary** (good entry point as the parallel track begins ~Day 10–15)

Concepts directly exercised:

- Configuring Application Insights for App Service (Monitor & maintain Azure resources domain)
- Diagnostic settings and Log Analytics workspaces
- App Service application settings and connection strings
- Resource group scoping for monitoring resources
**Note for cert:** AZ-104 expects you to know how to configure these via portal,
CLI, AND ARM/Bicep. We do CLI today; do the portal walkthrough as a study session.

### AZ-305 — **Secondary**

Concepts touched:

- Designing for monitoring and alerting (Design infrastructure solutions domain)
- Reliability patterns: retry, circuit breaker, timeout (Design business continuity)
- Cost optimization through telemetry (Design data storage / governance)
**Architect-level framing:** AZ-305 doesn't ask you to write the Polly code; it
asks you to choose the pattern. Today's work IS the chosen pattern.

### AI-102 — **Primary**

Concepts directly exercised:

- Monitoring Azure AI services (Monitor and optimize AI solutions domain)
- Implementing logging and diagnostics for AI workloads
- Managing costs of AI solutions (token telemetry maps directly)
**Note for cert:** AI-102 cares about Azure AI services specifically (Azure OpenAI,
Cognitive Services). Our gateway calls Anthropic today, but the OBSERVABILITY
PATTERNS are identical. When we add Azure OpenAI as a second provider (future
day), this same telemetry layer covers AI-102 monitoring objectives directly.

## Architect Posture Check

Fill `04-posture-check.md` at the END of the day, BEFORE marking complete.
Four questions:

1. Whose problem did I actually solve today?
2. What would I refuse to ship if I were the only one in the room?
3. What did I try, fail at, and learn? (Add to the Graveyard.)
4. Can I explain this at both a 10-year-old level AND a doctorate level?

## Parking Lot (do not do today)

- Migrate to OpenTelemetry (future ADR when we add service #2)
- Add prompt caching to ClaudeChatModelProvider (separate day)
- Bicep-ify App Insights provisioning (do once IaC track starts)
- Define formal SLOs and alerts (Day 7 candidate)
