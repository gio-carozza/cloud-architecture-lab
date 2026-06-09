# Practice Questions — Design Infrastructure Solutions (AZ-305 Domain 4)

---

## Q1: Choosing compute for a batch AI workload

**Scenario:** A company needs to generate product descriptions for 50,000 SKUs every Sunday night using Azure OpenAI. The job must complete within 12 hours. There is no real-time user interaction. The platform team wants to minimise infrastructure management overhead and token cost.

**Question:** Which compute architecture should the architect recommend?

A) Azure Kubernetes Service with a GPT-4o deployment and a horizontal pod autoscaler  
B) Azure OpenAI Global Batch API, submitting a JSONL file and polling for completion  
C) Azure Functions (Consumption plan) calling the synchronous chat completions endpoint in parallel  
D) Azure Logic Apps with a For-Each loop invoking the Azure OpenAI connector  

**Answer:** B

**Why:** Global Batch requires zero infrastructure management (no cluster, no pods, no autoscaler), uses isolated enqueued-token quota that doesn't compete with online traffic, and prices at 50% of the synchronous rate — the lowest cost option. A) AKS adds substantial operational overhead (cluster management, scaling config) disproportionate to a nightly batch job; token cost is unchanged. C) Functions calling synchronous endpoints pays full token rate, consumes TPM quota shared with interactive users, and hits per-function timeouts for long-running jobs. D) Logic Apps has per-action pricing that adds up for 50,000 iterations and is not designed for high-throughput token workloads.

**Exam domain:** Design infrastructure solutions  
**Cert:** AZ-305  
**Roadmap day:** Day-008

---

## Q2: WAF Cost Optimization — deferrable workload identification

**Scenario:** An enterprise runs three Azure OpenAI-backed workloads on the same synchronous deployment: (1) a real-time chatbot for customer support, (2) a twice-daily sentiment analysis run on 10,000 support tickets, and (3) a weekly competitive intelligence report generated from 500 documents. The team wants to reduce monthly Azure OpenAI spend by at least 40% without degrading user experience.

**Question:** Which workloads should be migrated to Azure OpenAI Global Batch, per the WAF Cost Optimization pillar?

A) Workload 1 only — the chatbot generates the most tokens per month  
B) Workloads 2 and 3 — both have deferrable results and no real-time latency requirement  
C) All three workloads — batch pricing applies universally and reduces total spend  
D) Workload 3 only — weekly cadence makes it the most deferrable  

**Answer:** B

**Why:** The WAF Cost Optimization pillar directs you to route to the lowest-cost tier that meets the latency SLA. Workloads 2 and 3 have no interactive user waiting for results — their SLAs can absorb the batch window, delivering the 50% cost reduction on those volumes. Migrating both captures the largest total saving. A) the chatbot is user-facing and latency-sensitive — batch is incompatible with sub-second response requirements. C) the chatbot cannot use batch without breaking the user experience. D) migrating only Workload 3 leaves a significant saving on the table from Workload 2's twice-daily 10,000-ticket runs.

**Exam domain:** Design infrastructure solutions  
**Cert:** AZ-305  
**Roadmap day:** Day-008

---

## Q3: Async pattern — correct HTTP contract

**Scenario:** An architect is designing an API endpoint that submits a long-running AI document analysis job (estimated 2–10 minutes per document). The design must follow REST conventions and allow clients to check progress without holding a long-lived connection open.

**Question:** What is the correct HTTP response pattern for the submit endpoint?

A) Return HTTP 200 with the analysis result in the body once processing completes  
B) Return HTTP 202 Accepted with a `Location` header pointing to a status endpoint, and a job ID in the response body  
C) Return HTTP 204 No Content and send a webhook callback to the client when done  
D) Return HTTP 200 with a `Retry-After` header and an empty body  

**Answer:** B

**Why:** HTTP 202 Accepted is the correct status for "the request has been received and will be processed" — it explicitly communicates that the result is not yet available. The `Location` header pointing to a status endpoint follows the async REST pattern (also called the status monitor pattern in Azure API design guidance), giving clients a deterministic polling target. A) HTTP 200 implies synchronous completion — holding the connection for 2–10 minutes is not acceptable. C) webhook callbacks are a valid push alternative but require clients to expose a receiving endpoint, adding complexity and coupling. D) HTTP 200 with Retry-After is not a standard pattern and misleads clients into thinking the response body contains the result.

