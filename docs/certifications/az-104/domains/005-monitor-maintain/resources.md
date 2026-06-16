# Resources — Monitor and maintain Azure resources

## Day 007 additions (2026-06-03)

*Focus: KQL for App Insights, alert rules, custom dimensions*

---

## Official Microsoft Resources

- [Log Analytics tutorial](https://learn.microsoft.com/en-us/azure/azure-monitor/logs/log-analytics-tutorial) — hands-on walkthrough of KQL query writing in the Azure portal
- [KQL quick reference](https://learn.microsoft.com/en-us/azure/data-explorer/kql-quick-reference) — syntax cheat sheet; operators, aggregations, scalar functions
- [Application Insights data model](https://learn.microsoft.com/en-us/azure/azure-monitor/app/data-model-complete) — canonical reference for `requests`, `dependencies`, `traces`, `exceptions` table schemas and column definitions
- [Create a log search alert](https://learn.microsoft.com/en-us/azure/azure-monitor/alerts/alerts-create-log-alert-rule) — step-by-step for scheduled query alert rules including action group attachment
- [Action groups](https://learn.microsoft.com/en-us/azure/azure-monitor/alerts/action-groups) — how to create and configure notification targets for alert rules
- [OpenTelemetry custom dimensions in App Insights](https://learn.microsoft.com/en-us/azure/azure-monitor/app/opentelemetry-add-modify) — how Activity tags become `customDimensions` entries in the `dependencies` table

## Diagrams and Visual References

- [Azure Monitor data platform overview](https://learn.microsoft.com/en-us/azure/azure-monitor/data-platform) — MS Learn; diagram showing how metrics, logs, and traces flow through the platform
- [App Insights table relationships](https://learn.microsoft.com/en-us/azure/azure-monitor/app/data-model-complete#table-relationships) — how `operation_Id` links records across `requests`, `dependencies`, `traces`, `exceptions`

## Video (≤ 20 min)

- [AZ-104 Exam Readiness: Monitor and maintain Azure resources](https://learn.microsoft.com/en-us/shows/exam-readiness-zone/preparing-for-az-104-monitor-and-maintain-azure-resources-5-of-5) — Exam Readiness Zone, ~12 min, covers alert rules, Azure Monitor, and Log Analytics for AZ-104 Domain 5
- [KQL for beginners](https://www.youtube.com/watch?v=Pl8n6GaWEo0) — Azure Fridays, ~18 min, covers the foundational KQL operators used in AZ-104 exam questions

## Hands-on

- [KQL cookbook in this repo](../../../../standards/kql-cookbook.md) — Queries 1–9 covering latency, error rate, token usage, correlation tracing, cache hit rate, and estimated savings — all verified against live App Insights data
