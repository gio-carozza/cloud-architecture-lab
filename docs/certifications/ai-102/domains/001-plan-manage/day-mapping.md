# Day Mapping — Plan and Manage an Azure AI Solution (AI-102 Domain 1)

| Day | Topics Covered |
|-----|----------------|
| Day-006 | Structured logging for AI workloads (Serilog → Azure Monitor OTel exporter); correlation ID middleware for distributed tracing; token telemetry as cost metric (input/output/cache tokens as Counter instruments + custom events with model/provider/path dimensions); error classification for LLM calls (4xx client / 401-403 auth / 429 throttle / 5xx transient / timeout); safe error contracts (correlationId only in response, full detail in logs); Application Insights as the observability sink |
| Day-007 | Prompt caching for AI workload cost management (cache_control annotation, ≥1024-token minimum, TTL required for Claude 4 models, 90% input-token cost reduction); cache hit rate as a first-class operational metric (cache_read vs. cache_creation_input_tokens, KQL hit-rate and savings queries); placement of caching inside vs. outside the provider boundary (ADR-009: inside ClaudeApiClient, not in a decorator above IChatModelProvider) |
| Day-008 | Cost-per-token attribution by processing path (sync vs. batch); operationalizing a generative AI gateway (telemetry, cost governance, routing correctness); quota isolation for mixed synchronous/batch workloads; model selection governance; responsible AI — Azure AI Content Safety integration |
