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

---

## Q6: Application Insights storage model

**Scenario:** A DevOps team is provisioning a new Application Insights resource for a .NET 8 API on Azure App Service. A team member creates it in the Azure portal using the default settings and finds it labeled "Classic" in the portal.

**Question:** What is the most important reason to delete this resource and create a workspace-based Application Insights instead?

A) Classic Application Insights does not support ASP.NET Core applications  
B) Classic Application Insights does not support custom metrics  
C) Classic Application Insights is being deprecated — workspace-based stores data in a Log Analytics workspace enabling cross-resource KQL, unified RBAC, and configurable retention  
D) Classic Application Insights costs more per GB than workspace-based  

**Answer:** C

**Why:** Workspace-based App Insights is the current and future-supported mode. It uses a Log Analytics workspace as its backing store, enabling cross-resource KQL queries (joining AI tables with other Azure resource logs), workspace-level RBAC, and configurable retention policies. A) false — classic supports all .NET applications. B) false — classic supports custom metrics. D) the cost model differs, but the primary architectural reason to prefer workspace-based is capability and longevity, not unit pricing.

**Exam domain:** Describe Azure management and governance  
**Cert:** AZ-900  
**Roadmap day:** Day-006

---

## Q7: Log Analytics workspace role

**Scenario:** A company asks: "We already have Application Insights. Why do we also need a Log Analytics workspace?"

**Question:** What is the correct explanation of how Log Analytics workspace relates to workspace-based Application Insights?

A) They are competing services — you use one or the other, not both  
B) Log Analytics workspace is the optional long-term archive for Application Insights data  
C) Log Analytics workspace is the backing data store for workspace-based Application Insights — App Insights is the instrumentation layer that writes to it  
D) Log Analytics workspace stores infrastructure metrics; Application Insights stores application traces — they store different data in separate systems  

**Answer:** C

**Why:** In workspace-based mode, Application Insights is an instrumentation front-end: it receives telemetry from the SDK and writes it into a Log Analytics workspace (the actual storage and query layer). KQL queries run against the workspace. A) they are complementary, not competing. B) the workspace is the primary store, not an archive. D) both store infrastructure and application data — the distinction is not by data type.

**Exam domain:** Describe Azure management and governance  
**Cert:** AZ-900  
**Roadmap day:** Day-006

---

## Q8: App Insights connection string vs. instrumentation key

**Scenario:** A team configures an Azure App Service application to send telemetry to Application Insights by setting the `APPINSIGHTS_INSTRUMENTATIONKEY` environment variable using the classic instrumentation key from the portal.

**Question:** What configuration should they use instead, and why?

A) Use `APPLICATIONINSIGHTS_SAMPLING_PERCENTAGE` to reduce telemetry volume  
B) Use `APPLICATIONINSIGHTS_CONNECTION_STRING` with the full connection string value — it includes the ingestion endpoint explicitly and supports Private Link and sovereign cloud scenarios that an instrumentation key alone cannot  
C) Use `APPLICATIONINSIGHTS_AUTH_STRING` with a managed identity credential  
D) Instrumentation key is the current standard; connection string is only needed for on-premises servers  

**Answer:** B

**Why:** The connection string format (`InstrumentationKey=...;IngestionEndpoint=...`) explicitly encodes the endpoint, enabling Private Link ingestion, sovereign cloud endpoints, and regional routing. The instrumentation key alone assumes the public global endpoint. Microsoft recommends the connection string for all new configurations. A) sampling configuration is unrelated to connectivity. C) `APPLICATIONINSIGHTS_AUTH_STRING` is not a standard env var name. D) is the opposite of the truth.

**Exam domain:** Describe Azure management and governance  
**Cert:** AZ-900  
**Roadmap day:** Day-006

---

## Q9: KQL query scope with workspace-based App Insights

**Scenario:** An architect wants to write a single KQL query that correlates Application Insights traces from a .NET API with activity logs from Azure Key Vault to diagnose a suspicious incident.

**Question:** Which configuration enables this cross-resource query?

A) Enable diagnostic forwarding from Key Vault to the Application Insights resource directly  
B) Use workspace-based Application Insights and forward Key Vault diagnostic logs to the same Log Analytics workspace — both datasets are then queryable together via KQL  
C) Use Azure Sentinel to correlate across resources — standard Azure Monitor cannot join across resource types  
D) Export Application Insights data to Azure Storage and use Azure Synapse to join with Key Vault logs  

**Answer:** B

**Why:** When both App Insights telemetry and Key Vault diagnostic logs land in the same Log Analytics workspace, a single KQL `join` or `union` across their respective tables is possible — the workspace is the unification layer. A) App Insights is not a log ingestion target for other Azure resource diagnostics. C) Azure Monitor with workspace-based App Insights natively supports this without Sentinel. D) exporting to Storage + Synapse works but is expensive, slow, and unnecessary.

**Exam domain:** Describe Azure management and governance  
**Cert:** AZ-900  
**Roadmap day:** Day-006

---

## Q10: Monitoring resource provisioning order

**Scenario:** A team is automating their Azure observability stack with an ARM template. The template includes both Application Insights and a Log Analytics workspace. The template fails with a dependency error when creating the App Insights resource.

**Question:** What is the correct provisioning order?

A) App Insights → Log Analytics workspace (App Insights must exist to accept the workspace ID)  
B) Log Analytics workspace → App Insights (the workspace must exist before App Insights can reference it as its backing store)  
C) Both can be provisioned in parallel — there is no dependency between them  
D) API Management → Log Analytics workspace → App Insights (API Management must orchestrate the creation)  

**Answer:** B

**Why:** Workspace-based Application Insights requires the Log Analytics workspace ID at creation time. The workspace is the dependency; it must exist first. In ARM/Bicep, this is expressed with a `dependsOn` or by using the workspace's symbolic name as the input reference. A) inverts the dependency. C) they have a hard dependency. D) API Management is not involved in App Insights provisioning.

**Exam domain:** Describe Azure management and governance  
**Cert:** AZ-900  
**Roadmap day:** Day-006
