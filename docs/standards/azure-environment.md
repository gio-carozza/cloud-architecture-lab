# Azure Lab Environment

Subscription
gio-architecture-lab

Resource Group
rg-ai-lab-dev-eastus

Budget
lab-monthly-limit

Region
East US

## Purpose

This environment hosts experimental workloads used during the architecture learning roadmap.

The goal is to maintain a controlled cloud environment where services can be deployed and tested while maintaining cost awareness.

## Notes

The environment is intentionally simple during early learning phases.

Future iterations may include more advanced architecture patterns.

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
- `Anthropic__Model` (`claude-sonnet-4-6` — updated Day 7; was `claude-opus-4-7`)
- `Anthropic__BaseUrl` (`https://api.anthropic.com/v1`)
- `Anthropic__MaxTokens`
- `Anthropic__EnablePromptCaching` (`true` — added Day 7; pending portal apply)
- `Anthropic__SystemPrompt` (≥1024-token operational prompt — added Day 7; pending portal apply)

### Observability (Day 6+)
- `APPLICATIONINSIGHTS_CONNECTION_STRING`

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