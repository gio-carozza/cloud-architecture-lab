# Practice Questions — Plan and Manage an Azure AI Solution (AI-102 Domain 1)

---

## Q1: Cost attribution gap

**Scenario:** An AI platform team reports that their monthly Azure OpenAI spend has doubled over the past 60 days. Their gateway logs total token counts per request, but all requests share the same `operation_type` dimension in Application Insights. The team cannot determine whether the growth is driven by the synchronous chatbot path, the nightly batch summarization job, or both.

**Question:** What should the architect add to enable path-level cost attribution?

A) Enable Azure Cost Management budget alerts with a lower threshold  
B) Log a `path` or `operation_type` dimension (`"sync"` / `"batch"`) alongside token counts in every telemetry event  
C) Create separate Application Insights resources for the synchronous and batch services  
D) Switch from Application Insights to Azure Monitor Metrics for token tracking  

**Answer:** B

**Why:** Adding a `path` dimension to existing telemetry events enables per-path aggregation (KQL `summarize sum(tokens) by path`) without infrastructure changes. A) budget alerts fire after spend occurs — they cannot break down cost by path. C) separate AI resources would work but require routing changes and duplicate instrumentation; a single dimension tag is far cheaper. D) Azure Monitor Metrics have no request-level context — they aggregate at the resource level, not the processing-path level.

**Exam domain:** Plan and manage an Azure AI solution  
**Cert:** AI-102  
**Roadmap day:** Day-008

---

## Q2: Quota isolation for mixed workloads

**Scenario:** A company runs both a real-time customer support chatbot and a nightly document batch processing job from the same Azure OpenAI deployment. Every morning, users report that the chatbot is returning HTTP 429 (Too Many Requests) errors. Azure Monitor shows the deployment hit its TPM quota limit overnight during the batch run.

**Question:** What is the most appropriate architectural fix?

A) Reduce the number of documents processed in the nightly batch job  
B) Move the batch job to Azure OpenAI Global Batch, which uses separate enqueued-token quota  
C) Add retry logic with exponential backoff to the chatbot client  
D) Increase the TPM quota on the shared deployment to cover both workloads  

**Answer:** B

**Why:** Global Batch uses an isolated enqueued-token quota pool — batch processing cannot consume the TPM quota used by synchronous calls. This eliminates the quota contention at its architectural root. A) reducing batch volume treats the symptom, not the cause, and may not process all required documents. C) retry logic helps with transient errors but cannot fix systematic quota exhaustion caused by the batch job. D) increasing shared quota is more expensive and still leaves both workloads competing for the same pool — the next batch spike will exhaust the higher limit too.

**Exam domain:** Plan and manage an Azure AI solution  
**Cert:** AI-102  
**Roadmap day:** Day-008

---

## Q3: Operationalization readiness

**Scenario:** A team is preparing to move an Azure OpenAI-backed summarization service to production. The service works correctly in development. The team has unit tests passing, a CI pipeline, and structured logging to stdout. The platform architect flags the service as not production-ready.

**Question:** Which missing capability is the most likely reason for the flag?

A) The service does not use the latest Azure OpenAI model version  
B) The service has no distributed telemetry (Application Insights traces, cost-per-request logging, or alerting on error rate / quota usage)  
C) The service has not been load tested to its maximum expected throughput  
D) The service does not implement retry logic on the Azure OpenAI client  

**Answer:** B

**Why:** Structured logging to stdout provides no production visibility — it cannot be queried, alerted on, or correlated across requests. Without distributed telemetry (correlation IDs, token usage per request, error rate metrics, quota headroom alerts), the service is operationally blind. A) model version affects quality, not operability. C) load testing is valuable but is not a baseline operationalization requirement — telemetry is. D) retry logic is a resilience concern; the architect flagged operationalization readiness, which centres on observability and governance.

**Exam domain:** Plan and manage an Azure AI solution  
**Cert:** AI-102  
**Roadmap day:** Day-008

---

## Q4: Model selection governance

**Scenario:** An enterprise AI team has three workloads: (1) real-time customer chat (latency-sensitive, moderate complexity), (2) nightly legal document summarization (offline, high accuracy required, long context), and (3) intent classification at API ingress (sub-100ms, simple task). All three currently use the same `gpt-4o` deployment. The architect recommends splitting models by workload.

**Question:** What is the primary reason to use different models for different workloads rather than a single high-capability model for all?

