# Concepts — Monitor and maintain Azure resources

## Day 007 additions (2026-06-03)
*Topics: KQL against Log Analytics / App Insights, custom dimensions on dependency spans, alert rules for operational metrics*

---

## KQL (Kusto Query Language) for Azure Monitor

### If you're 10 years old
Imagine your app writes a diary every time it does something — every request, every error, every slow moment. KQL is the way you ask questions about that diary. Instead of reading every page yourself, you write a question like "show me every time it took more than 3 seconds" and the diary answers instantly, even if it has millions of pages.

### If you're a CEO
KQL is how your operations team answers the question "what is the AI doing and what is it costing?" without calling a developer. It runs against your App Insights data in real time. The operations team you hire needs to know KQL to be effective on Azure — it's the operational literacy test for Azure-native engineering. If your team can't write KQL, they can't diagnose incidents or prove the AI is working correctly.

### If you're an Engineer
KQL uses a pipe syntax: each `|` passes a result table to the next operator. Key operators: `where` (filter rows), `summarize` (aggregate), `project` (select columns), `extend` (add columns), `render` (visualize). For AI gateway work: `dependencies | where name == "claude.chat.api" | where timestamp > ago(1h) | summarize p95=percentile(duration,95), errorRate=countif(success==false)*100.0/count() by bin(timestamp,5m)`. Custom dimensions need `toint()` or `tostring()` casts. Access them with bracket notation: `customDimensions["llm.tokens.input"]`, not dot notation (dot notation fails for keys containing dots). Practice in Log Analytics query explorer — it has IntelliSense and a result preview.

### If you're an Architect
KQL is the query language for Azure Monitor's Log Analytics backend, which underpins Application Insights, Azure Sentinel, and Azure Monitor Logs. It uses a pipe-based syntax where each operator transforms a tabular result:

```kql
dependencies
| where name == "claude.chat.api"
| where timestamp > ago(1h)
| summarize count() by bin(timestamp, 5m)
| render timechart
```

Key tables for AI gateway work:
- `requests` — inbound HTTP requests to the app
- `dependencies` — outbound calls (LLM API calls land here as custom Activity spans)
- `traces` — structured log messages from ILogger
- `exceptions` — unhandled exceptions with stack traces

Custom dimensions surfaced via OpenTelemetry Activity tags (e.g., `llm.cache.read_tokens`, `llm.tokens.input`) are queryable as `customDimensions["key"]`. This is the bridge between application telemetry and operational analytics.

**Why it matters in enterprise:** KQL is the lingua franca of Azure operations. AZ-104 tests the ability to write and interpret KQL for monitoring scenarios. The exam expects you to know which table to query, how to filter by time, how to aggregate, and how to set up alerts based on query results.

**Common beginner mistake:** Querying the `traces` table for dependency spans. Custom Activity spans (created via `ActivitySource.StartActivity`) land in the `dependencies` table in App Insights, not `traces`. Traces are for `ILogger` output. The wrong table returns empty results with no error.

---

## Azure Monitor Alert Rules

### If you're 10 years old
Your app is like a factory. You want a siren to go off if the factory slows down or starts making mistakes. Azure Monitor alert rules are automatic sirens — you tell them "if more than 5% of requests fail, sound the alarm," and they watch the logs 24/7 so you don't have to.

### If you're a CEO
Alert rules mean your team finds out about problems in minutes, not when a customer complains. Without them, incidents are discovered reactively — after users are already impacted. With them, the on-call engineer gets a text at 3 AM, diagnoses the issue, and fixes it before business hours. For AI products, you also want cost alerts — an alert that fires when AI spend trends above threshold, before the monthly invoice surprises you. This is table stakes for any production AI deployment.

### If you're an Engineer
Alert rule anatomy: Scope (Log Analytics workspace or App Insights resource) + Signal (KQL query or metric) + Condition (threshold + aggregation + evaluation window + frequency) + Action Group (who gets notified). KQL-based alerts run on a schedule (e.g., every 5 minutes) and evaluate the query result against the threshold. Metric-based alerts are near-real-time. Create the Action Group first — an alert rule with no action group fires silently and increments the portal counter but notifies nobody. For AI gateways: create alerts for 5xx rate > 5%, p95 latency > 5s, and cache hit rate < 10% during business hours.