**Exam domain:** Design infrastructure solutions  
**Cert:** AZ-305  
**Roadmap day:** Day-008

---

## Q4: Latency SLA as a routing constraint

**Scenario:** An AI gateway serves two client types: (A) a React web app where users type questions and expect answers in under 3 seconds, and (B) a backend data pipeline that submits 500 documents nightly and reads results the next morning. The gateway currently routes all traffic to the same synchronous endpoint.

**Question:** What architectural change best aligns the gateway design with the latency SLA of each client?

A) Add a premium deployment tier for the web app and a standard tier for the pipeline  
B) Expose a dedicated batch endpoint for the pipeline that routes to the batch API; keep the existing synchronous endpoint for the web app  
C) Add a `priority` header to the existing endpoint and route high-priority requests faster  
D) Rate-limit the pipeline client to prevent it from consuming capacity needed by the web app  

**Answer:** B

**Why:** Separate endpoints encode the routing decision at the API contract level — clients choose based on their known latency SLA at design time. The batch endpoint routes to the batch API (50% cost, isolated quota, 24-hour SLA); the synchronous endpoint retains sub-second response. A) tiered deployments isolate quota but don't reduce cost for the pipeline; both still pay synchronous rates. C) a priority header on a single endpoint is ambiguous — it doesn't change the processing path or the pricing tier, just the queue order within the synchronous path. D) rate-limiting treats the symptom (quota contention) without addressing the cost or the architectural mismatch.

**Exam domain:** Design infrastructure solutions  
**Cert:** AZ-305  
**Roadmap day:** Day-008

---

## Q5: Over-engineered compute selection

**Scenario:** A development team proposes the following architecture for a nightly job that processes 1,000 AI summarization requests: an AKS cluster with 3 nodes, a Helm chart for the worker deployment, a Redis cache for job state, a custom Kubernetes operator for scheduling, and a Prometheus + Grafana stack for monitoring. The job runs once per day and takes approximately 20 minutes.

**Question:** What should the solutions architect recommend instead, and why?

A) Keep the AKS architecture — it provides the most resilient platform for future scale  
B) Replace the AKS cluster with Azure OpenAI Global Batch; the job shape (submit once, retrieve once daily) maps directly to the provider-native batch API with no infrastructure to manage  
C) Replace AKS with Azure Functions on a Premium plan to reduce management overhead while retaining container-based compute  
D) Replace AKS with Azure Container Apps with KEDA scaling, which is simpler than AKS but still container-native  

**Answer:** B

**Why:** The workload is a single nightly batch run — the provider-native batch API handles it with zero infrastructure (no cluster, no cache, no operator, no monitoring stack), at 50% token cost, and with a 24-hour SLA that comfortably fits a once-daily cadence. The AKS architecture is 10× over-engineered for this shape. C) Azure Functions on Premium still pays synchronous token rates and requires managing the function runtime and scaling config. D) Container Apps with KEDA is simpler than AKS but still requires cluster management, image builds, and scaling configuration — none of which are necessary when the provider-native batch API handles everything.

**Exam domain:** Design infrastructure solutions  
**Cert:** AZ-305  
**Roadmap day:** Day-008

---

## Q11: Observability infrastructure provisioning order

**Scenario:** A team is deploying a new Azure App Service API and needs to set up production monitoring. They need Application Insights, a Log Analytics workspace, and an alert rule for 5xx errors. A junior engineer asks: "Does the order we provision these resources matter?"

**Question:** What is the correct provisioning dependency order?

A) Application Insights → Log Analytics workspace → Alert rule  
B) Alert rule → Application Insights → Log Analytics workspace  
C) Log Analytics workspace → Application Insights → Alert rule (workspace must exist before App Insights can reference it; App Insights must exist before alert rules reference its data)  
D) All three can be provisioned in parallel — there are no dependencies between them  