A) Azure policy requires a separate deployment per workload  
B) Matching model capability and cost to workload requirements reduces per-token spend without sacrificing quality  
C) Using the same model across workloads creates data residency compliance issues  
D) A single model cannot handle both conversational and document summarization tasks simultaneously  

**Answer:** B

**Why:** A high-capability, high-cost model (e.g., GPT-4o) applied to a simple classification task wastes token budget — a smaller, cheaper model handles it at equal accuracy and a fraction of the cost. The routing principle is: cheapest model that meets the quality bar, escalate on need. A) no such Azure policy exists. C) data residency is a deployment/region concern, not a model selection concern. D) a single model can handle multiple task types; the constraint is cost, not capability.

**Exam domain:** Plan and manage an Azure AI solution  
**Cert:** AI-102  
**Roadmap day:** Day-008

---

## Q5: Responsible AI — content safety integration

**Scenario:** A healthcare company is deploying an Azure OpenAI chatbot that answers patient questions about medications. During review, the risk team flags a requirement: the system must prevent harmful or medically dangerous outputs from reaching patients, and the prevention must be auditable.

**Question:** Which Azure service should the architect integrate with the AI gateway to meet this requirement?

A) Azure API Management with rate limiting policies  
B) Azure AI Content Safety, configured with custom blocklists and harm category thresholds, with filtering decisions logged to Application Insights  
C) Azure Key Vault to store and rotate the OpenAI API key  
D) Azure Active Directory Conditional Access to restrict which users can query the chatbot  

**Answer:** B

**Why:** Azure AI Content Safety evaluates both input prompts and model outputs against harm categories (violence, hate, sexual, self-harm) and supports custom blocklists for domain-specific terms. Logging filtering decisions to Application Insights creates the audit trail the risk team requires. A) API Management enforces rate limits and auth — it has no content evaluation capability. C) Key Vault is a secret management service; it cannot evaluate response content. D) Conditional Access controls access at the identity layer, not at the content layer — a credentialed user could still receive a harmful response.

**Exam domain:** Plan and manage an Azure AI solution  
**Cert:** AI-102  
**Roadmap day:** Day-008

---

## Q6: Structured logging — PII risk

**Scenario:** A developer implements structured logging for an AI gateway. To make debugging easy, the logger emits the full user prompt and LLM completion text to Application Insights on every request.

**Question:** What risk does this logging approach introduce, and what should be logged instead?

A) Logging full prompt text is fine — Application Insights encrypts all data at rest  
B) Logging full prompt text creates a PII exposure and data governance risk; log token counts, model IDs, latency, and correlation IDs instead — never log actual prompt content unless under a controlled access and data retention policy  
C) Logging full prompt text increases latency — use async logging to avoid the performance impact  
D) Logging full prompt text is required for AI-102 compliance; it cannot be omitted  

**Answer:** B

**Why:** Full prompt and completion text may contain user PII, confidential business data, or sensitive context. Storing this in Application Insights exposes it to everyone with workspace read access and may violate GDPR, HIPAA, or internal data governance policies. Token counts, latency, model IDs, and correlation IDs provide full operational visibility without content exposure. A) encryption at rest doesn't address access control or data classification compliance. C) logging latency is irrelevant to the PII risk. D) AI-102 does not require logging prompt content.

**Exam domain:** Plan and manage an Azure AI solution  
**Cert:** AI-102  
**Roadmap day:** Day-006

---

## Q7: Correlation ID propagation

**Scenario:** An AI gateway receives requests with an `X-Correlation-Id` header from a client. An incident occurs where the LLM provider call fails. The on-call engineer queries Application Insights but cannot correlate the gateway log entries with the client's reported correlation ID because the gateway generates its own internal trace IDs.

**Question:** What should the architect add to ensure the client's correlation ID is traceable end-to-end in Application Insights?

A) Ask clients to stop sending `X-Correlation-Id` and use Application Insights' automatic trace IDs instead  
B) Add a `CorrelationIdMiddleware` that reads `X-Correlation-Id` from the request header (or generates a new GUID if absent), stores it in `HttpContext.Items`, and enriches all log lines via `LogContext.PushProperty("CorrelationId", ...)`  
C) Log the `X-Correlation-Id` header once at request entry — it will automatically propagate to all downstream log entries  
D) Enable Application Insights distributed tracing — it automatically reads and propagates all custom headers  

