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
Full entries in the Graveyard table (`docs/standards/_principles.md`). Summary:

- **`SamplingDuration=30s` + `AttemptTimeout=45s`** → startup validator threw. v10 enforces `SamplingDuration >= 2x AttemptTimeout`. Learned: startup validators catch a class of semantic errors no compiler can.
- **`MaxRetryAttempts=0`** → v10 rejects it. Intent ("no retries on chat POST") unchanged; encoding shifted to `MaxRetryAttempts=1` + `ShouldHandle=false`. Learned: ADRs document intent, code expresses it within current library constraints.
- **`Serilog.Sinks.ApplicationInsights` + `Microsoft.ApplicationInsights.AspNetCore`** → nearly installed both before catching that Day 5 already shipped OTel-first. Parallel pipelines would have duplicated every signal. Learned: check the existing dependency graph before following a plan written before the system existed.
- **Azure CLI SSL resets on `management.azure.com`** → TLS interception drops connections mid-handshake on this network. Workaround: `Invoke-RestMethod` with ARM bearer token (Windows TLS stack). `az config set core.disable_ssl_certificate_verification=true` only helps for reads.

## 4. Could I explain today's work to a 10-year-old AND defend it at a doctorate level?

### 10-year-old version
"We taught the AI gateway to keep a really detailed diary. Every time
someone asks the AI a question, it writes down what was asked, how long
it took, how much it cost, and what went wrong if anything broke. We
also taught it to be patient — if the AI service is busy, it waits and
tries again instead of giving up. And if the service is really sick, it
stops bothering it for a bit so it can recover."

### Doctorate-level version
"Implemented a structured observability layer using Serilog as a logging
library within a single OpenTelemetry export pipeline (ADR-008). Serilog
formats to Console; the OTel ILoggerProvider exports to Azure Monitor via
`Azure.Monitor.OpenTelemetry.AspNetCore`. W3C Trace Context propagation via
custom middleware enables end-to-end correlation across gateway and provider
boundaries. LLM-specific telemetry is split across two nested Activity spans:
the outer orchestration span (`ai.chat.complete`) carries provider and model
identity; the inner transport span (`claude.chat.api`) carries token counts,
latency, and endpoint — tagged where the data naturally lives to avoid
coupling layers. Resilience is provided by `Microsoft.Extensions.Http.Resilience`
v10, configured with no effective retries on chat POST (non-idempotent, paid
call), 45s attempt timeout, 60s total timeout, and a circuit breaker at 20%
failure ratio over a 120s sampling window. The production error contract
surfaces only a correlation identifier and a safe message; full exception
detail is logged server-side with classification tags. ADR-006 established
OTel-first as the export architecture; ADR-008 refined it to position Serilog
explicitly as a logging library inside that pipeline, not a parallel sink."

If you cannot deliver both versions cleanly, schedule a teach-back
session before moving to Day 7. Owning a concept means being fluent in
both registers.