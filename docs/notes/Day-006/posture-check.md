# Day 6 — Posture Check

> Fill this at the END of Day 6, BEFORE marking the day complete.
> Honest answers only. The graveyard is more valuable than the trophy case.

## 1. Whose problem did I actually solve today?
The on-call engineer at 3 AM who gets paged when `/api/ai/chat` starts
returning 500s. Without Day 6's work, they'd be staring at a black box.
With it, they have correlation IDs, structured logs, latency percentiles,
token telemetry, and KQL queries to answer "what happened?" in minutes
instead of hours.

Secondary user: the finance/FinOps person who needs to know what this
gateway costs in tokens per hour.

*If "the platform" was your first instinct, push harder until you can name a human.*

## 2. What would I refuse to ship if I were the only one in the room?
- [ ] Returning raw exception messages or stack traces to clients (info disclosure)
- [ ] Logging full prompt bodies (PII + secrets risk)
- [ ] Retrying on 401s (wastes budget, alarms upstream)
- [ ] Skipping the circuit breaker because "Anthropic is reliable enough"
       (cascade failures are how outages compound)

If any of these slipped in under deadline pressure, name it here:
I would stand my ground and not compromise quality over deadline.

## 3. What did I try, fail at, and learn?
<add each entry to the Graveyard table in docs/standards/_principles.md>

Examples to watch for on Day 6:
- Mis-set `Anthropic__ApiKey` with single underscore → 401 → learned App Service env var → IConfiguration translation rule
- Tried Polly directly before discovering Microsoft.Extensions.Http.Resilience → wasted X minutes → learned to check the Microsoft-maintained wrapper first

## 4. Could I explain today's work to a 10-year-old AND defend it at a doctorate level?

### 10-year-old version
"We taught the AI gateway to keep a really detailed diary. Every time
someone asks the AI a question, it writes down what was asked, how long
it took, how much it cost, and what went wrong if anything broke. We
also taught it to be patient — if the AI service is busy, it waits and
tries again instead of giving up. And if the service is really sick, it
stops bothering it for a bit so it can recover."

### Doctorate-level version
"Implemented a structured observability layer using Serilog with the
Application Insights sink, backed by a workspace-based Log Analytics
deployment. W3C Trace Context propagation via custom middleware enables
end-to-end correlation across the gateway and provider boundaries.
LLM-specific telemetry (input/output tokens, model identifier, provider
latency) is emitted as activity tags for downstream cost attribution and
SLO computation. Resilience is provided by the standard resilience
handler from Microsoft.Extensions.Http.Resilience, configured with
jittered exponential retry, attempt timeout, and a closed/open/half-open
circuit breaker keyed on failure ratio. Errors are classified at the
provider boundary into transient/auth/throttle/internal categories,
each mapped to distinct retry policies and HTTP status responses. The
production error contract surfaces only a correlation identifier and a
safe message, with full exception detail logged server-side. ADR-006
captures the decision to defer OpenTelemetry adoption until the addition
of a second service warrants the orchestration overhead."

If you cannot deliver both versions cleanly, schedule a teach-back
session before moving to Day 7. Owning a concept means being fluent in
both registers.