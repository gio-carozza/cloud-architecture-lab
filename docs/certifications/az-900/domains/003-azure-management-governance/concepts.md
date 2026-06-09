# Concepts — Azure Management and Governance (AZ-900 Domain 3)

---

## Azure Monitor

### If you're 10 years old
Imagine your house has sensors everywhere — one on the furnace, one on each door, one on the water heater. Azure Monitor is like a control panel that shows you all those sensors at once. If the furnace gets too hot, it sends you a text message. If the water heater breaks, it shows you a graph of when it started going wrong. Azure Monitor does the same thing for all your cloud apps and services — it watches them, records what happens, and tells you when something needs attention.

### If you're a CEO
Azure Monitor is Microsoft's built-in operations center for every Azure resource you own. It answers "is it working, how fast is it, and what does it cost?" in a single pane — without buying additional tooling. The alternative is finding out about problems from customers, which is always more expensive than a monitoring alert.

### If you're an Engineer
Azure Monitor collects telemetry from Azure resources (automatically) and from your applications (via SDKs). Two main data types: **Metrics** (numeric, time-series, stored 93 days — e.g., CPU%, request count, custom histogram values) and **Logs** (structured text, stored in Log Analytics workspace, queried via KQL). Key integrations: `Azure.Monitor.OpenTelemetry.AspNetCore` NuGet package exports .NET OpenTelemetry traces/metrics/logs directly to App Insights (which feeds Azure Monitor). Alert rules fire on metric thresholds or KQL log queries. Action Groups route alerts to email, SMS, or webhooks.

### If you're an Architect
Azure Monitor is the observability backbone for every Azure workload. Design-time decisions: (1) workspace-based Application Insights (not classic) is the current standard — it stores data in a Log Analytics workspace, enabling cross-resource KQL queries; (2) metrics and logs have different retention, cost, and query semantics — use metrics for SLO thresholds and alerting; logs for root-cause analysis; (3) custom metrics from OpenTelemetry SDK (`Histogram<T>`, `Counter<T>`) surface in the `customMetrics` table and support percentile queries — essential for latency SLOs; (4) alert rules on scheduled KQL queries (not just metric thresholds) enable complex conditions like "failure rate > 5% only when traffic > 10 req/min." Common beginner mistake: relying on average latency for alerts instead of p95 — averages hide the tail experiences that cause customer complaints.

---

## Azure Monitor Custom Metrics and Histograms

### If you're 10 years old
Regular Azure metrics track things Azure already knows about — like how much CPU your computer uses. Custom metrics are things YOU teach Azure to measure. If you build a game and want Azure to track how long players wait before they hear the first sound, you can add that measurement yourself. Azure will store it, show it on a chart, and even send you an alert if the wait gets too long.

### If you're a CEO
Custom metrics let your team measure what actually matters to the business, not just what the infrastructure tracks by default. A 50ms improvement in time-to-first-response may not show up in any Azure default metric — but if you track it as a custom metric, you can put an SLA on it and monitor it like any other operational commitment.

### If you're an Engineer
Custom metrics are emitted via the OpenTelemetry `Meter` API: `Meter.CreateHistogram<double>("metric.name")` then `.Record(value, tags)`. In .NET with `Azure.Monitor.OpenTelemetry.AspNetCore`, these flow into the `customMetrics` table in Application Insights automatically. Query with KQL: `customMetrics | where name == "ai.provider.stream.ttft_ms" | summarize p95=percentile(value, 95) by bin(timestamp, 5m)`. Histogram instruments (`Histogram<T>`) store individual sample values, enabling percentile queries (p50/p95/p99). Counter instruments store cumulative counts. Tags (key-value pairs on `Record()`) become `customDimensions` columns — use them for slicing by model, provider, or region. Retention: 90 days default in workspace-based App Insights.

### If you're an Architect
Custom metrics are the bridge between what Azure monitors by default and what your SLOs actually require. Three architectural considerations: (1) histogram vs. counter vs. gauge — histograms are the right instrument for latency (gives you percentiles); counters for event rates (cache hits/misses); gauges for current-state values (queue depth). Choosing the wrong instrument type produces data you can't query for SLO compliance. (2) metric names should follow a consistent namespace (`ai.provider.*` or `ai.chat.stream.*`) — ad hoc naming makes cross-service dashboards expensive to maintain; (3) custom metrics cost ingestion compute — emit at appropriate granularity; avoid emitting a metric per token in a streaming response when one metric per request is sufficient. Common beginner mistake: using Log Analytics KQL queries as the primary alert signal for latency — logs have 2–5 minute ingestion delay, metrics have ~1 minute; use metrics for alerting, logs for investigation.

---

## Application Insights — Workspace-Based vs. Classic

<!-- Day 6 Additions -->

### If you're 10 years old
Think of your app's telemetry data like letters. Classic Application Insights stores letters in its own private mailbox. Workspace-based Application Insights puts them in a shared filing cabinet (Log Analytics) so all your apps' letters are in the same place — making it much easier to find things that happened across multiple apps at the same time.

### If you're a CEO
Microsoft is retiring the "classic" version of Application Insights. Workspace-based App Insights stores all telemetry in a shared Log Analytics workspace, which enables a single pane of glass across all monitored services. Building on classic today means a mandatory migration tomorrow; workspace-based is the correct foundation for any new Azure monitoring investment.

### If you're an Engineer
Workspace-based Application Insights requires a Log Analytics workspace as its backing store. Create the workspace first (`az monitor log-analytics workspace create`), then create App Insights referencing it (`--workspace` flag). Configure the application with the `APPLICATIONINSIGHTS_CONNECTION_STRING` environment variable — not the older instrumentation key. Connection strings include the ingestion endpoint explicitly and support sovereign cloud routing. In .NET 8: install `Azure.Monitor.OpenTelemetry.AspNetCore` and call `services.AddOpenTelemetry().UseAzureMonitor()`. All OTel traces, metrics, and logs flow to the workspace automatically.