### If you're an Architect
Alert rules in Azure Monitor consist of three parts:
1. **Signal** — a KQL query or metric that produces a numeric result
2. **Condition** — a threshold applied to that signal (e.g., `> 5`)
3. **Action group** — who gets notified and how (email, SMS, webhook, Logic App)

For Log Analytics-based alerts (scheduled query alerts), the rule runs a KQL query on a defined frequency (e.g., every 5 minutes) and evaluates the result against the threshold. For metric-based alerts, Azure evaluates the metric stream directly without a KQL query.

**Alert rule anatomy (KQL-based):**
- **Scope:** Log Analytics workspace or App Insights resource
- **Condition:** query + aggregation + threshold + evaluation window
- **Frequency:** how often the condition is checked
- **Action group:** the notification target (must be pre-created)
- **Severity:** 0 (Critical) → 4 (Verbose)

Day 6 shipped `alert-ai-gateway-5xx-rate-dev-eastus-gio` at severity 2, firing when `avg(failureRate) > 5%` over a 5-minute window. Day 7 adds `ai.provider.cache.hits` and `ai.provider.cache.misses` as counter metrics — candidates for a future cache-degradation alert.

**Why it matters in enterprise:** Alerts are the difference between discovering a problem in real time and discovering it on the monthly invoice. AZ-104 tests both the alert rule configuration and the action group setup.

**Common beginner mistake:** Creating an alert rule without first creating an action group. An alert rule with no action group fires silently — it increments the fired count in the portal but nobody is notified. Action groups must exist before or alongside the alert rule.

---

## Application Insights Tables and the Dependency Span Pattern

### If you're 10 years old
Your app talks to many other services — like a restaurant calling its suppliers. Application Insights tracks each of those calls in a special list called "dependencies." Every time your app calls the AI service, it gets written in that list: when it happened, how long it took, whether it worked. You can then search the list to find slow calls or failures.

### If you're a CEO
Application Insights gives you a complete picture of every call your application makes — to the AI model, to the database, to external APIs. When performance degrades or costs spike, the operations team can pinpoint exactly which call is responsible: how long it took, what it cost in tokens, and whether it succeeded. This is the difference between "the app was slow" and "the AI call on request #4821 took 12 seconds because of a specific prompt pattern."

### If you're an Engineer
The `dependencies` table captures all outbound calls from the app. OpenTelemetry `Activity` spans (created via `ActivitySource.StartActivity`) export as dependency records when they are child spans of a request span. Access custom tags as `customDimensions["key"]` — bracket notation required for keys with dots (like `llm.tokens.input`). The `toint()` cast converts string custom dimensions to integers for arithmetic: `toint(customDimensions["llm.tokens.input"])`. Correlate spans across a request using `operation_Id` (the W3C trace ID). Common error: creating an Activity that isn't a child of the request span — it won't appear correlated to the inbound request in the trace view.

### If you're an Architect
In Application Insights (backed by Log Analytics), outbound calls are modeled as dependency telemetry. The OpenTelemetry SDK exports `Activity` spans as dependency records when they are child spans of a request span. The `dependencies` table schema includes:

| Column | Content |
|---|---|
| `name` | Activity name (e.g., `claude.chat.api`) |
| `duration` | Span duration in milliseconds |
| `success` | Whether the dependency call succeeded |
| `customDimensions` | Key-value bag from Activity tags |
| `operation_Id` | W3C trace ID for cross-span correlation |
| `timestamp` | When the span ended |

Custom Activity tags set via `activity.SetTag("llm.tokens.input", value)` appear in `customDimensions` and are queryable via `toint(customDimensions["llm.tokens.input"])`. The `toint()` cast is necessary because all custom dimensions are stored as strings.

**Why it matters in enterprise:** Understanding which table holds which telemetry is an AZ-104 exam skill and a day-1 operational skill. Engineers who query the wrong table waste time and draw wrong conclusions. The dependency → custom dimensions → `toint()` pattern is the standard way to surface numeric metrics from structured traces.

**Common beginner mistake:** Using `customDimensions.key` (dot notation) instead of `customDimensions["key"]` (bracket notation) in KQL. Dot notation fails for keys with dots or hyphens in the name (like `llm.cache.read_tokens`). Always use bracket notation for custom dimensions with non-identifier characters.

---