**Answer:** C

**Why:** Workspace-based Application Insights requires the Log Analytics workspace ID at creation time — hard dependency. Alert rules that evaluate App Insights data (via KQL or metric conditions) require the App Insights resource to exist as the target scope. The correct sequence is: workspace → App Insights → alert rule. A and B invert the workspace → App Insights dependency. D is false — there are hard creation-time dependencies.

**Exam domain:** Design infrastructure solutions  
**Cert:** AZ-305  
**Roadmap day:** Day-006

---

## Q12: Monitoring design — workspace topology

**Scenario:** An enterprise runs three environments (dev, staging, production) each with their own Azure App Service and Application Insights. All three App Insights resources share a single Log Analytics workspace. A security audit flags that developers can query production telemetry from their dev workstations.

**Question:** What architectural change addresses the audit finding while maintaining operational capability?

A) Delete the shared workspace and use classic Application Insights (no Log Analytics required)  
B) Create a separate Log Analytics workspace per environment and re-link each App Insights resource to its environment's workspace — apply RBAC per workspace  
C) Disable developer access to the portal for the production App Insights component  
D) Move production App Insights to a different Azure subscription  

**Answer:** B

**Why:** RBAC is applied at the workspace level in workspace-based App Insights. A separate workspace per environment is the standard topology for controlling who can read prod data — developers get access to dev/staging workspaces, only ops and on-call get prod workspace access. A) reverting to classic loses workspace capabilities and doesn't fix the access problem. C) portal access can be restricted but doesn't prevent programmatic query access to the shared workspace. D) a different subscription adds cost and management overhead; separate workspaces already solve the RBAC requirement within the same subscription.

**Exam domain:** Design infrastructure solutions  
**Cert:** AZ-305  
**Roadmap day:** Day-006

---

## Q13: Alert rule design — error rate threshold

**Scenario:** An architect is setting up alerting for an AI gateway deployed on Azure App Service. The requirement is: "notify the on-call engineer if the 5xx error rate exceeds 5% over a 5-minute window, so they can investigate before the issue escalates."

**Question:** Which combination of Azure resources correctly implements this requirement?

A) An Azure Service Health alert targeting the App Service plan  
B) An Azure Monitor alert rule with a KQL condition evaluating `requests` table in App Insights, an Action Group with email notification, Severity 2  
C) Azure Cost Management budget alert set to fire when monthly spend exceeds $500  
D) Azure Advisor recommendation for "enable diagnostic logs"  

**Answer:** B

**Why:** Azure Monitor alert rules evaluate KQL conditions against Log Analytics (or metrics) on a schedule and fire an Action Group (email, SMS, webhook) when the condition is met. The correct KQL: `requests | where timestamp > ago(5m) | summarize error_rate = countif(resultCode >= 500) / count() | where error_rate > 0.05`. Action Group routes the alert to the on-call engineer. A) Service Health alerts notify about Azure platform incidents, not application error rates. C) Cost Management alerts are billing thresholds, not operational metrics. D) Advisor recommendations are best-practice suggestions, not threshold alerts.

**Exam domain:** Design infrastructure solutions  
**Cert:** AZ-305  
**Roadmap day:** Day-006

---

## Q14: Log ingestion cost governance

**Scenario:** An enterprise architect is designing a Log Analytics workspace for a high-traffic AI gateway. The gateway logs every token count, latency, and model call — approximately 50GB of logs per day. The initial estimate puts Log Analytics ingestion cost at $8,000/month.

**Question:** Which architectural approach most directly reduces this cost without losing operational visibility?

A) Switch to classic Application Insights (no Log Analytics = no ingestion cost)  
B) Apply a commitment tier pricing on the workspace (e.g., 50GB/day commitment) instead of pay-as-you-go, AND configure a data collection rule to filter verbose diagnostic logs before ingestion  
C) Move all telemetry to Azure Storage blobs and query with Azure Synapse  
D) Stop logging token counts — only log errors  

**Answer:** B

