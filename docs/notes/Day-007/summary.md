# Day 7 — Prompt Caching & Cost Observability

## Track
Hybrid (Build primary, Cert reinforcement secondary)

## Focus
Add Anthropic prompt caching inside the provider boundary, making input-token
cost a first-class observable metric rather than a billing-statement surprise.

## Why This Matters (enterprise context)
A gateway that bills the full input token cost of a static system prompt on every
call is leaving money on the table at a predictable, linear rate. At 3,000 tokens
and $3/M, 10,000 daily calls costs ~$90/day in input tokens alone. Anthropic's
ephemeral cache cuts that to ~$9/day on the cached portion — a 90% reduction that
requires no infrastructure change and no architectural compromise.

But cost reduction alone is table-stakes thinking. What turns this into an
architect-level deliverable is making the cache's behavior *observable*: cache hit
rate, creation volume, estimated savings — all queryable via KQL, not readable
only from an invoice. The moment you can write a KQL query that shows "cache hit
rate degraded 30% in the last hour," you've moved cost control from reactive
(monthly billing review) to proactive (on-call alert candidate).

The second architectural question Day 7 must answer is WHERE caching belongs in
the codebase. ADR-009 decided: inside the provider boundary, not in a decorator
above it. That decision keeps `IChatModelProvider`, `ChatRequest`, and
`ChatResponse` unchanged — the seam from Day 5 stays intact. The cost is accepted:
when a second cacheable provider lands, this code will need refactoring. The
forward-compatibility path is documented in ADR-009.

## Whose Problem Am I Solving?

### Collaboration Lens (Day 007)

**Primary — Cloud & Model-Vendor Support**
Posture: informed questioner — arrive with repro steps and a hypothesis, not "it doesn't work"
Today's question: is the cache_control TTL requirement for Claude 4 documented, or a discovered behavioral regression?

**10yo:** Today we found out the cloud tool we use has hidden rules that changed between versions — and we learned to test our assumptions with proof instead of trusting the manual.
**CEO:** Three undocumented vendor behaviors cost half a day of debugging; each is now a gotcha in the repo so the next engineer finds it in 30 seconds instead of three hours.
**Engineer:** claude-opus-4-6 returns HTTP 200 with zero cache tokens — no error; TTL required for Claude 4 cache_control; nested cache_creation format undocumented. All three needed repro + hypothesis before docs confirmed them.
**Architect:** Vendor API behavioral regressions across model generations are a category of failure unit tests cannot catch — the discipline is empirical verification after every model migration, not trust in backwards compatibility.

**Also in frame:**
- DevOps / SRE — cache hit/miss counters now in App Insights; billing spike is diagnosable in KQL, not next month's invoice
- Security/AppSec/CISO — system prompt content entering the cache is a data-handling surface worth noting in the P1 self-audit

Primary: the FinOps engineer who asks "what does this gateway cost in tokens per
hour, and is the cache working?" Today's work answers that question with a KQL
query instead of a spreadsheet.

Secondary: the on-call architect who wakes up to a billing spike — today's
`ai.provider.cache.hits` and `ai.provider.cache.misses` counters are the
difference between "the cache stopped working" being diagnosable in 60 seconds
vs. discovered on next month's bill.

## What I Will Build

1. **`AnthropicOptions`** — add `EnablePromptCaching` (bool, default `true`) and
   `SystemPrompt` (string) to allow operational toggling and a configurable cached
   system prompt without code changes.
2. **`ClaudeApiClient`** — construct the system prompt as a content array with
   `cache_control: {"type":"ephemeral","ttl":"1h"}` annotation when caching is enabled.
   Extract `cache_read_input_tokens` and `cache_creation_input_tokens` from the
   response `usage` block, with fallback to the new nested `cache_creation.ephemeral_*_input_tokens`
   format returned by Claude 4 models.
3. **`GatewayTelemetry`** — add `ai.provider.cache.hits` and
   `ai.provider.cache.misses` counters.
4. **Activity tags** — surface `llm.cache.read_tokens` and
   `llm.cache.creation_tokens` on the existing `claude.chat.api` span.
5. **KQL Queries 8 & 9** — cache hit rate and estimated savings queries added to
   `docs/standards/kql-cookbook.md`.
6. **Infra template** — `Infra/Day-007/appsettings-template.md` documents the two
   new App Service settings.
7. **Architecture doc** — `docs/architecture/day-007-prompt-caching-and-cost-observability.md`.

## Step-by-Step Execution

### Phase A — Code (done)
All three files have been updated per ADR-009:
- `Options/AnthropicOptions.cs`: `EnablePromptCaching` + `SystemPrompt` added.
- `Services/Claude/ClaudeApiClient.cs`: `BuildAnthropicRequest` emits content
  array with `cache_control` block; `TryExtractUsage` returns 4-tuple;
  cache tags and counters wired.
- `Telemetry/GatewayTelemetry.cs`: `CacheHits` and `CacheMisses` counters added.

### Phase B — Configure a ≥1100-token system prompt for local verification
Anthropic's minimum cacheable block size is 1024 tokens (Claude 3+ models).
A system prompt shorter than this produces no cache hits regardless of annotation.

