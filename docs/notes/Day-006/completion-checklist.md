# Day 6 — Completion Checklist

## Infrastructure
- [ ] Verified existing App Insights `appi-ai-lab-api-dev-eastus-gio` is workspace-based (or migrated if classic)
- [ ] Log Analytics workspace `law-ai-lab-dev-eastus-gio` created (if not present)
- [ ] Workspace-based Application Insights confirmed (workspaceResourceId is non-null)
- [ ] `APPLICATIONINSIGHTS_CONNECTION_STRING` set on App Service
- [ ] Connection string verified via `az webapp config appsettings list`
- [ ] `Anthropic__Model` updated to `claude-opus-4-7` on App Service

## Code — Packages
- [ ] `Serilog.AspNetCore` added to .csproj
- [ ] `Serilog.Sinks.ApplicationInsights` added
- [ ] `Microsoft.ApplicationInsights.AspNetCore` added
- [ ] `Microsoft.Extensions.Http.Resilience` added
- [ ] `dotnet restore` succeeds

## Code — Wiring
- [ ] Serilog configured in `Program.cs` with Console + AppInsights sinks
- [ ] `UseSerilogRequestLogging` enabled with correlation ID enrichment
- [ ] `CorrelationIdMiddleware` created and registered (before request logging)
- [ ] `ExceptionHandlingMiddleware` created and registered (first in pipeline)
- [ ] `IHttpClientFactory` typed client for `ClaudeChatModelProvider`
- [ ] `AddStandardResilienceHandler` configured with retry, timeout, circuit breaker
- [ ] `Activity` instrumentation in provider with `llm.*` tags

## Behavior — Local
- [ ] `dotnet run` starts without errors
- [ ] `POST /api/ai/chat` returns 200 with completion
- [ ] Console shows structured log lines with `CorrelationId`
- [ ] `X-Correlation-Id` request header is honored (echoed in response header)
- [ ] Forced bad API key returns safe error JSON (no stack trace)
- [ ] Forced bad API key logs full exception server-side with correlation ID
- [ ] Token counts visible in logs (`llm.tokens.input`, `llm.tokens.output`)

## Behavior — Deployed
- [ ] Successful deploy via `/deploy` slash command (Kudu zip path)
- [ ] `GET /health` returns 200 from Azure
- [ ] `POST /api/ai/chat` returns 200 from Azure
- [ ] App Insights → Logs shows requests within 2 minutes
- [ ] KQL query for slowest requests returns results
- [ ] KQL query for token usage returns results

## Documentation
- [ ] `ADR-006-adopt-serilog-with-application-insights-sink.md` written and Accepted
- [ ] `docs/architecture/day-006-observability-and-resilience.md` includes updated sequence diagram
- [ ] `docs/notes/Day-006/kql.md` contains at least 5 starter KQL queries
- [ ] `docs/notes/Day-006/architect-thinking.md` written
- [ ] `Infra/Day-006/appsettings-template.md` includes APPLICATIONINSIGHTS_CONNECTION_STRING
- [ ] Root `CLAUDE.md` "What I'm Building Toward" section reflects Day 6 completion
- [ ] Git commit: `feat(day-006): observability and resilience for AI gateway`

## Certification
- [ ] Read AI-102 "Monitor and optimize AI solutions" objective list (15 min)
- [ ] Read AZ-104 "Monitor and back up Azure resources" objective list (15 min)
- [ ] Note 3 questions from each that today's work directly answers
- [ ] File notes in `docs/certifications/ai-102/study-notes/day-006-mapping.md`

## Stretch (optional, only if base is complete)
- [ ] Add Bicep template for App Insights in `Infra/Day-006/appinsights.bicep`
- [ ] Define an Azure Monitor alert rule for 5xx rate > 5% over 5 min
- [ ] Add a Workbook or pinned Dashboard with the starter KQL queries