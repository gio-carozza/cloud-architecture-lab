# Concepts — Design identity, governance, and monitoring solutions

## Day 007 additions (2026-06-03)
*Topics: WAF Cost Optimization pillar as a design constraint, observable cost controls (metrics vs. logs), cost governance for AI workloads*

---

## WAF Cost Optimization as a Design Constraint

### If you're 10 years old
The Azure Well-Architected Framework is like a building inspection checklist for cloud systems. One section of the checklist is about money — "are you spending as little as possible while still doing what you need to do?" On Day 7 we added prompt caching to the AI gateway, which cuts the cost of each request by about 90%. That's not just a nice-to-have — it's what the Cost Optimization checklist requires you to design for from the beginning.

### If you're an architect
The Azure Well-Architected Framework (WAF) Cost Optimization pillar defines cost efficiency as a first-class design requirement, not a post-deployment tuning exercise. The five WAF pillars are Reliability, Security, Cost Optimization, Operational Excellence, and Performance Efficiency. For AZ-305, you must be able to recommend designs that satisfy multiple pillars simultaneously — not trade one off against another.

For AI gateways, cost optimization manifests at the architecture level in three ways:

1. **Prompt caching** — reusing stable input token sequences across requests, reducing per-call input token cost by ~90% on the cached portion
2. **Model tier selection** — routing requests to the cheapest model that meets the quality bar (Haiku for simple tasks, Sonnet for general, Opus for complex reasoning)
3. **Batch API** — offloading non-latency-sensitive workloads to async batch processing at lower cost per token

Each of these is a design decision, not a configuration choice. They must appear in the architecture before the first line of code is written. WAF Cost Optimization asks: "what is the cheapest design that meets the reliability, security, and performance requirements?" — not "how do we reduce cost after we've already built the expensive thing."

**Why it matters in enterprise:** AZ-305 exam questions require you to choose between architectures, not just configure one. The WAF cost lens is a deciding factor when two designs are functionally equivalent but differ in operational cost.

**Common beginner mistake:** Treating WAF pillars as a post-audit checklist rather than a design constraint. The WAF is most valuable at the whiteboard stage, where design decisions are cheap to change. Applying it after deployment means retrofitting — expensive, disruptive, and often incomplete.

---

## Observable Cost Controls: Metrics vs. Logs

### If you're 10 years old
Imagine you're trying to save electricity at home. You could check your monthly bill (logs — you only see the total after it's over). Or you could install a meter on each outlet that shows how much power each appliance uses right now (metrics — you see it happening). Metrics let you catch a problem while it's still small. The electricity bill tells you after you've already paid.

### If you're an architect
In Azure Monitor, **metrics** and **logs** serve fundamentally different purposes in cost governance:

| | Metrics | Logs |
|---|---|---|
| **Latency** | Near-real-time (seconds) | Minutes to hours |
| **Granularity** | Pre-aggregated (count, sum, avg) | Individual records with full context |
| **Query** | Azure Metrics Explorer, alert rules | KQL in Log Analytics |
| **Retention** | 93 days (free) | Configurable, billed by volume |
| **Best for** | Alerting on trends | Diagnosing individual events |

For AI gateway cost control, the right model is:
- **Counters as metrics** — `ai.provider.cache.hits`, `ai.provider.cache.misses`, `ai.provider.requests` — for real-time alerting and dashboards
- **Activity tags as log dimensions** — `llm.cache.read_tokens`, `llm.tokens.input` in `customDimensions` — for per-request analysis and cost attribution via KQL

The AZ-305 architect's obligation is to design both layers: metrics for alerting (when is the cache broken?), logs for diagnosis (which requests are contributing to cost?). A cost control that is only visible in logs is reactive. A cost control that surfaces a metric with an alert rule attached is proactive.

**Why it matters in enterprise:** AZ-305 asks you to "recommend a monitoring solution" — the correct answer includes both the signal type (metric vs. log) and the alerting mechanism. Logs-only answers are penalized because they describe reactive visibility, not proactive governance.

**Common beginner mistake:** Designing a logging solution for cost data and calling it "cost observability." Logs let you analyze cost after the fact. The architect-level answer adds a metric (or a KQL-based scheduled query alert) that fires before the invoice arrives.

---

## Cost Governance for AI Workloads

### If you're 10 years old
When your family decides how much to spend on electricity, you set a budget and watch the meter. If the meter starts running fast, someone turns off lights. AI workloads are the same — you set a budget for how many tokens you'll use, watch the meter (the telemetry), and turn down expensive features (like high-end models) if you're running hot.

### If you're an architect
Cost governance for AI workloads in enterprise requires three layers:

**1. Budget controls** — Azure Cost Management budgets with alert thresholds (50%, 75%, 90% of monthly limit). These are financial guardrails; they notify but do not stop spending.

**2. Operational controls** — App Service application settings as runtime toggles (`Anthropic__EnablePromptCaching`, model tier selection). These allow on-call engineers to reduce cost without a deployment.

**3. Observability controls** — Token-level telemetry surfaced as queryable metrics and custom dimensions. This is what enables the on-call engineer to know *why* cost is elevated before touching the operational controls.

The AZ-305 design decision: which tier of control handles which scenario?
- **Invoice surprise** → budget alert fired last week. Fix: tighten budget thresholds, add operational toggles.
- **Cost spike at 3 AM** → operational toggle (flip model tier or disable caching). Fix: the toggle must exist before the spike.
- **Sustained overrun** → forensic KQL analysis. Fix: per-request token attribution reveals the root cause.

An architect who designs only budget controls has reactive cost governance. One who adds operational toggles has responsive cost governance. One who adds token telemetry has proactive cost governance. AZ-305 expects the third.

**Why it matters in enterprise:** AI workloads have unbounded cost profiles — a prompt loop bug can generate millions of tokens in minutes. A defense-in-depth cost governance model (budget + operational toggle + telemetry) is the correct architecture pattern.

**Common beginner mistake:** Relying on Azure Cost Management budgets alone. Budget alerts are lagging indicators — they fire after money has been spent. The architect-level answer adds real-time observability so anomalies are detectable before they cross the budget threshold.

---
