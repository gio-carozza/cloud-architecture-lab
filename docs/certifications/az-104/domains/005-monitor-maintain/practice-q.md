# Practice Questions — Monitor and maintain Azure resources

## Day 007 additions (2026-06-03)
*5 questions — KQL, App Insights tables, alert rules, custom dimensions*

---

## Q1: Correct table for dependency telemetry

**Scenario:** Your team deployed an AI gateway that calls an external API using OpenTelemetry Activity spans. You want to write a KQL query to find all calls to the external API that took longer than 5 seconds in the last hour. A colleague suggests querying the `traces` table.

**Question:** Which table should you query, and why is your colleague's suggestion incorrect?

A) `requests` — because outbound calls are recorded as inbound request metadata
B) `dependencies` — because OpenTelemetry Activity spans for outbound calls land here
C) `traces` — because Activity spans are a form of structured trace
D) `exceptions` — because slow calls may raise timeout exceptions

**Answer:** B

**Why:** OpenTelemetry Activity spans exported to Application Insights appear in the `dependencies` table, not `traces`. The `traces` table contains `ILogger` output (structured log messages). The `requests` table contains inbound HTTP requests to the monitored app. The `exceptions` table contains unhandled exceptions. A colleague querying `traces` will get empty results for dependency data with no error — a common and confusing mistake.

**Exam domain:** Monitor and maintain Azure resources
**Cert:** AZ-104
**Roadmap day:** Day-007

---

## Q2: Custom dimension query syntax

**Scenario:** Your AI gateway emits a custom dimension `llm.cache.read_tokens` on each outbound API call span. You need to write a KQL query that returns only spans where this value is greater than zero.

**Question:** Which KQL expression correctly filters for spans where `llm.cache.read_tokens` is greater than zero?

A) `where customDimensions.llm.cache.read_tokens > 0`
B) `where toint(customDimensions["llm.cache.read_tokens"]) > 0`
C) `where customDimensions["llm.cache.read_tokens"] > 0`
D) `where tags["llm.cache.read_tokens"] > 0`

**Answer:** B

**Why:** Custom dimensions in App Insights are stored as strings regardless of their original type. Direct comparison with `> 0` (option C) will fail or produce incorrect results because string comparison is not numeric. The `toint()` function converts the string to an integer before comparison. Option A uses dot notation, which fails for keys containing dots. Option D references a non-existent `tags` column — the correct column name is `customDimensions`.

**Exam domain:** Monitor and maintain Azure resources
**Cert:** AZ-104
**Roadmap day:** Day-007

---

## Q3: Alert rule with no action group

**Scenario:** An Azure Monitor alert rule was configured to fire when the 5xx error rate exceeds 5% over a 5-minute window. After a production incident, the team discovers the alert fired 12 times during the outage but nobody received a notification.

**Question:** What is the most likely cause?

A) The alert rule severity was set too low (severity 4)
B) The alert rule was in a disabled state
C) No action group was attached to the alert rule
D) The Log Analytics workspace was in a different region than the App Service

**Answer:** C

**Why:** An alert rule without an action group evaluates correctly and increments its fired count, but has no mechanism to notify anyone — no email, no SMS, no webhook. The alert is functionally invisible outside the Azure portal. Option A (severity) affects priority labeling, not notification delivery. Option B would prevent the rule from evaluating at all. Option D (region mismatch) is not a constraint for alert rules — they query across regions.

**Exam domain:** Monitor and maintain Azure resources
**Cert:** AZ-104
**Roadmap day:** Day-007

---

## Q4: Correlating logs across tables

**Scenario:** A user reports that a specific AI gateway request returned an error. You have the correlation ID `a3f2c1b0` from the response header. You need to find all log entries (requests, dependency calls, and exceptions) associated with this single user request.

**Question:** Which KQL query correctly retrieves all telemetry for this correlation ID?

A) `requests | where customDimensions["CorrelationId"] == "a3f2c1b0"`
B) `traces | where message contains "a3f2c1b0"`
C) `union requests, dependencies, traces, exceptions | where customDimensions["CorrelationId"] == "a3f2c1b0" or operation_Id == "a3f2c1b0"`
D) `dependencies | where operation_Id == "a3f2c1b0"`

**Answer:** C

**Why:** A single user request produces telemetry across multiple tables: the inbound request in `requests`, outbound calls in `dependencies`, log messages in `traces`, and any errors in `exceptions`. The `union` operator merges all four tables. Filtering on both `customDimensions["CorrelationId"]` (application-layer) and `operation_Id` (W3C trace ID) ensures you catch all associated records regardless of how the ID was propagated. Options A and D only search one table each. Option B searches only log messages and matches by string content rather than structured field.

**Exam domain:** Monitor and maintain Azure resources
**Cert:** AZ-104
**Roadmap day:** Day-007

---

## Q5: Scheduled query alert evaluation window

**Scenario:** You configure an Azure Monitor scheduled query alert that runs the following KQL every 5 minutes against a 15-minute evaluation window: `requests | where success == false | summarize failureRate = countif(success == false) * 100.0 / count()`. The threshold is `failureRate > 5`. During an incident, failures start at 10:00 AM and stop at 10:04 AM.

**Question:** At what time will the alert first fire?

A) 10:00 AM — the moment the first failure occurs
B) 10:05 AM — when the next 5-minute evaluation runs
C) 10:15 AM — after the 15-minute evaluation window fully contains the failures
D) The alert will not fire because failures stopped before 10:05 AM

**Answer:** B

**Why:** Scheduled query alerts run at the configured frequency (every 5 minutes), not in real time. The rule evaluates at the next scheduled run after 10:00 AM (10:05 AM), looking back over the 15-minute window (09:50–10:05). If the failure rate in that window exceeds 5%, the alert fires at 10:05. Option A is incorrect — scheduled query alerts are not event-driven. Option C misunderstands the evaluation window — it is a lookback window, not a delay before firing. Option D is incorrect because the failures occurred within the 15-minute window evaluated at 10:05.

**Exam domain:** Monitor and maintain Azure resources
**Cert:** AZ-104
**Roadmap day:** Day-007

---