**Answer:** B

**Why:** Correlation ID propagation requires explicit middleware: read the header at ingress, store it in request context, and push it onto the Serilog `LogContext` so every log line in that request's scope includes it. This makes the value queryable in KQL: `traces | where customDimensions["CorrelationId"] == "client-provided-id"`. A) discarding the client's ID breaks end-to-end traceability across system boundaries. C) logging it once at entry doesn't enrich subsequent log calls in the same request. D) App Insights distributed tracing handles W3C `traceparent` headers, not arbitrary custom correlation IDs.

**Exam domain:** Plan and manage an Azure AI solution  
**Cert:** AI-102  
**Roadmap day:** Day-006

---

## Q8: Token telemetry — metric vs. log

**Scenario:** An AI gateway logs token counts per request as unstructured text: `"INFO: request completed, tokens: 1200 input 340 output"`. The FinOps team asks for a time-series chart of hourly token spend by model. The data exists in logs but is impossible to aggregate without parsing every log entry.

**Question:** What is the correct approach for making token data aggregatable?

A) Parse the existing log text with a KQL `parse` expression to extract the numbers  
B) Emit token counts as structured custom events with typed dimensions (`llm.tokens.input`, `llm.tokens.output`, `llm.model`) AND as OpenTelemetry Counter instruments — enabling both per-request queries and time-series aggregation  
C) Export logs to Azure Storage and process with Azure Data Factory to build the time-series  
D) Application Insights automatically parses numeric values from log text  

**Answer:** B

**Why:** Structured custom events with typed dimensions enable KQL `summarize sum(tolong(customDimensions["llm.tokens.input"])) by bin(timestamp, 1h)`. OTel `Counter<long>` instruments feed the `customMetrics` table, enabling Azure Monitor time-series charts and alert rules without scanning full log volumes. A) KQL `parse` is brittle (text format changes break it) and requires a full table scan for every aggregation query. C) Data Factory adds latency and cost for a problem solvable at the telemetry design layer. D) Application Insights does not automatically type values from unstructured log strings.

**Exam domain:** Plan and manage an Azure AI solution  
**Cert:** AI-102  
**Roadmap day:** Day-006

---

## Q9: Error classification — auth vs. transient

**Scenario:** An AI gateway serves a high-traffic production environment. The Anthropic API key expires on a Sunday evening. For the next 4 hours, the gateway retries every failed request 3 times (12 seconds of extra latency per request), sends 4× the normal request volume to Anthropic, and logs 50,000 `Error`-level events — drowning out the original alert.

**Question:** Which error classification rule, if implemented, would have prevented this?

A) Reduce `MaxRetryAttempts` from 3 to 1 to limit the volume of retry traffic  
B) Classify 401 responses as non-retriable — fail immediately with an `Error` log, alert Severity 1, and return `401` to the caller without exhausting retry budget  
C) Set a shorter attempt timeout (5 seconds) so retries complete faster  
D) Enable the circuit breaker — it would have opened after the first few failures and stopped retries  

**Answer:** B

**Why:** A 401 indicates an invalid or expired credential — a condition that cannot be resolved by retrying. Classifying 401 as non-retriable means: first attempt fails, log `Error`, fire Severity 1 alert, return 401 to caller immediately. Zero wasted retries, accurate alert, no amplification. A) reducing retries reduces waste but still retries non-retriable errors. C) shorter timeouts reduce per-request delay but still perform useless retries. D) the circuit breaker would eventually open but takes minutes to trigger and still retries during the ramp-up; error classification prevents the first retry.

**Exam domain:** Plan and manage an Azure AI solution  
**Cert:** AI-102  
**Roadmap day:** Day-006

---

## Q10: Safe error contract

**Scenario:** An AI gateway's global exception handler catches an unhandled exception from the LLM provider SDK and returns the full exception message in the API response body, including the internal stack trace and the Anthropic API URL being called.

**Question:** What security and operational risk does this create, and what is the correct response contract?

A) No risk — stack traces help clients debug their own integration  
B) Information disclosure risk: internal stack traces and infrastructure details help attackers map the system. Return only `{ "error": "An unexpected error occurred.", "correlationId": "..." }` and log full exception detail server-side with the correlation ID  
C) The risk is latency — stack trace serialization is slow; use async exception handling  
D) Return HTTP 200 with an error field so clients don't need special error handling  

