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
- `Anthropic__Model` (e.g., `claude-opus-4-6`)
- `Anthropic__BaseUrl` (`https://api.anthropic.com/v1`)
- `Anthropic__MaxTokens`

### Observability (Day 6+)
- `APPLICATIONINSIGHTS_CONNECTION_STRING`

### Deployment
- `WEBSITE_RUN_FROM_PACKAGE=1`

## Monitoring resources

Application Insights:
- Resource: `appi-ai-lab-api-dev-eastus-gio` (workspace-based)
- Workspace: `law-ai-lab-dev-eastus-gio`

Validate telemetry in: Application Insights → Transaction Search,
Failures, Live Metrics, Logs.  