**Why:** Commitment tiers offer significant discounts over pay-as-you-go at sustained ingestion volumes (e.g., 50GB/day commitment tier is ~25% cheaper than PAYG at that scale). Data collection rules allow pre-ingestion filtering — dropping verbose fields or entire log categories that aren't needed for alerting or compliance. A) classic App Insights still has ingestion costs and loses cross-resource KQL. C) Storage + Synapse is cheaper per GB but adds query latency and doesn't support real-time alerting. D) removing token telemetry eliminates cost attribution capability — a governance regression.

**Exam domain:** Design infrastructure solutions  
**Cert:** AZ-305  
**Roadmap day:** Day-006

---

## Q15: Observability as infrastructure — design document requirement

**Scenario:** A solutions architect is reviewing a design document for a new AI-powered customer service platform. The document covers compute (App Service), storage (Cosmos DB), and networking (VNet). The architect flags it as incomplete before approving.

**Question:** Which missing section is the architect most likely flagging?

A) The document doesn't specify the Azure region for deployment  
B) The document has no observability infrastructure design — no Log Analytics workspace, Application Insights, alert rules, or retention policy are specified  
C) The document doesn't include a disaster recovery site in a secondary region  
D) The document uses App Service instead of Azure Kubernetes Service  

**Answer:** B

**Why:** Production systems require observability infrastructure specified in the design document — not retrofitted post-deploy. Missing: Log Analytics workspace (topology, retention), Application Insights (workspace-based, connection string wiring), alert rules (thresholds, severity, action groups), and RBAC. Without this, the first production incident has no tooling for diagnosis. A) region is important but is typically covered for compute resources. C) DR is a business continuity concern, not the most likely flag on a greenfield AI service design. D) AKS vs. App Service is a valid concern but would be flagged separately.

**Exam domain:** Design infrastructure solutions  
**Cert:** AZ-305  
**Roadmap day:** Day-006

---

## Q6: Streaming vs. buffered response — architectural driver

**Scenario:** An enterprise is building an AI assistant gateway. One team proposes a synchronous REST endpoint returning the full LLM completion after 3–6 seconds. Another proposes an SSE streaming endpoint delivering tokens within 200ms. The CTO asks the architect to justify the streaming approach.

**Question:** What is the primary architectural justification for streaming?

A) Streaming is cheaper — SSE reduces per-token cost at the provider  
B) Streaming reduces time-to-first-token (TTFT), transforming perceived latency from total response time to first-token arrival — the primary driver of user-perceived performance in an interactive chat product  
C) Azure App Service does not support buffered responses longer than 2 seconds  
D) Streaming is more secure because smaller payloads are harder to intercept  

**Answer:** B

**Why:** In a streaming UX, users begin reading after the first token — not the last. A 200ms TTFT with 5s total duration feels fast; a 5s blank wait feels broken, even with identical total duration. B captures the architectural value. A) is false — provider token cost is identical for `"stream": true` vs buffered. C) is false — App Service supports both. D) is false — streaming has no meaningful security advantage over buffered at the payload level.

**Exam domain:** Design infrastructure solutions  
**Cert:** AZ-305  
**Roadmap day:** Day-009

---

## Q7: TTFT SLO definition

**Scenario:** An architect is defining SLOs for a streaming AI chat gateway. The team measures both TTFT and total response duration. The architect must select the right metric for the customer-facing SLA.

**Question:** Which metric and threshold best represents a customer-facing latency commitment for a streaming AI product?

A) Mean total response duration < 5 seconds  
B) p95 TTFT < 1 second  
C) p50 total response duration < 3 seconds  
D) Token throughput > 20 tokens/second  

**Answer:** B

**Why:** p95 TTFT is the metric that directly controls whether users perceive the product as responsive. In a streaming UX, total duration is invisible — users are reading while content arrives. p95 covers the tail of the distribution that users experience as slow. A) mean total duration averages over outliers and measures an experience users don't have in a streaming product. C) p50 leaves half of users at above-median latency. D) token throughput is a secondary quality metric unrelated to the blank-screen latency SLA.

**Exam domain:** Design infrastructure solutions  
**Cert:** AZ-305  
**Roadmap day:** Day-009

---

## Q8: Proxy buffering in the streaming path

