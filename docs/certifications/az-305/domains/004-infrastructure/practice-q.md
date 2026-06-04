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
