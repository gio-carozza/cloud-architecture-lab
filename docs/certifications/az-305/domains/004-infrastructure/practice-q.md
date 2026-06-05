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