**Scenario:** A streaming AI gateway is deployed to Azure App Service (Linux). Local integration tests confirm incremental token delivery. After the Azure deploy, the same requests deliver all tokens in a single burst. The gateway correctly sets `Content-Type: text/event-stream` and calls `FlushAsync()` after each chunk.

**Question:** What infrastructure layer is silently buffering the response, and what is the fix?

A) Azure Front Door is caching SSE responses; configure CDN bypass for the streaming route  
B) Linux App Service doesn't support streaming; migrate to Windows-based App Service  
C) nginx (the reverse proxy in App Service Linux) buffers responses by default; add `X-Accel-Buffering: no` to the response headers  
D) Azure Load Balancer is reassembling packets before forwarding; enable TCP passthrough  

**Answer:** C

**Why:** Azure App Service on Linux uses nginx as a reverse proxy. Nginx buffers upstream responses by default, converting incremental SSE frames into a single burst delivery. `X-Accel-Buffering: no` is nginx's documented per-response mechanism to disable this behavior. A) Azure Front Door is not in the default App Service path. B) is false — both Linux and Windows App Service support streaming. D) Azure Load Balancer operates at Layer 4 and does not reassemble HTTP application payloads.

**Exam domain:** Design infrastructure solutions  
**Cert:** AZ-305  
**Roadmap day:** Day-009

---

## Q9: Client disconnect and upstream cost governance

**Scenario:** Usage analysis shows 15% of streaming sessions are abandoned mid-stream. The gateway continues generating and billing for tokens until completion, even after the client disconnects. The team needs to eliminate this waste.

**Question:** What is the correct architectural fix?

A) Set a 30-second maximum streaming duration to limit abandoned sessions  
B) Use a circuit breaker on the upstream LLM connection to interrupt streams when error rates rise  
C) Pass `HttpContext.RequestAborted` as the CancellationToken to the upstream HTTP call; client disconnects propagate upstream and cancel token generation immediately  
D) Implement client-side keep-alive pings; terminate sessions that stop sending pings  

**Answer:** C

**Why:** ASP.NET Core signals client disconnect through `HttpContext.RequestAborted`. Passing this token to the upstream `SendAsync` call cancels the LLM HTTP connection the instant the client disconnects — stopping token generation, billing, and I/O simultaneously. A) a fixed 30-second cutoff penalises legitimate long responses. B) circuit breakers respond to aggregate error rates, not individual client lifecycles. D) keep-alive pings require client cooperation and add complexity that `RequestAborted` already solves natively.

**Exam domain:** Design infrastructure solutions  
**Cert:** AZ-305  
**Roadmap day:** Day-009

---

## Q10: Interface design for streaming capability — SOLID principles

**Scenario:** An AI gateway has `IChatModelProvider` with `Task<ChatResponse> SendAsync(...)`. Adding streaming. Proposal A: create `IStreamingChatModelProvider` (same pattern as `IBatchChatModelProvider`). Proposal B: extend `IChatModelProvider` with `IAsyncEnumerable<ChatChunk> StreamAsync(...)`.

**Question:** Which SOLID principle determines the correct choice, and what is the deciding criterion?

A) Open/Closed — the existing interface must be closed to modification; always extend via a new interface  
B) Liskov Substitution — a non-streaming provider can implement `StreamAsync` by yielding one terminal chunk (graceful degradation), so extension is valid; batch broke this test and earned its own seam  
C) Interface Segregation — streaming and buffered chat are separate concerns that must always be separated  
D) Single Responsibility — each interface must have only one method  

**Answer:** B

**Why:** The LSP test: can a provider that lacks the capability implement the method without throwing? For streaming: yes — call `SendAsync`, yield the result as one chunk. For batch: no — a non-batch provider cannot provide a valid job ID or poll result. The asymmetry is why batch earned a new seam and streaming extends the existing one. A) OCP applies to extending classes without modifying them, not to preventing interface evolution. C) ISP requires new interfaces when different consumers need only subsets of methods — the same controller uses both buffered and streamed chat, so no segregation pressure exists. D) single-method interfaces are not required by SRP.

