# Concepts — Monitor and maintain Azure resources

## Day 007 additions (2026-06-03)
*Topics: KQL against Log Analytics / App Insights, custom dimensions on dependency spans, alert rules for operational metrics*

---

## KQL (Kusto Query Language) for Azure Monitor

### If you're 10 years old
Imagine your app writes a diary every time it does something — every request, every error, every slow moment. KQL is the way you ask questions about that diary. Instead of reading every page yourself, you write a question like "show me every time it took more than 3 seconds" and the diary answers instantly, even if it has millions of pages.

### If you're an architect
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

### If you're an architect
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

### If you're an architect
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