Set locally via user-secrets:
```powershell
dotnet user-secrets set "Anthropic:SystemPrompt" "<your 1100+ token prompt>" `
  --project src/lab-observability-api
```

Or add temporarily to `appsettings.Development.json` (never commit the actual
text to `appsettings.json` if it contains operational instructions).

### Phase C — Local build and verification
```powershell
dotnet build src/lab-observability-api/lab-observability-api.csproj
dotnet run --project src/lab-observability-api
```

Send two identical requests:
```powershell
$body = '{"message":"Hello"}'
Invoke-RestMethod -Method Post -Uri https://localhost:7XXX/api/ai/chat `
  -ContentType "application/json" -Body $body
Invoke-RestMethod -Method Post -Uri https://localhost:7XXX/api/ai/chat `
  -ContentType "application/json" -Body $body
```

First request: console logs should show `cache_creation_input_tokens > 0`.
Second request: console logs should show `cache_read_input_tokens > 0`.

### Phase D — Add KQL queries to kql-cookbook.md
Add Query 8 (cache hit rate) and Query 9 (estimated savings) to
`docs/standards/kql-cookbook.md`. See architect-thinking.md for the KQL.

### Phase E — Infra template
Create `Infra/Day-007/appsettings-template.md` documenting the two new settings.

### Phase F — Deploy and verify in App Insights
Set the two new settings on the App Service, then deploy:
```powershell
az webapp config appsettings set -g rg-ai-lab-dev-eastus `
  -n app-ai-lab-api-dev-eastus-gio `
  --settings Anthropic__EnablePromptCaching=true `
             Anthropic__SystemPrompt="<1100+ token prompt>"
```
Then `/deploy`. After deploy:
- Send two requests to the live endpoint.
- Query App Insights: `dependencies | where name == "claude.chat.api"` should show
  `llm.cache.creation_tokens` on the first and `llm.cache.read_tokens` on the second.

### Phase G — Architecture doc
Write `docs/architecture/day-007-prompt-caching-and-cost-observability.md`.

## Architect Thinking
See `architect-thinking.md`.

## Artifacts

### Code
- `src/lab-observability-api/Options/AnthropicOptions.cs` — `EnablePromptCaching`, `SystemPrompt`
- `src/lab-observability-api/Services/Claude/ClaudeApiClient.cs` — caching payload + telemetry
- `src/lab-observability-api/Telemetry/GatewayTelemetry.cs` — `CacheHits`, `CacheMisses`

### Docs
- `docs/adr/ADR-009-implement-prompt-caching-inside-provider-boundary.md` (pre-written)
- `docs/architecture/day-007-prompt-caching-and-cost-observability.md`
- `docs/standards/kql-cookbook.md` — Query 8 and Query 9 (cache metrics)
- `docs/notes/Day-007/summary.md` (this file)
- `docs/notes/Day-007/completion-checklist.md`
- `docs/notes/Day-007/architect-thinking.md`

### Infra
- `Infra/Day-007/appsettings-template.md`

## Portfolio Value
"Implemented Anthropic prompt caching on a production AI gateway, reducing
input-token costs by ~90% on cached content with zero changes to the
provider-agnostic API contract. Made cache economics first-class telemetry:
hit rate, creation volume, and estimated savings are queryable via KQL,
enabling proactive cost alerting rather than monthly invoice review."

This proves:
- You know the difference between where logic COULD live and where it SHOULD live.
- You understand that cost observability is not the same as cost reduction.
- You made a concrete YAGNI decision under pressure and documented why.
- You can speak to the forward-compatibility path if an interviewer pushes on it.

## Completion Checklist
See `completion-checklist.md`.

## Certification Reinforcement

### AZ-900 — **None**
No direct mapping this day. Cost optimization patterns are AZ-305/AI-102 territory.

### AZ-104 — **Secondary**
- Configuring App Service application settings via CLI (`az webapp config appsettings set`)
- Operational toggle pattern (`Anthropic__EnablePromptCaching`) — same pattern as
  feature flags managed through App Service settings
- KQL against Log Analytics / App Insights

### AZ-305 — **Secondary**
- Cost optimization as a design principle (Well-Architected Framework: Cost Optimization pillar)
- Designing observable cost controls — metrics vs. logs distinction
- YAGNI in abstraction design: deferring `CachingChatModelProvider` decorator
  until the second provider's shape is known maps directly to AZ-305's emphasis
  on "design for change, not prediction"

### AI-102 — **Primary**
- Managing costs of Azure AI solutions (Monitor and Optimize AI Solutions domain)
- Implementing caching strategies for AI workloads
- Token usage telemetry and cost attribution
**Note:** AI-102 exam focuses on Azure AI services, but the PATTERN — annotate
cacheable content, measure hit rate, expose savings as a metric — is identical
to Azure OpenAI's prompt caching. Today's implementation IS the AI-102 cost
optimization pattern applied to the Anthropic provider.

## Architect Posture Check
Fill `posture-check.md` at END of day, BEFORE marking complete.
Four questions:
1. Whose problem did I actually solve today?
2. What would I refuse to ship if I were the only one in the room?
3. What did I try, fail at, and learn? (Add to the Graveyard.)
4. Can I explain this at both a 10-year-old level AND a doctorate level?