**Exam domain:** Design infrastructure solutions  
**Cert:** AZ-305  
**Roadmap day:** Day-009

---

## Q16: Recommending a caching solution — AI gateway system prompt

**Scenario:** An enterprise AI gateway processes 20,000 requests per day. Each request includes a 4,000-token system prompt that is identical for all requests and changes weekly. The team wants to reduce input-token billing with minimal infrastructure overhead. They ask the architect to recommend a caching solution.

**Question:** Which caching solution best meets these requirements?

A) Azure Cache for Redis (Standard tier) — cache full response payloads using the user message as the key  
B) Provider-native prompt caching — annotate the system prompt block with `cache_control: {"type":"ephemeral","ttl":"1h"}` inside the AI client  
C) Azure API Management response cache policy — cache the HTTP response for repeated identical POST requests  
D) Azure CDN — cache POST responses using the request URL as the cache key  

**Answer:** B

**Why:** The system prompt is large (4,000 tokens — above the 1,024 minimum), stable (weekly changes), and repeated on every request — the ideal candidate for provider-native caching. This reduces input-token billing by ~90% with zero additional infrastructure. A) Redis would need to cache complete response payloads, which vary per user message; cache key design for variable inputs is complex and the input-token billing problem is not addressed. C) API Management response caching applies to GET requests and identical POST bodies; user messages vary, so cache hits would be rare or non-existent. D) CDNs do not cache POST request bodies or variable AI responses in standard configurations.

**Exam domain:** Design infrastructure solutions  
**Cert:** AZ-305  
**Roadmap day:** Day-007

---

## Q17: YAGNI vs. premature abstraction in provider design

**Scenario:** An AI gateway has one LLM provider (Anthropic). The team is implementing prompt caching. Proposal A builds a `CachingChatModelProvider` decorator above `IChatModelProvider`. Proposal B implements caching inside `ClaudeApiClient` (the Anthropic implementation). There is no confirmed second cacheable provider in the roadmap.

**Question:** Which principle determines the correct choice, and what should the architect recommend?

A) Open/Closed Principle — always use a decorator to avoid modifying existing code; choose Proposal A  
B) YAGNI — no second provider exists to generalise for; choose Proposal B (inside the provider), document the refactoring path for when a second cacheable provider arrives  
C) Single Responsibility — caching and API calling are separate concerns; always separate them with Proposal A  
D) DRY — build the decorator now to prevent future duplication when a second provider lands  

**Answer:** B

**Why:** YAGNI: the decorator is the right abstraction when there are two or more providers with caching needs. With one provider and no confirmed second, the abstraction solves a problem that doesn't exist, adds an untested interface boundary, and creates maintenance overhead with no current user. Building caching inside the provider is the simplest correct solution today. The forward-compatibility path is documented in an ADR — deliberate deferral, not an oversight. A) OCP prevents modification of closed types; `IChatModelProvider` is an open extension point. C) SRP is overridden by YAGNI when the second concern has no current user. D) DRY applies to actual existing duplication, not anticipated future duplication.

**Exam domain:** Design infrastructure solutions  
**Cert:** AZ-305  
**Roadmap day:** Day-007

---

## Q18: Cache tier selection — AI workload

**Scenario:** An architect evaluates caching options for an AI chat API. User messages vary on every request, but a 3,500-token system prompt is identical for all requests. The goal is to reduce input-token billing. The team asks whether Azure Cache for Redis is the right choice.

**Question:** What is the correct architectural recommendation?

A) Redis is the correct choice — it is the standard caching layer for all Azure API workloads  
B) Provider-native prompt caching directly eliminates the billing for repeated system prompt tokens with no infrastructure cost; Redis would cache full responses (which vary per user message) and does not address the input-token billing problem  
C) Redis is required because provider-native caching APIs do not support system prompt blocks  
D) Use both Redis and provider-native caching together for maximum cost coverage  

**Answer:** B

