# Practice Questions — Azure Management and Governance (AZ-900 Domain 3)

---

## Q1: Azure Monitor data types

**Scenario:** A development team wants to monitor their Azure App Service application. They need to track both the number of HTTP requests per minute (to set autoscale rules) and the full details of each failed request (to diagnose errors after the fact).

**Question:** Which Azure Monitor data types should they use for each requirement?

A) Metrics for both — metrics store all telemetry in a time-series format  
B) Logs for both — logs provide richer data than metrics for all scenarios  
C) Metrics for request count (autoscale); Logs for failed request details (diagnosis)  
D) Application Insights for request count; Azure Monitor for failed request details  

**Answer:** C

**Why:** Metrics are numeric, time-series data with low latency (~1 min ingestion) — ideal for autoscale rules and alerts that need near-real-time thresholds. Logs (stored in Log Analytics) contain structured detail per event — ideal for root-cause analysis after a failure. A) metrics don't store full request details, only numeric summaries. B) logs have 2–5 minute ingestion delay and are too slow for autoscale triggers. D) Application Insights feeds Azure Monitor — they're part of the same platform, not alternatives.

**Exam domain:** Describe Azure management and governance  
**Cert:** AZ-900  
**Roadmap day:** Day-009

---

## Q2: Azure Monitor Alerts purpose

**Scenario:** An operations team wants to be notified automatically when the error rate on their API exceeds 5% over a 5-minute window, so they can respond before customers start calling.

**Question:** Which Azure feature should they configure?

A) Azure Advisor — it provides proactive recommendations based on Azure best practices  
B) Azure Service Health — it notifies teams of Azure platform incidents and maintenance  
C) Azure Monitor Alert Rule — it fires notifications when a metric or log condition is met  
D) Azure Cost Management — it sends alerts when spending exceeds a budget threshold  

**Answer:** C

**Why:** Azure Monitor Alert Rules evaluate a condition (metric threshold or KQL log query) on a schedule and fire an Action Group (email, SMS, webhook) when the condition is met. This is exactly the proactive operational alerting described. A) Advisor gives recommendations (e.g., "resize this VM"), not threshold alerts. B) Service Health notifies about Azure infrastructure problems, not application-level errors. D) Cost Management alerts are for billing thresholds, not operational metrics.

**Exam domain:** Describe Azure management and governance  
**Cert:** AZ-900  
**Roadmap day:** Day-009

---

## Q3: Custom metrics vs. built-in metrics

**Scenario:** A team has an AI gateway and wants to track time-to-first-token (TTFT) — how long users wait before they see the first word of a response. This is not a metric Azure App Service provides automatically.

**Question:** How can the team make TTFT visible in Azure Monitor?

A) Enable diagnostic settings on the App Service — this automatically captures all application timing data  
B) Use Azure Application Insights to emit a custom metric from the application code using the OpenTelemetry SDK  
C) Create an Azure Monitor alert rule with a KQL query — it will calculate TTFT from the request logs automatically  
D) TTFT cannot be tracked in Azure Monitor — a third-party APM tool is required  

**Answer:** B

**Why:** Application-defined metrics (like TTFT) must be emitted by the application code using a telemetry SDK. `Azure.Monitor.OpenTelemetry.AspNetCore` with an OpenTelemetry `Histogram<double>` instrument emits the value to the `customMetrics` table in Application Insights / Azure Monitor. A) diagnostic settings capture platform-level metrics (CPU, memory, HTTP status codes) — not application-calculated values like TTFT. C) a KQL alert rule evaluates existing data; it can't calculate a value that was never emitted. D) is false — Azure Monitor natively supports custom metrics via OpenTelemetry.

**Exam domain:** Describe Azure management and governance  
**Cert:** AZ-900  
**Roadmap day:** Day-009

---

## Q4: Action Groups

**Scenario:** A company has three Azure workloads. When any of them exceeds a 5xx error threshold, the on-call engineer should receive an email AND an SMS. Setting up separate email+SMS contacts for each workload's alert rules is becoming repetitive.

**Question:** What is the correct Azure feature to avoid duplicating this notification configuration?

A) Create one Alert Rule that covers all three workloads simultaneously  
B) Create one Action Group with the email and SMS contacts, then reference it from all three alert rules  
C) Use Azure Policy to enforce that all alert rules send to the same email address  
D) Configure Azure Service Health to forward all platform notifications to the engineer  

**Answer:** B

**Why:** An Action Group is a reusable collection of notification actions (email, SMS, webhook, Azure Function, etc.). Multiple alert rules across multiple resources can reference the same Action Group — so updating the on-call contact requires only one change. A) an alert rule targets one scope (resource, resource group, or subscription) and evaluates one condition — you'd still need multiple rules. C) Azure Policy enforces governance rules (resource configuration compliance), not alert routing. D) Service Health notifies about Azure platform events, not application metrics.

**Exam domain:** Describe Azure management and governance  
**Cert:** AZ-900  
**Roadmap day:** Day-009

---

## Q5: Azure Monitor scope — what it monitors

**Scenario:** A manager asks: "Does Azure Monitor only watch virtual machines, or can it monitor web apps, databases, and our custom application code too?"

**Question:** What is the correct description of Azure Monitor's scope?

A) Azure Monitor only monitors infrastructure resources (VMs, storage, networking); application-level monitoring requires a separate tool  
B) Azure Monitor monitors all Azure resources (infrastructure, PaaS, databases) and application code via Application Insights SDK integration  
C) Azure Monitor monitors Azure resources automatically, but custom application code requires a third-party APM tool like Datadog  
D) Azure Monitor monitors VMs and Azure Kubernetes Service only; App Service requires the Application Insights Classic SDK  

**Answer:** B

**Why:** Azure Monitor is the unified monitoring platform for all Azure resources. It automatically collects platform metrics and activity logs from every Azure service (VMs, App Service, SQL, Cosmos DB, Key Vault, etc.). Application Insights is a feature of Azure Monitor that adds application-level telemetry (traces, exceptions, custom metrics) via SDK integration — the `Azure.Monitor.OpenTelemetry.AspNetCore` package for .NET. A) is false — Azure Monitor covers PaaS and application code. C) is false — Azure Monitor natively handles application telemetry. D) is false — App Service is fully supported, and the OpenTelemetry-based SDK (not "Classic") is the current standard.

**Exam domain:** Describe Azure management and governance  
**Cert:** AZ-900  
**Roadmap day:** Day-009
