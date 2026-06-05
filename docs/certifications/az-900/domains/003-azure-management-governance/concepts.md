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
Custom metrics are emitted via the OpenTelemetry `Meter` API: `Meter.CreateHistogram<double>("metric.name")` then `.Record(value, tags)`. In .NET with `Azure.Monitor.OpenTelemetry.AspNetCore`, these flow into the `customMetrics` table in Application Insights automatically. Query with KQL: `customMetrics | where name == "ai.chat.stream.ttft_ms" | summarize p95=percentile(value, 95) by bin(timestamp, 5m)`. Histogram instruments (`Histogram<T>`) store individual sample values, enabling percentile queries (p50/p95/p99). Counter instruments store cumulative counts. Tags (key-value pairs on `Record()`) become `customDimensions` columns — use them for slicing by model, provider, or region. Retention: 90 days default in workspace-based App Insights.

### If you're an Architect
Custom metrics are the bridge between what Azure monitors by default and what your SLOs actually require. Three architectural considerations: (1) histogram vs. counter vs. gauge — histograms are the right instrument for latency (gives you percentiles); counters for event rates (cache hits/misses); gauges for current-state values (queue depth). Choosing the wrong instrument type produces data you can't query for SLO compliance. (2) metric names should follow a consistent namespace (`ai.provider.*` or `ai.chat.stream.*`) — ad hoc naming makes cross-service dashboards expensive to maintain; (3) custom metrics cost ingestion compute — emit at appropriate granularity; avoid emitting a metric per token in a streaming response when one metric per request is sufficient. Common beginner mistake: using Log Analytics KQL queries as the primary alert signal for latency — logs have 2–5 minute ingestion delay, metrics have ~1 minute; use metrics for alerting, logs for investigation.

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
