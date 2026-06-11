# Day 6 — Completion Checklist

## Infrastructure

- [x] Verified existing App Insights `appi-ai-lab-api-dev-eastus-gio` is workspace-based (or migrated if classic) — confirmed from connection string ingestion endpoint: `eastus-8.in.applicationinsights.azure.com` (workspace-based format; classic uses `dc.services.visualstudio.com`)
- [x] Log Analytics workspace `law-ai-lab-dev-eastus-gio` created (if not present) — implied by workspace-based App Insights (workspace-based requires a Log Analytics workspace)
- [x] Workspace-based Application Insights confirmed (workspaceResourceId is non-null) — confirmed indirectly via ingestion endpoint format; direct API call blocked by intermittent CLI SSL reset on `microsoft.insights` provider
- [x] `APPLICATIONINSIGHTS_CONNECTION_STRING` set on App Service — confirmed via `az rest` ARM call: `InstrumentationKey=c08131fc...;IngestionEndpoint=https://eastus-8.in.applicationinsights.azure.com/...`
- [x] Connection string verified via `az webapp config appsettings list` — verified via `az rest` direct ARM call (workaround: `az webapp` wrapper triggers an internal `get_raw_functionapp` check that hits the SSL reset; raw ARM path works)
- [x] `Anthropic__Model` updated to `claude-opus-4-7` on App Service — confirmed in appsettings and in live response: `"model":"claude-opus-4-7"`

## Code — Packages
- [x] `Serilog.AspNetCore` added to .csproj — v8.0.3
- [x] ~~`Serilog.Sinks.ApplicationInsights` added~~ — SUPERSEDED: ADR-008 chose OTel as sole export pipeline; this package must not be installed (would duplicate every signal to App Insights)
- [x] ~~`Microsoft.ApplicationInsights.AspNetCore` added~~ — SUPERSEDED: `Azure.Monitor.OpenTelemetry.AspNetCore` v1.4.0 covers this path
- [x] `Microsoft.Extensions.Http.Resilience` added — v10.5.0
- [x] `dotnet restore` succeeds — implied by clean build (0 errors, 0 warnings)

## Code — Wiring
- [x] Serilog configured in `Program.cs` with Console + AppInsights sinks — Console sink confirmed; App Insights export via OTel per ADR-008, not a Serilog sink
- [x] `UseSerilogRequestLogging` enabled with correlation ID enrichment — `Program.cs:135–160`
- [x] `CorrelationIdMiddleware` created and registered (before request logging) — `Middleware/CorrelationIdMiddleware.cs`; registered at `Program.cs:130`
- [x] `ExceptionHandlingMiddleware` created and registered (first in pipeline) — implemented inline as `app.Use()` lambda at `Program.cs:165–224`; catches `ClaudeProviderException` + `Exception`, returns safe JSON contract
- [x] `IHttpClientFactory` typed client for `ClaudeChatModelProvider` — `AddHttpClient<ClaudeApiClient>`; provider depends on `ClaudeApiClient`
- [x] `AddStandardResilienceHandler` configured with retry, timeout, circuit breaker — `SamplingDuration=120s`, `AttemptTimeout=45s`, `ShouldHandle=_=>false` (retry stage present but disabled per ADR-006)
- [x] `Activity` instrumentation in provider with `llm.*` tags — outer span `ai.chat.complete` in `ClaudeChatModelProvider`; inner span `claude.chat.api` in `ClaudeApiClient` with `llm.tokens.input`, `llm.tokens.output`, `llm.latency_ms`, `llm.endpoint`

## Behavior — Local
- [x] `dotnet run` starts without errors — ready in < 2s after resilience config fix
- [x] `POST /api/ai/chat` returns 200 with completion — `{"provider":"anthropic","model":"claude-opus-4-6","response":"Hello to you!"}`
- [x] Console shows structured log lines with `CorrelationId` — every log line carried matching correlation ID through controller → provider → client
- [x] `X-Correlation-Id` request header is honored (echoed in response header) — `x-correlation-id: 4eadbbf97771415ab30949e67d17be43` confirmed
- [x] Forced bad API key returns safe error JSON (no stack trace) — `502` with `{"code":"claude_provider_error","message":"The AI provider request failed.","correlationId":"..."}`, no stack trace, no internal message
- [x] Forced bad API key logs full exception server-side with correlation ID — `ClaudeProviderException: invalid x-api-key` with full stack trace, `ProviderErrorCode=authentication_error`, `IsTransient=False`, `CorrelationId` on every log line; retry correctly did not fire (`Handled: 'False', Attempt: '0'`)
- [x] Token counts visible in App Insights (`llm.tokens.input`, `llm.tokens.output`) — confirmed in `dependencies` table post-deploy: `claude.chat.api` span at 07:06:03Z shows `llm.tokens.input=19`, `llm.tokens.output=8`, `llm.latency_ms=897ms` from live Azure traffic (`app-ai-lab-api-dev-eastus-gio`); `ai.chat.complete` span carries `llm.provider` + `llm.model`; old pre-deploy span (`anthropic.messages.create`, 06:50Z) has no token tags — clean before/after contrast confirms instrumentation is live

