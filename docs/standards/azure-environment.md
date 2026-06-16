# Azure Lab Environment

## Identity

| Field | Value |
|---|---|
| Subscription | `gio-architecture-lab` |
| Subscription ID | `6b7c3ded-aa98-43aa-8c19-fbf1b15a6920` |
| Resource Group | `rg-ai-lab-dev-eastus` |
| Region | East US |
| Budget | `lab-monthly-limit` ($50/month) |

## Purpose

Single-environment lab hosting the AI Gateway (`app-ai-lab-api-dev-eastus-gio`) and its supporting infrastructure. Intentionally simple — one resource group, one app, one Application Insights. Complexity scales with the roadmap, not ahead of it.

## Cost Governance

### Budget

- Name: lab-monthly-limit
- Amount: $50/month
- Alerts:
  - 50% (email)
  - 75% (email + action group)
  - 90% (email + action group)

### Action Group

- Name: ag-cost-alerts
- Notifications:
  - Email alerts
  - SMS alerts

## App Service Application Settings

The deployed App Service `app-ai-lab-api-dev-eastus-gio` uses the following
environment variables (set as App Service configuration, NOT in appsettings.json):

### Anthropic provider configuration

- `Anthropic__ApiKey`
- `Anthropic__Model` (`claude-sonnet-4-6` — updated Day 7; was `claude-opus-4-6` — invalid model ID discovered via silent cache miss)
- `Anthropic__BaseUrl` (`https://api.anthropic.com/v1`)
- `Anthropic__MaxTokens`
- `Anthropic__EnablePromptCaching` (`true` — added Day 7)
- `Anthropic__SystemPrompt` (6920-char / ≈1490-token operational system prompt — added Day 7)

### Observability (Day 6+)

- `APPLICATIONINSIGHTS_CONNECTION_STRING` (SDK auto-discovery key)
- `ApplicationInsights__ConnectionString` (IConfiguration-style key for OpenTelemetry exporter)

### Deployment

- `WEBSITE_RUN_FROM_PACKAGE=1`

## Monitoring resources

Application Insights:

- Resource: `appi-ai-lab-api-dev-eastus-gio` (workspace-based)
- Workspace: `law-ai-lab-dev-eastus-gio`

Action Group (AI gateway alerts):

- Name: `ag-ai-lab-dev-eastus-gio`
- Receivers: `gio.carozza@outlook.com`
- Note: separate from `ag-cost-alerts` (budget alerts)

Alert Rule:

- Name: `alert-ai-gateway-5xx-rate-dev-eastus-gio`
- Condition: `avg(failureRate) > 5%` over any 5-min window
- KQL: `requests` table — `failures / total * 100`; zero-traffic safe
- Severity: 2 (Warning)
- Bicep: `Infra/Day-006/appinsights.bicep`

Validate telemetry in: Application Insights → Transaction Search,
Failures, Live Metrics, Logs (KQL cookbook: `docs/standards/kql-cookbook.md`).

## Platform Resource Roadmap (future — not yet provisioned)

Resources added as training reaches the relevant day.
All naming follows `docs/standards/naming-conventions.md`.

### Planned resources and target days

| Day | Resource |
|---|---|
| Day 012 | Azure SQL or PostgreSQL — tenant data store |
| Day 018 | Azure Key Vault — secrets for all sites and gateway |
| Day 021 | Azure AI Search — vector index for RAG |
| Day 061 | Azure AD B2C or custom identity backend |
| Day 067 | Bicep IaC for the full platform |
| Day 068 | CI/CD pipeline (GitHub Actions) |
| Day 071+ | App Service instances for Security/Identity site |
| Day 101+ | App Service instances for Case Management app |
| Day 131+ | App Service instances for Admin site |

### Unified logging architecture (Day 064)

One Log Analytics workspace shared across all sites and the gateway.
All sites emit to the same workspace with consistent correlation IDs.
KQL queries span gateway + case management + identity audit in one
statement. This is the single observability pane for the platform.

### Multi-tenant naming pattern (when provisioned)

- Platform-level: `rg-ai-platform-{env}-eastus`
- Per-tenant (if isolated): `rg-ai-tenant-{id}-{env}-eastus`
- All globally-unique resources retain the `-gio` ownership suffix.