**Answer:** B

**Why:** Stack traces in API responses are an OWASP Top 10 information disclosure vulnerability. They reveal internal class names, file paths, dependency versions, and infrastructure endpoints — all useful for targeted attacks. The correct contract: return a sanitised message + correlation ID to the caller so they can report it; log the full exception + correlation ID server-side so the on-call engineer can diagnose. A) clients have no need for internal stack traces. C) serialization latency is trivial — the concern is security. D) HTTP 200 for errors breaks REST semantics and prevents proper client error handling.

**Exam domain:** Plan and manage an Azure AI solution  
**Cert:** AI-102  
**Roadmap day:** Day-006

---

## Q11: Prompt caching — minimum token threshold

**Scenario:** A team implements Anthropic prompt caching on their AI gateway. They annotate a 500-token system prompt with `cache_control: {"type":"ephemeral","ttl":"1h"}`. After a week in production, their App Insights telemetry shows `cache_creation_input_tokens: 0` on every request with no API errors.

**Question:** What is the most likely cause of zero cache creation tokens?

A) The `ttl` field is not supported for Anthropic Claude 4 models — remove it  
B) The system prompt is below the 1,024-token minimum cacheable block size — the annotation is silently ignored  
C) The App Insights OpenTelemetry exporter strips cache token fields from dependency spans  
D) Prompt caching requires a dedicated Anthropic API key with the "caching" scope enabled  

**Answer:** B

**Why:** Anthropic requires a minimum of 1,024 tokens in a cacheable block for Claude 3+ models. A 500-token system prompt falls below this threshold — the `cache_control` annotation is accepted without error, but the cache block is never created and `cache_creation_input_tokens` is 0 on every request. A) the `ttl` field IS required for Claude 4 models and its presence is correct; the issue is the token count, not the TTL. C) App Insights does not strip custom dimensions set via OpenTelemetry Activity tags. D) no separate API key or scope is required for prompt caching.

**Exam domain:** Plan and manage an Azure AI solution  
**Cert:** AI-102  
**Roadmap day:** Day-007

---

## Q12: Prompt caching — placement in provider abstraction

**Scenario:** An architect is adding prompt caching to an AI gateway with a `IChatModelProvider` interface, a `ClaudeChatModelProvider` implementation, and a provider-agnostic `ChatRequest` / `ChatResponse` contract. Proposal A places caching in a `CachingChatModelProvider` decorator above the provider seam. Proposal B places it inside `ClaudeApiClient` (below the seam). The constraint: `ChatRequest` and `IChatModelProvider` must not change.

**Question:** Which placement is architecturally correct?

A) Proposal A — `CachingChatModelProvider` decorator above the seam because caching is a cross-cutting concern  
B) Proposal B — inside `ClaudeApiClient` below the seam, because `cache_control` is Anthropic API-specific and must not leak into the provider-agnostic contract  
C) In the controller layer — caching should be transparent to all downstream components  
D) In a shared base class accessible to all providers — ensures future consistency  

**Answer:** B

**Why:** The `cache_control` annotation is an Anthropic-specific wire format. Placing it in a decorator above `IChatModelProvider` would require `ChatRequest` to carry Anthropic-specific fields, violating the provider-agnostic contract. Placing it inside `ClaudeApiClient` keeps the seam clean — `ChatRequest` and `IChatModelProvider` remain unchanged, and the detail is encapsulated in the implementation. The accepted cost (documented in ADR-009): when a second cacheable provider arrives, this code requires extraction to a decorator. A) the decorator is the right long-term pattern but at the wrong level if `ChatRequest` must stay agnostic. C) controllers have no access to provider-specific payload construction. D) a shared base class coupling all providers to Anthropic-specific constructs breaks the abstraction.

**Exam domain:** Plan and manage an Azure AI solution  
**Cert:** AI-102  
**Roadmap day:** Day-007

---

## Q13: Cache hit rate — KQL query

**Scenario:** An AI gateway logs `llm.cache.read_tokens` and `llm.cache.creation_tokens` as custom dimensions on the `claude.chat.api` dependency span. The team wants a KQL query that calculates the cache hit rate (percentage of requests where a cache read occurred) per hour to validate that caching is working and to set an alert threshold.

**Question:** Which KQL query correctly calculates the per-hour cache hit rate?

