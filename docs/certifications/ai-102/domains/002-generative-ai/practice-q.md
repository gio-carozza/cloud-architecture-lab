# Practice Questions — Implement Generative AI Solutions (AI-102 Domain 2)

---

## Q1: Choosing the right processing path

**Scenario:** A financial services company runs a .NET AI gateway backed by Azure OpenAI. Every night at 2 AM they need to summarize 8,000 customer support transcripts for a compliance report delivered at 8 AM. The current implementation calls the chat completions endpoint synchronously for each transcript, taking 4+ hours and consuming the full TPM quota, which causes 429 errors for daytime users.

**Question:** What change should the architect make?

A) Increase the TPM quota on the Azure OpenAI deployment to handle both workloads  
B) Move the nightly summarization to the Global Batch API with a separate enqueued-token quota  
C) Add retry logic with exponential backoff to handle the 429 errors  
D) Run the summarization during business hours to spread the load across the day  

**Answer:** B

**Why:** Global Batch uses separate enqueued-token quota — it cannot starve the synchronous deployment's TPM quota. The 6-hour batch window fits comfortably in the 24-hour SLA and costs 50% less per token. A) increases cost without fixing the quota isolation problem. C) adds resilience to a flawed design without addressing root cause. D) moves the problem to daytime, worsening the conflict with live users.

**Exam domain:** Implement generative AI solutions  
**Cert:** AI-102  
**Roadmap day:** Day-008

---

## Q2: Batch API lifecycle phases

**Scenario:** A developer is implementing the Anthropic Messages Batch API for an offline content moderation pipeline. After calling the submit endpoint, the API returns an HTTP 200 with a batch ID and status `"in_progress"`. The developer calls the results endpoint immediately and receives an error.

**Question:** What is the correct next step after receiving the batch ID?

A) Retry the results endpoint with exponential backoff until content is returned  
B) Poll the batch status endpoint until status is `"ended"`, then call the results endpoint  
C) Submit a cancellation and resubmit — `in_progress` means the request failed  
D) Wait exactly 24 hours before calling the results endpoint  

**Answer:** B

**Why:** The batch pattern is submit → **poll status** → retrieve results. The status endpoint (not the results endpoint) indicates readiness — poll it until the terminal state (`ended` for Anthropic; `completed` for Azure OpenAI), then retrieve results. A) hitting the results endpoint before completion returns an error, not content. C) `in_progress` is normal and expected — it means processing is underway. D) 24 hours is the SLA ceiling, not a fixed wait time; most jobs complete much sooner.

**Exam domain:** Implement generative AI solutions  
**Cert:** AI-102  
**Roadmap day:** Day-008

---

## Q3: Budget enforcement placement

**Scenario:** An enterprise AI gateway team notices that a misconfigured client submitted a batch job with 500,000 requests, exhausting the month's Azure OpenAI batch quota in a single run. The team already has Azure Cost Management budgets and email alerts configured, but the alert only fired after the quota was consumed.

**Question:** What architectural control would have prevented the quota exhaustion?

A) Set the Azure Cost Management budget alert threshold lower so it fires earlier  
B) Require all batch submissions to go through a gateway layer that validates request count against a hard cap before enqueuing  
C) Enable Azure OpenAI content filtering to reject oversized requests  
D) Add a retry limit to the batch client so it stops after a quota error  

**Answer:** B

**Why:** Pre-flight validation at the gateway boundary is the only control that rejects cost *before* it occurs. The gateway estimates token cost from the payload and returns HTTP 400/402 if the cap is exceeded — no tokens are consumed. A) cost alerts are reactive — they fire after spend happens, not before. C) content filtering is for safety/policy, not cost; it has no concept of request count. D) retry limits reduce repeat failures but cannot prevent the initial oversized submission from draining quota.

**Exam domain:** Implement generative AI solutions  
**Cert:** AI-102  
**Roadmap day:** Day-008

---

## Q4: Provider abstraction for batch

**Scenario:** A company currently uses the Anthropic Batch API for offline workloads. They want to migrate to Azure OpenAI Global Batch without rewriting the core processing pipeline. The gateway is built on a provider abstraction (`IBatchProvider`) with `SubmitAsync`, `GetStatusAsync`, and `GetResultsAsync` methods.

**Question:** What does the provider abstraction pattern enable in this migration?

