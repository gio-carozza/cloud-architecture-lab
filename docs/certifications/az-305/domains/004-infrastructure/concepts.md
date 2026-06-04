# Concepts — Design Infrastructure Solutions (AZ-305 Domain 4)

---

## Async / Job-Shaped Workload Pattern

### If you're 10 years old
Imagine a restaurant. Some orders go to the "fast lane" (burgers, ready in 3 minutes). Others go to the "slow lane" — a whole roast chicken that takes an hour. You don't make the customer stand at the counter for an hour; you give them a ticket number, seat them, and bring the food when it's ready. Async workloads work the same way: the system accepts the job, gives back a ticket (job ID), does the work in the background, and lets the caller collect the result when it's done.

### If you're an architect
The async/job-shaped workload pattern decouples job submission from result retrieval via a durable queue or broker. The canonical shape is: **submit → acknowledge (202 Accepted + job ID) → poll status → retrieve result**. This maps directly to the Anthropic Batch API, Azure OpenAI Global Batch, and any durable task framework (Azure Durable Functions, Service Bus + worker pattern).

The pattern is appropriate when: (a) processing time exceeds acceptable synchronous wait time (typically > 1–2 seconds for interactive, > 60 seconds for any user-facing), (b) the workload is resource-intensive and benefits from dedicated compute, or (c) bursts require queue-based load levelling rather than direct scaling. The tradeoff is operational complexity: you now have a queue, a worker, a status store, and a retrieval endpoint — four components instead of one.

**Why this matters in enterprise:** The async pattern is the foundation of scalable AI pipelines. Without it, every expensive LLM call blocks a thread and a connection. The exam tests whether you can identify which compute architecture fits an async workload (durable functions vs. container jobs vs. batch-native APIs) and articulate why.

**Common beginner mistake:** Building a synchronous request-response API for a job that takes 30+ seconds, then adding timeouts and "please wait" spinners as a band-aid, rather than designing the async handoff from the start.

---

## Recommending Compute for Batch Workloads — WAF Decision Framework

### If you're 10 years old
If you need to move 10 boxes, you could carry them one at a time (slow, tiring), hire someone to carry them on a cart for you (a service that handles it), or rent a van and drive them all at once (batch). Picking the right option depends on how heavy the boxes are, how fast you need them moved, and how much money you want to spend. Architects make the same kind of decision for compute — they pick the right "vehicle" for the job.

### If you're an architect
The Azure Well-Architected Framework (Cost Optimization pillar) directs architects to **match compute size and billing model to workload shape**. For batch AI workloads the decision tree is:

| Factor | Leaning |
|--------|---------|
| Job duration < 10 min, event-triggered | Azure Functions (Consumption or Flex) |
| Job duration > 10 min, stateful orchestration | Azure Durable Functions |
| Very large parallel compute, HPC-style | Azure Batch |
| Containerised worker, scale-to-zero | Azure Container Apps Jobs |
| Provider-native batch (LLM) | Azure OpenAI Global Batch / Anthropic Batch API |

For LLM-specific batch workloads, the provider-native batch API is the default recommendation: it requires no additional compute resource, no queue management, and delivers the 50% cost reduction as a built-in pricing tier. You pay only for tokens — not for the compute that processed them. Bring-your-own-compute (Functions, Container Apps) is appropriate when pre/post-processing logic is substantial enough to warrant a full application runtime.

**Why this matters in enterprise:** The AZ-305 exam presents scenarios where you must choose among multiple valid compute options. The WAF cost lens eliminates options that are over-engineered or over-priced for the workload's actual requirements.

**Common beginner mistake:** Reaching for Azure Kubernetes Service for a simple nightly batch job, adding operational overhead (cluster management, pod scaling, ingress) that is wildly disproportionate to the workload's needs.

---

## Cost Optimization Pillar — Deferrable Workload Routing

### If you're 10 years old
Electricity costs less at night (off-peak hours) than during the day (peak hours). Smart home owners run their dishwasher at midnight to save money. The Cost Optimization pillar in Azure works the same way — run things that can wait during "off-peak" (batch mode) and save the expensive "right now" capacity for things that truly need it.

### If you're an architect
The Azure Well-Architected Framework Cost Optimization pillar defines **deferrable workload routing** as one of the primary cost levers: workloads without a real-time latency requirement should be routed to a lower-cost compute or pricing tier. For AI workloads this translates directly to the batch vs. synchronous routing decision:

- **Synchronous path** — tokens billed at standard rate, dedicated TPM quota consumed, response in seconds
- **Batch path** — tokens billed at 50% of standard rate, isolated enqueued-token quota, response within 24 hours

The routing criterion is the **latency SLA**: if the business requirement can tolerate a result in minutes-to-hours rather than seconds, the batch path is the correct choice on cost grounds alone. This is a WAF design decision, not an implementation detail — it should be documented in an ADR and encoded in the gateway's routing logic.

**Why this matters in enterprise:** The unconditional 50% cost reduction on batch-eligible workloads is one of the highest-ROI architectural decisions available for AI-heavy platforms. Missing it because "synchronous was easier to implement" is a recurring pattern that costs organisations tens of thousands of dollars per month at scale.

**Common beginner mistake:** Treating the batch vs. synchronous choice as a performance decision ("batch is slower") rather than a cost-governance decision ("batch is cheaper for deferrable work"). Synchronous is not "better" — it is more expensive and appropriate only when latency SLA demands it.

---

## Latency SLA as an Architectural Constraint

### If you're 10 years old
Some things have to happen right now — like a fire alarm going off the second there's smoke. Other things can wait — like getting your school report card at the end of term instead of the minute each test is marked. Architects decide which things are "right now" versus "can wait" — and that decision changes how they build the whole system.

### If you're an architect
The latency SLA is the maximum acceptable time between a request being submitted and the result being available to the caller. It is a **first-class architectural constraint** that drives compute selection, processing path, and pricing tier. The decision tree:

- **< 1 second** — synchronous API, premium compute tier, low-latency routing
- **1–30 seconds** — synchronous with streaming (token streaming for LLMs) or async with short-poll
- **30 seconds – 5 minutes** — async with status endpoint (202 + poll), standard compute
- **> 5 minutes / overnight** — batch API or scheduled job, lowest-cost compute tier

For AI gateways, explicitly capturing the latency SLA for each request class in the gateway contract (as an input field or endpoint segment) forces the routing decision to be made by the client at design time, not by the gateway at runtime based on guesswork.

**Why this matters in enterprise:** Latency SLAs are contractual obligations — violating them triggers SLA credits or escalations. Designing systems without explicit SLA tiers leads to over-provisioning (paying for sub-second latency everywhere) or under-provisioning (batching something that needed to be interactive).

**Common beginner mistake:** Designing a single endpoint that "tries to be fast but falls back to async" without documenting which path a given request will take, leaving callers with unpredictable response times and no way to reason about cost.
