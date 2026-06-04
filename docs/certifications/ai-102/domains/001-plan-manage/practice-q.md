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