**Why:** Provider-native caching directly targets the cost driver: the 3,500-token system prompt billed on every request. Cache reads cost ~10% of creation price — effectively eliminating 90% of system prompt billing with no infrastructure. Redis would need to cache the full response per user message, but user messages vary, making cache hits rare without semantic similarity evaluation (a non-trivial engineering investment). Redis solves a different problem and adds operational complexity not warranted here. A) Redis is a general-purpose solution, not universally optimal. C) is false — both Anthropic and Azure OpenAI provide native prompt caching for system prompt blocks. D) combining both adds cost and complexity for overlapping partial benefits.

**Exam domain:** Design infrastructure solutions  
**Cert:** AZ-305  
**Roadmap day:** Day-007

---

## Q19: Operational toggle via App Service configuration

**Scenario:** An AI gateway adds an `EnablePromptCaching` boolean setting controlling whether the `cache_control` annotation is sent to the provider. The team needs to disable caching in staging and enable it in production, and to toggle it without redeploying code during troubleshooting. The setting is currently hardcoded in `appsettings.json`.

**Question:** What is the correct pattern for making this setting operationally toggleable without a code deployment?

A) Store the flag in Azure Key Vault — operators update the secret value without redeploying  
B) Bind the flag to an App Service environment variable (`Anthropic__EnablePromptCaching`) via `IOptions<AnthropicOptions>` — update via Azure Portal or REST API without a code deployment  
C) Expose the flag as a query parameter on the API endpoint — each client decides whether to request caching  
D) Configure the flag in Azure API Management policies — route non-caching requests to a separate backend  

**Answer:** B

**Why:** App Service environment variables are the standard Azure pattern for operational configuration that changes without code deployment. The double-underscore notation (`Anthropic__EnablePromptCaching`) maps to nested `Anthropic:EnablePromptCaching` in the .NET configuration hierarchy; `IOptions<AnthropicOptions>` binds it at startup. Updating the variable on the App Service and restarting the app (or using a deployment slot swap) applies the change — no code change, no pipeline run. A) Key Vault stores credentials and secrets, not operational feature flags; it is operationally heavier for a simple boolean and requires vault reference wiring. C) exposing an internal implementation detail as a client parameter violates the gateway's abstraction. D) API Management routing adds infrastructure complexity for a configuration toggle that App Service environment variables solve directly.

**Exam domain:** Design infrastructure solutions  
**Cert:** AZ-305  
**Roadmap day:** Day-007

---

## Q20: Measuring caching ROI with KQL

**Scenario:** A team implements prompt caching and measures a 90% cache hit rate. Their FinOps team asks for a KQL query that estimates daily token-level savings in dollars. The team has `llm.cache.read_tokens` and `llm.cache.creation_tokens` logged as custom dimensions on the `claude.chat.api` dependency span. Token pricing: $3.00/M creation, $0.30/M cache reads.

**Question:** Which KQL query correctly estimates the daily dollar saving from caching?

A) `dependencies | where name == "claude.chat.api" | summarize savings=sum(toint(customDimensions["llm.cache.read_tokens"])) by bin(timestamp, 1d)`  
B) `dependencies | where name == "claude.chat.api" | summarize cache_reads=sum(toint(customDimensions["llm.cache.read_tokens"])) by bin(timestamp, 1d) | extend savings_usd=(cache_reads/1000000.0)*(3.00-0.30)`  
C) `customMetrics | where name == "ai.provider.cache.hits" | summarize sum(value) by bin(timestamp, 1d)`  
D) `traces | where customDimensions["llm.cache.read_tokens"] > 0 | summarize count() by bin(timestamp, 1d)`  

**Answer:** B

**Why:** Dollar saving per cache-read token = (creation price − read price) = $3.00/M − $0.30/M = $2.70/M. Option B sums the total cache read tokens per day and multiplies by the price differential, giving the daily saving — the tokens that were NOT billed at creation price. A) sums token counts without applying any dollar value, producing a number with no monetary meaning. C) counts cache hit events from the counter metric, not token volumes — a hit on 1 token and a hit on 4,000 tokens are counted equally. D) queries the wrong table (`traces` for ILogger output, not Activity spans) and counts requests, not tokens.

**Exam domain:** Design infrastructure solutions  
**Cert:** AZ-305  
**Roadmap day:** Day-007