A) It allows the same JSONL request format to be used with both providers without modification  
B) It isolates provider-specific API details behind a common interface, so the pipeline switches providers by swapping the registered implementation  
C) It eliminates the need to manage separate quota pools for Anthropic and Azure OpenAI  
D) It automatically translates Anthropic model parameter names to Azure OpenAI equivalents at runtime  

**Answer:** B

**Why:** The abstraction seam (`IBatchProvider`) maps the common three-phase semantic (submit/poll/retrieve) to each provider's specific API, keeping provider-specific details out of the pipeline. Registering a new implementation in DI is the only change required in the consuming code. A) JSONL formats differ between providers — the abstraction hides this, but the formats are not identical. C) quota is a deployment concern, not a code concern — the abstraction does not affect it. D) model parameter translation is a concern of the provider implementation, not a "automatic" runtime feature.

**Exam domain:** Implement generative AI solutions  
**Cert:** AI-102  
**Roadmap day:** Day-008

---

## Q5: Cost optimization — batch vs. synchronous tradeoff

**Scenario:** A retail company uses Azure OpenAI to power two workloads: (1) a customer-facing chatbot that answers questions in real time, and (2) a weekly product catalog enrichment job that generates descriptions for 20,000 SKUs. Both currently use the same synchronous chat completions deployment. The team wants to cut LLM costs without degrading the chatbot experience.

**Question:** What is the most cost-effective architectural change for the catalog enrichment workload?

A) Increase the deployment's model tier to a more efficient model for both workloads  
B) Move the catalog enrichment job to Azure OpenAI Global Batch on a separate deployment, keeping the chatbot on synchronous  
C) Rate-limit the catalog enrichment job to run at 10 requests per second to reduce costs  
D) Cache all catalog enrichment responses and skip re-generation for SKUs that haven't changed  

**Answer:** B

**Why:** Global Batch prices at 50% of synchronous rate and uses separate quota, delivering the maximum cost reduction for a deferrable, non-interactive workload while completely isolating it from the chatbot's TPM quota. A) switching model tiers affects both workloads and may degrade chatbot quality — it also doesn't match the batch discount structure. C) rate-limiting reduces throughput but not per-token cost — same price, just slower. D) caching is a valid secondary optimization but doesn't address the per-token cost of new or changed SKUs, and the question asks for the most cost-effective architectural change.

**Exam domain:** Implement generative AI solutions  
**Cert:** AI-102  
**Roadmap day:** Day-008

---

## Q6: Streaming response buffering on App Service

**Scenario:** A developer deploys a .NET 8 gateway to Azure App Service (Linux). The `/chat/stream` endpoint returns `Content-Type: text/event-stream` and works correctly in local testing — tokens appear incrementally. After Azure deploy, the same endpoint delivers all tokens in a single burst after a multi-second delay.

**Question:** What is the most likely cause and minimum fix?

A) The Azure App Service SKU doesn't support streaming; upgrade to Premium tier  
B) `X-Accel-Buffering: no` response header is missing; nginx on App Service buffers SSE by default  
C) The HttpClient `Timeout` is too short, causing the connection to reset mid-stream  
D) `Cache-Control: no-cache` is missing; App Service CDN is caching the stream  

**Answer:** B

**Why:** Azure App Service on Linux uses nginx as a reverse proxy. Nginx buffers responses by default, accumulating all SSE chunks before forwarding — producing exactly the observed behavior (incremental locally, all-at-once on Azure). The fix is `X-Accel-Buffering: no` in the response headers, which instructs nginx to disable buffering for that response. A) is false — no SKU restriction on streaming exists. C) would cause errors or truncation, not buffering. D) `no-cache` prevents HTTP caching but doesn't affect nginx proxy buffering; App Service doesn't apply a CDN by default.

**Exam domain:** Implement generative AI solutions  
**Cert:** AI-102  
**Roadmap day:** Day-009

---

## Q7: TTFT vs. total latency SLO

**Scenario:** Your team is setting an SLA for an AI chat gateway using SSE streaming. The product manager asks which latency metric to use. An engineer proposes p95 total response time (first byte to last token). An architect proposes p95 time-to-first-token (TTFT).

**Question:** Which metric is more appropriate for the SLA, and why?

A) Total response time — it captures the full user-visible delay and is simpler to measure  
B) TTFT — it directly maps to perceived latency in a streaming product where users see tokens as they arrive  
C) Mean total response time — averages smooth outliers and give a fairer picture  
D) Token throughput (tokens/second) — it captures rendering speed better than a latency metric  

**Answer:** B

