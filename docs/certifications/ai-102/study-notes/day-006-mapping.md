# Day 6 → AI-102 Mapping

## What Day 6 covered (build side)
- Structured logging via Serilog
- Application Insights (workspace-based) wired to App Service
- Correlation ID middleware (W3C trace context)
- LLM-specific telemetry: tokens-in, tokens-out, latency, model, provider
- Resilience pipeline: attempt timeout, circuit breaker; no retries on chat POST (non-idempotent, paid call — see ADR-006)
- Production error contract (no stack traces leaked)
- ADR-006 documenting the observability decision

## AI-102 objectives directly exercised
### Domain 1 — Plan and Manage
- [x] Monitor an Azure AI service
  - We monitored an Anthropic-backed gateway, but the *pattern* is identical
    for Azure OpenAI. Telemetry layer is provider-agnostic by design.
- [x] Manage costs for Azure AI services
  - Token telemetry feeds cost dashboards. Same pattern applies to Azure OpenAI
    PTU and pay-as-you-go monitoring.

### Domain 6 — Generative AI Solutions
- [x] Manage Azure OpenAI deployments (partial — pattern only)
  - Resilience and observability patterns transfer 1:1 when Azure OpenAI
    becomes a second provider.

## Likely exam-style questions Day 6 answers
1. *Which Azure service should you use to collect, query, and visualize
   telemetry from an AI workload?*
   → **Application Insights (workspace-based) backed by Log Analytics.**
2. *You need to correlate a single user request across the API gateway and
   the AI service call. What should you implement?*
   → **W3C Trace Context / correlation ID propagation via middleware.**
3. *Your AI gateway must continue serving traffic when the upstream model
   provider returns intermittent 503s. Which patterns apply?*
   → **Retry with exponential backoff + jitter, attempt timeout, circuit breaker.**
4. *Where should you store secrets used by the AI gateway in production?*
   → **Azure Key Vault, referenced from App Service app settings (not in code).**

## Gaps to study (NOT covered by Day 6 — read on Microsoft Learn)
- Azure OpenAI deployment types (Standard, Provisioned, Global)
- Content filters and content safety configuration
- Azure AI Foundry / Studio workflows
- Quota management and regional capacity planning
- Diagnostic settings vs Application Insights vs Azure Monitor (terminology nuance)

## Action items
- [ ] Read MS Learn module: "Monitor Azure OpenAI" (15 min)
- [ ] Read MS Learn module: "Manage Azure OpenAI Service costs" (15 min)
- [x] Run KQL queries — verified in App Insights post-deploy; token spans confirmed in `dependencies` table (see `docs/standards/kql-cookbook.md` for the queries)