### If you're an Architect
The shift from classic to workspace-based Application Insights is a structural change, not just a feature upgrade. Key architectural implications: (1) data lands in a **Log Analytics workspace** — the same storage engine as all other Azure resource logs — enabling `union` queries across App Insights tables (`requests`, `dependencies`, `traces`) and ARM activity logs in a single KQL statement; (2) data retention, access control (RBAC), and export are all configured on the workspace, not on the App Insights component; (3) connection strings contain the ingestion endpoint explicitly, making them compatible with sovereign clouds and Private Link setups where instrumentation keys are insufficient. At enterprise scale, one workspace per environment (dev/staging/prod) with all application components forwarding to it is the standard topology — it enables cross-service incident correlation without needing multiple dashboards. Common beginner mistake: provisioning App Insights without a workspace in new deployments, which creates a classic resource that Microsoft will eventually force-migrate.

---

## Log Analytics Workspace

<!-- Day 6 Additions -->

### If you're 10 years old
Imagine a massive library where all the diaries from every computer and app in your company are collected. Log Analytics workspace is that library. You can search through all the diaries using KQL (a search language), find out exactly when something went wrong, and answer questions like "which app had the most errors today?" Even if you have 20 apps, they all write to the same library.

### If you're a CEO
Log Analytics workspace is where Azure centralises all diagnostic and application data. It is the single source of truth for operational questions — "is it working?", "when did it break?", "what changed 10 minutes before the outage?" Every hour your team spends guessing what went wrong in production is an hour that proper log analytics access would have resolved in minutes.

### If you're an Engineer
A Log Analytics workspace is a data store identified by a workspace ID. All resources that emit diagnostic logs can forward to it via Diagnostic Settings. App Insights workspace-based mode stores `requests`, `dependencies`, `traces`, `exceptions`, `customEvents`, `customMetrics` tables in the workspace. Query via KQL in the Azure portal or via `az monitor log-analytics query --workspace <id> --analytics-query "..."`. Data retention default: 30 days free, configurable up to 730 days (at cost). Archive tier extends to 12 years. Common KQL pattern for AI gateway: `customEvents | where customDimensions["llm.provider"] == "anthropic" | summarize avg(toreal(customDimensions["llm.tokens.total"])) by bin(timestamp, 1h)`.

### If you're an Architect
Log Analytics workspace is the centralised log store in Azure's observability architecture. Its position in the hierarchy: Azure Monitor → Log Analytics workspace ← Application Insights (workspace-based), Diagnostic Settings (all resource types). Architectural decisions at the workspace level: (1) one workspace per environment (dev/staging/prod) vs. one global workspace — the former simplifies RBAC and prevents dev noise from obscuring prod alerts; the latter reduces cost and enables cross-env queries; (2) data retention cost is workspace-level — shorter retention for high-volume verbose logs, longer for audit/security logs; (3) Log Analytics workspaces support commitment tiers (100GB–5000GB/day) that discount significantly over pay-as-you-go at scale. The AZ-305 exam tests whether you can recommend the correct workspace topology given a scenario's RBAC, cost, and compliance requirements. Common beginner mistake: forwarding all diagnostic logs to a single workspace with no retention policy, then discovering a $5,000/month log ingestion bill.

---

## Azure App Service

### If you're 10 years old
App Service is like renting a ready-made shop space in a shopping mall. You bring your product (your app), and Microsoft provides the building, the heating, the security guard, and the internet connection. You don't have to worry about any of that — you just focus on what your shop sells. And if your shop gets really busy, Microsoft can automatically open more copies of your shop without you having to do anything.

### If you're a CEO
App Service is the fastest path from "we have an app" to "the app is live on the internet" in Azure. It handles scaling, patching, SSL certificates, and deployment infrastructure automatically. The team focuses on the application; Microsoft manages the platform. The risk of not using it is building that plumbing yourself — which is slower and more error-prone for most web workloads.

### If you're an Engineer
App Service hosts web apps, REST APIs, and background services on managed infrastructure (Windows or Linux). Key features: deployment slots (staging/production swap with no downtime), automatic scaling (scale out rules based on metrics), built-in SSL, custom domains, and GitHub Actions / Kudu deployment integration. For .NET 8 APIs: publish with `dotnet publish -c Release`, zip the output, deploy via Kudu publish API (`POST /api/publish?type=zip`). Set `WEBSITE_RUN_FROM_PACKAGE=1` to run from a zip without extraction — faster cold start. App settings (environment variables) use double underscore (`Anthropic__ApiKey`) which ASP.NET Core maps to `Anthropic:ApiKey` in `IConfiguration`. Linux tiers use nginx as a reverse proxy — for SSE streaming, set `X-Accel-Buffering: no` to prevent nginx from buffering the response.

### If you're an Architect
App Service sits at the intersection of developer velocity and operational simplicity. The architectural tradeoff: App Service reduces infrastructure overhead at the cost of less control over the underlying compute and network. For an AI gateway, the key constraints are: (1) cold start latency on consumption tiers (use Always-On on Basic+ tiers for interactive workloads); (2) the nginx reverse proxy — beneficial for TLS termination and load balancing, but requires explicit configuration for SSE streaming; (3) outbound IP addresses are shared on Basic tier, dedicated on Standard+ — matters for IP-allowlisted upstream APIs; (4) deployment slots enable zero-downtime deploys with pre-warm capability. Common beginner mistake: deploying from `bin/Debug` instead of `dotnet publish` output — they are structurally different and the debug build includes PDBs and dev-only files that inflate package size and cold start time.

---