**Why:** In a streaming product, users begin reading after the first token arrives. TTFT controls whether the experience feels instant or broken. A 200ms TTFT followed by 4 seconds of streaming feels fast; a 4-second blank screen followed by instant text feels broken — even if total duration is identical. A) total response time is correct for synchronous endpoints but misleading for streaming. C) means hide the outlier spikes that are exactly what users experience as "broken." D) token throughput matters for reading comfort but doesn't address the blank-screen problem.

**Exam domain:** Implement generative AI solutions  
**Cert:** AI-102  
**Roadmap day:** Day-009

---

## Q8: Mid-stream error handling

**Scenario:** A gateway begins streaming tokens to a browser client. Partway through the response, the upstream provider returns HTTP 429 (Too Many Requests). The developer needs to surface this error to the client.

**Question:** What is the correct approach?

A) Interrupt the stream and return HTTP 503 to the client  
B) Log the error server-side and close the stream silently without notifying the client  
C) Emit an SSE `event: error` frame with a safe error payload, then close the stream  
D) Buffer remaining retried tokens and append them to the stream once the 429 resolves  

**Answer:** C

**Why:** Once the gateway sends `Content-Type: text/event-stream` and the first data bytes, the HTTP 200 status code is committed — it cannot be changed to 503. The correct contract is to emit a structured SSE error event (e.g., `event: error\ndata: {"code":"stream_error","correlationId":"..."}`), then close the stream cleanly. A) is impossible — status is already committed. B) leaves the client waiting indefinitely with no indication of failure. D) retrying a 429 inside the same stream compounds rate limiting and introduces unpredictable delays.

**Exam domain:** Implement generative AI solutions  
**Cert:** AI-102  
**Roadmap day:** Day-009

---

## Q9: Client disconnect and upstream resource management

**Scenario:** A streaming gateway is connected to an LLM provider. The user closes the browser tab mid-stream. The gateway continues processing and incurs token charges for the rest of the response, even though no client is reading the output.

**Question:** What is the correct fix?

A) Add a server-side timeout that cuts off all streams after 30 seconds  
B) Wire `HttpContext.RequestAborted` as the CancellationToken passed to the upstream LLM HTTP call  
C) Wrap the streaming loop in a try/catch and suppress all OperationCancelledException exceptions  
D) Set `client.Timeout = TimeSpan.FromSeconds(10)` on the HttpClient  

**Answer:** B

**Why:** ASP.NET Core sets `HttpContext.RequestAborted` when the client disconnects. Passing this token to the upstream `SendAsync` call causes the LLM HTTP connection to cancel the moment the client disconnects — stopping both token charges and network I/O. A) blunt timeout penalises long-but-valid responses and doesn't specifically respond to disconnect. C) catching and suppressing exceptions without cancelling the upstream call doesn't stop token generation. D) a short `client.Timeout` cuts off valid long responses and causes errors for legitimate requests.

**Exam domain:** Implement generative AI solutions  
**Cert:** AI-102  
**Roadmap day:** Day-009

---

## Q10: Provider interface design for streaming

**Scenario:** An AI gateway has `IChatModelProvider` with `Task<ChatResponse> SendAsync(...)`. The team needs to add streaming. A junior engineer proposes `IStreamingChatModelProvider` with `IAsyncEnumerable<ChatChunk> StreamAsync(...)`. A senior engineer proposes extending `IChatModelProvider` directly with `StreamAsync`.

**Question:** Which approach is architecturally correct, and what is the deciding test?

A) New interface — Interface Segregation always requires separate interfaces for different return types  
B) Extend the existing interface — streaming is the same operation with different delivery; a non-streaming provider degrades gracefully by yielding one terminal chunk  
C) New interface — the batch API used this pattern, so consistency requires streaming to do the same  
D) Content negotiation on the existing endpoint — no interface change is needed  

**Answer:** B

**Why:** The deciding test is Liskov Substitutability: can a provider that doesn't natively stream implement the method without throwing? For streaming, yes — it wraps `SendAsync` and yields one terminal chunk. For batch (`IBatchChatModelProvider`), no — a non-batch provider has no valid submit/poll/retrieve path and must throw. This asymmetry is why batch earned its own interface and streaming doesn't. A) ISP requires new interfaces when consumers are forced to depend on operations they never call; the same controller uses both buffered and streamed chat. C) copying batch's form while ignoring why batch split is cargo-culting. D) content negotiation relocates the problem; the seam design question remains.

**Exam domain:** Implement generative AI solutions  
**Cert:** AI-102  
**Roadmap day:** Day-009
