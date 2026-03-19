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