A) `traces | where customDimensions["llm.cache.read_tokens"] > 0 | summarize count() by bin(timestamp, 1h)`  
B) `dependencies | where name == "claude.chat.api" | summarize hits=countif(toint(customDimensions["llm.cache.read_tokens"]) > 0), total=count() by bin(timestamp, 1h) | extend hitRate=hits*100.0/total`  
C) `customMetrics | where name == "ai.provider.cache.hits" | summarize sum(value) by bin(timestamp, 1h)`  
D) `requests | summarize hitRate=avg(toint(customDimensions["llm.cache.read_tokens"])) by bin(timestamp, 1h)`  

**Answer:** B

**Why:** Cache hit rate is the proportion of requests where `cache_read_input_tokens > 0`. The query must: use `dependencies` (where Activity spans land, not `traces`), filter to the correct span name, count hits where cache read tokens are non-zero, divide by total, and group by time bucket. A) uses the wrong table (`traces` holds ILogger output) and counts only hit requests, not the rate. C) counts raw hit counter events but cannot compute a rate without the total denominator. D) uses `requests` (inbound HTTP, not LLM calls) and averages token counts rather than computing a rate.

**Exam domain:** Plan and manage an Azure AI solution  
**Cert:** AI-102  
**Roadmap day:** Day-007

---

## Q14: Silent cache failure after model migration

**Scenario:** A team migrates an AI gateway from `claude-sonnet-3-7` to `claude-sonnet-4-6`. Prior to migration, prompt caching worked correctly (cache creation tokens confirmed via telemetry). After migration, `cache_creation_input_tokens` drops to 0 on all requests. The system prompt is 3,500 tokens. The `cache_control` annotation uses `{"type":"ephemeral"}` with no `ttl` field. No API errors are returned.

**Question:** What is causing the cache failure after the model migration?

A) `claude-sonnet-4-6` does not support prompt caching  
B) The system prompt exceeds the new model's per-request cache size limit  
C) Claude 4 models require an explicit `ttl` field in `cache_control` — `{"type":"ephemeral"}` without TTL is silently ignored, producing 0 cache creation tokens with full input billing  
D) The model ID must be embedded in the `cache_control` block alongside `type` and `ttl`  

**Answer:** C

**Why:** Claude 3 models accept `{"type":"ephemeral"}` without a TTL. Claude 4 models require `{"type":"ephemeral","ttl":"1h"}` (or `"5m"`) — the TTL field is mandatory. Without it, the annotation is accepted with no error, but 0 cache creation tokens are returned and full input billing applies. This is a silent behavioral difference between model generations that has no error signal. A) `claude-sonnet-4-6` fully supports prompt caching. B) 3,500 tokens is well above the 1,024-token minimum and unrelated to cache failure. D) model IDs appear in the top-level request body, not inside `cache_control` blocks.

**Exam domain:** Plan and manage an Azure AI solution  
**Cert:** AI-102  
**Roadmap day:** Day-007

---

## Q15: Estimating prompt caching daily savings

**Scenario:** An AI gateway makes 15,000 synchronous requests per day. Each request includes a 3,000-token system prompt. LLM pricing: $3.00/M input tokens, $0.30/M cache read tokens, $3.75/M cache creation tokens. After implementing prompt caching, telemetry shows a 90% cache hit rate — 13,500 requests are cache reads and 1,500 create the cache.

**Question:** What is the approximate daily cost saving from prompt caching compared to running without caching?

A) $0 — prompt caching only reduces latency, not direct billing  
B) ~$78/day  
C) ~$108/day  
D) ~$135/day  

**Answer:** C

**Why:** Without caching: 15,000 × 3,000 × $3.00/M = $135/day. With caching: (1,500 × 3,000 × $3.75/M) = $16.88 + (13,500 × 3,000 × $0.30/M) = $12.15 → total ≈ $29/day. Daily saving = $135 − $29 ≈ $106/day ≈ $108/day (rounded). The key insight: cache reads at $0.30/M are ~10% of the full input price, reducing the per-request cost of the system prompt by ~90% on cache hits. A) is false — prompt caching directly reduces per-token billing; cache reads are cheaper than full input tokens. B) misapplies creation vs. read token pricing. D) is the uncached cost, not the saving.

**Exam domain:** Plan and manage an Azure AI solution  
**Cert:** AI-102  
**Roadmap day:** Day-007