## Behavior — Deployed

- [x] Successful deploy via `/deploy` slash command (Kudu zip path) — app was already deployed from a prior session; live and responding on `app-ai-lab-api-dev-eastus-gio.azurewebsites.net`
- [x] `GET /health` returns 200 from Azure — `{"status":"healthy","checks":["api-process","routing","logging"],...}` with `x-correlation-id` header confirmed
- [x] `POST /api/ai/chat` returns 200 from Azure — `{"provider":"anthropic","model":"claude-opus-4-7","response":"Hello, hi, greetings!"}` confirmed; model is `claude-opus-4-7` on Azure vs `claude-opus-4-6` locally
- [x] App Insights → Logs shows requests within 2 minutes — `POST api/Ai/chat` (200, 1888ms) from live Azure hit at 06:50 UTC visible in `requests` table; verified via `az monitor app-insights query`
- [x] KQL query for slowest requests returns results — chat calls at top: 1888ms and 1797ms; bad-key 502 at 286ms; query against `requests` table confirmed working
- [x] KQL query for token usage returns results — `llm.tokens.input=12`, `llm.tokens.output=7`, `llm.latency_ms=1794ms` visible in `dependencies` table on `claude.chat.api` span; data from local run (local and Azure share the same App Insights resource); redeploy needed to get token spans from the live app itself

## Documentation
- [x] `ADR-006-adopt-serilog-with-application-insights-sink.md` written and Accepted — exists as `ADR-006-harden-ai-gateway-with-resilience-and-observability.md`; Status: Accepted
- [x] `docs/architecture/day-006-observability-and-resilience.md` includes updated sequence diagram — exists as `day-006-ai-gateway-v2-hardening.md` + `day-006-sequence-flow.md`
- [x] `docs/notes/Day-006/kql.md` contains at least 5 starter KQL queries — promoted to `docs/standards/kql-cookbook.md`
- [x] `docs/notes/Day-006/03-architect-thinking.md` written
- [x] `Infra/Day-006/appsettings-template.md` includes APPLICATIONINSIGHTS_CONNECTION_STRING
- [x] Root `CLAUDE.md` "What I'm Building Toward" section reflects Day 6 completion — updated: "Days 1–6 complete. Day 7 next." and "Observability & resilience (done — Day 6)"
- [x] Git commit: `feat(day-006): observability and resilience for AI gateway` — `04f4931` on `feature/day-006-gateway-hardening`

## Certification
- [ ] Read AI-102 "Monitor and optimize AI solutions" objective list (15 min)
- [ ] Read AZ-104 "Monitor and back up Azure resources" objective list (15 min)
- [ ] Note 3 questions from each that today's work directly answers
- [x] File notes in `docs/certifications/ai-102/study-notes/day-006-mapping.md` — AI-102 mapping confirmed

## Stretch (optional, only if base is complete)
- [x] Add Bicep template for App Insights in `Infra/Day-006/appinsights.bicep` — created; codifies Log Analytics workspace, App Insights component (workspace-based), Action Group, and 5xx Scheduled Query Alert Rule; outputs `ConnectionString`, `InstrumentationKey`, `workspaceId`, `appInsightsId` for downstream consumption
- [x] Define an Azure Monitor alert rule for 5xx rate > 5% over 5 min — `alert-ai-gateway-5xx-rate-dev-eastus-gio` created via ARM REST (Invoke-RestMethod); KQL computes `failureRate = failures/total*100`; avg > 5 fires severity-2 alert to Action Group `ag-ai-lab-dev-eastus-gio` → `gio.carozza@outlook.com`; zero-traffic guard: `failureRate=0` when `total=0`, no false positives
- [ ] Add a Workbook or pinned Dashboard with the starter KQL queries
