# Practice Questions — Design identity, governance, and monitoring solutions

## Day 007 additions (2026-06-03)
*5 questions — WAF Cost Optimization, observable cost controls, AI workload governance*

---

## Q1: WAF Cost Optimization pillar scope

**Scenario:** A solutions architect is reviewing the design for a new AI gateway that routes requests to Azure OpenAI. The business unit wants to minimize token costs. A junior architect suggests adding a "cost review meeting" every quarter to analyze Azure invoices and adjust the design.

**Question:** Which approach best reflects the WAF Cost Optimization pillar?

A) Schedule quarterly invoice reviews and adjust resource tiers based on findings
B) Design prompt caching, model tier routing, and token telemetry into the architecture before the first deployment
C) Use Azure Cost Management budgets to cap monthly spending
D) Choose the lowest-cost Azure OpenAI model for all requests

**Answer:** B

**Why:** The WAF Cost Optimization pillar treats cost efficiency as a first-class design requirement applied at architecture time, not as a post-deployment audit. Quarterly reviews (A) are reactive. Budget caps (C) prevent overspending but don't reduce cost per request. Choosing the lowest-cost model for all requests (D) ignores quality requirements. Option B — caching to reduce token volume, tier routing to match model cost to task complexity, and telemetry to make cost visible — is the proactive design approach the WAF demands.

**Exam domain:** Design identity, governance, and monitoring solutions
**Cert:** AZ-305
**Roadmap day:** Day-007

---

## Q2: Metrics vs. logs for cost alerting

**Scenario:** Your organization runs an AI gateway generating 50,000 token-based API calls per day. You need to detect a 10x cost spike within 5 minutes of it starting so the on-call engineer can take action before significant budget is consumed.

**Question:** Which monitoring approach best meets this requirement?

A) Write token usage to Azure Blob Storage and run a daily analysis job
B) Query the Log Analytics `dependencies` table with a KQL query every 5 minutes and set a threshold alert
C) Emit token count as a custom metric and create an Azure Monitor metric alert rule
D) Configure Azure Cost Management to send an email when daily spend exceeds $100

**Answer:** C

**Why:** Custom metrics are near-real-time (seconds latency) and can trigger metric alert rules that evaluate continuously against a rolling window — achieving sub-5-minute detection. KQL-based scheduled query alerts (B) run on a configured frequency (minimum 1 minute for Log Analytics) and have inherent ingestion lag, making 5-minute detection unreliable. Blob Storage analysis (A) is a batch pattern with hours of latency. Azure Cost Management alerts (D) are billing-layer signals with 24+ hour lag. For real-time cost anomaly detection, custom metrics with metric alert rules is the correct architecture.

**Exam domain:** Design identity, governance, and monitoring solutions
**Cert:** AZ-305
**Roadmap day:** Day-007

---

## Q3: Layered cost governance model

**Scenario:** An enterprise AI platform has experienced three types of cost incidents in the past year: (1) a misconfigured prompt loop that generated 10M tokens in 20 minutes, (2) a sustained 30% overrun across a full month due to model tier choices, (3) an unexpected invoice for a new AI feature that wasn't budgeted. The architect must design a governance model that handles all three scenarios.

**Question:** Which combination of controls addresses all three incident types?

A) Azure Cost Management budgets + quarterly architecture reviews
B) Real-time token telemetry (metrics + logs) + operational toggles (App Service settings) + Azure Cost Management budgets
C) Azure Policy to restrict model tier usage + Azure Cost Management budgets
D) Log Analytics workspace with KQL dashboards + weekly spending reports

**Answer:** B

**Why:** The three incidents map to three tiers of control. A prompt loop spike (incident 1) requires real-time detection (metrics alert) and an immediate lever (operational toggle to disable the feature). A sustained overrun (incident 2) requires forensic analysis (token telemetry in logs) to find which requests are expensive. An unbudgeted feature (incident 3) requires financial guardrails (Cost Management budgets). Option B covers all three tiers. Option A (budgets + reviews) is reactive for incidents 1 and 2. Option C (Policy + budgets) prevents usage but doesn't enable diagnosis. Option D (KQL + reports) is analytical only — no alerting, no operational control.

**Exam domain:** Design identity, governance, and monitoring solutions
**Cert:** AZ-305
**Roadmap day:** Day-007

---

## Q4: Recommend a monitoring solution for cost observability

**Scenario:** You are designing an AI gateway that calls Azure OpenAI. Stakeholders want to know in real time whether the prompt cache is working (to validate cost savings), and want monthly cost attribution by team. The solution must require no manual data collection.

**Question:** Which monitoring design meets both requirements?

A) Azure Cost Management with resource tags for team attribution; manual KQL queries run monthly for cache data
B) Custom metrics for cache hit/miss counters exported to Azure Monitor; Log Analytics with custom dimensions (team ID, token counts) for monthly attribution via KQL
C) Azure Application Insights with only default telemetry; export logs to Excel monthly for team attribution
D) Azure Advisor cost recommendations reviewed weekly; export Azure invoices to Power BI for team attribution

**Answer:** B

**Why:** Real-time cache validation requires custom metrics (near-real-time, alertable) — specifically `ai.provider.cache.hits` and `ai.provider.cache.misses` counters. Monthly cost attribution by team requires structured telemetry with a team identifier propagated as a custom dimension, queryable via KQL (`summarize sum(inputTokens) by teamId`). Option A requires manual KQL execution. Option C exports to Excel (manual). Option D uses Advisor (recommendations, not telemetry) and invoice exports (no per-request granularity for team attribution). Option B is the only fully automated, real-time + analytical combination.

**Exam domain:** Design identity, governance, and monitoring solutions
**Cert:** AZ-305
**Roadmap day:** Day-007

---

## Q5: YAGNI in monitoring architecture design

**Scenario:** An architect is designing monitoring for an AI gateway that currently has one LLM provider (Anthropic). A team member proposes building a generic "AI cost monitoring framework" that supports any future provider by abstracting all provider-specific metrics behind a unified schema. This would take 6 weeks to build.

**Question:** Which approach best reflects the AZ-305 principle of designing for change while avoiding premature generalization?

A) Build the generic framework now; provider-agnostic design is always superior
B) Build provider-specific monitoring for Anthropic today; define the abstraction boundary when the second provider's telemetry shape is known
C) Skip monitoring for now and add it when the second provider is onboarded
D) Use a third-party observability platform that already supports multiple providers

**Answer:** B

**Why:** AZ-305 emphasizes designing for change, not designing for all possible futures. With a single provider, a generic abstraction is designed against one example — its shape will likely need revision when the second provider reveals different telemetry fields, different naming, different billing semantics. Building the Anthropic-specific layer now delivers immediate value, and the abstraction boundary can be properly designed when two real examples exist. Option A builds speculative abstraction (high cost, uncertain value). Option C defers operational capability unnecessarily. Option D introduces a third-party dependency that may not integrate with the existing Azure Monitor stack.

**Exam domain:** Design identity, governance, and monitoring solutions
**Cert:** AZ-305
**Roadmap day:** Day-007

---
