# Architect Posture — The North Star

> Read this when you're stuck, demoralized, tempted to cut corners, or
> about to ship something you know isn't quite right. This document is
> the *why* behind the technical roadmap. Skills can be learned. The
> posture below is what makes someone hireable at $250k+ and trusted at
> $500k+.
>
> Full career progression detail: `docs/standards/career-path.md`

---

## Your Three-Phase Career

### Phase 1: AI Engineer

#### If you're 10 years old
An AI Engineer is like a chef who learns to use a really powerful new kitchen tool —
a robot that can write recipes, answer questions, and help cook meals. Your job is to
plug that robot into the restaurant kitchen and make sure the food it helps create is
actually good. You learn what the robot can do, where it makes mistakes, and how to give
it the right instructions so it doesn't burn the soup.

#### If you're a CEO
An AI Engineer is the person who makes AI work inside your product. They call the right
model, write prompts that produce reliable output, and wire everything into your existing
application without breaking it. ROI of a strong AI Engineer: faster time-to-ship on
AI features, lower hallucination rates, and a team that doesn't need a consultant for
every integration. Risk of a weak one: AI features that work in demos and fail in production.

#### If you're an Engineer
An AI Engineer integrates LLM APIs (Anthropic, OpenAI, Azure AI) into production systems.
Core skills: prompt engineering (system prompts, few-shot examples, output structuring),
API integration (streaming, tool use, function calling, batch APIs), context window
management, token cost optimization, and debugging model behavior — distinguishing a
hallucination from an API error, a prompt problem from a model limitation. You measure
quality not just as "does it run" but "does it produce correct output at acceptable cost."

#### If you're an Architect
The AI Engineer phase builds the intuition that architecture later depends on. You cannot
design an AI platform you haven't debugged at the API level. The critical milestone is a
working provider abstraction seam — the boundary that lets you swap Claude for Azure
OpenAI without changing application logic. Every subsequent phase assumes you've already
crossed this threshold and understands why it matters.

---

### Phase 2: Forward-Deployed Engineer

#### If you're 10 years old
A Forward-Deployed Engineer is like a helper who travels to different businesses and
builds them their own special AI assistant. One week you're at a hospital helping doctors,
the next week you're at a bank helping customers. You have to be really good at listening
— because every place has different problems and you can't build the same thing everywhere.
You also explain what you built to the boss, the workers, and the customers — all in
different ways that actually make sense to each of them.

#### If you're a CEO
A Forward-Deployed Engineer is your most valuable AI hire for enterprise sales. They sit
with your customer, understand the real business problem (not the stated technical
requirement), build a working prototype in days — not months — and iterate until the
customer says "this is exactly what we needed." They're the people Palantir, Anthropic,
and Scale AI send to close 7-figure deals. The skill is rare: most engineers can build;
few can sell through building. ROI: shorter sales cycles, higher deal values, faster
customer time-to-value.

#### If you're an Engineer
Forward-Deployed Engineering means scoping and shipping AI solutions in days, not sprints.
Key skills: rapid prototyping (working demo in 48 hours), solution scoping (minimum AI
that solves this specific problem), domain-specific prompt engineering (legal, medical,
financial), RAG integration (connecting AI to the customer's existing data), and
evaluation (proving the solution is actually correct before handing it over). You learn
to ask "what decision does this AI help the user make?" before writing any code.

#### If you're an Architect
The Forward-Deployed phase teaches the skill the AI Engineer phase doesn't: connecting
technical decisions to business outcomes. Every architectural choice becomes a story —
"we chose provider abstraction because if Anthropic raises prices, you can switch to
Azure OpenAI without rewriting your application." You learn to communicate at CEO, CFO,
and CTO levels simultaneously, in the same meeting. Architecturally, this phase focuses
on RAG patterns, agent frameworks, and evaluation — the three capabilities that turn
a chat endpoint into a product that solves real problems.

---

### Phase 3: LLM Architect

#### If you're 10 years old
An LLM Architect is like the person who designs the whole AI factory, not just one
machine. Instead of building one robot for one restaurant, you design the system that
lets every restaurant in the country have their own — safe, affordable, and upgradeable.
You think about what happens when a million people use the system at once, and you make
sure nobody accidentally breaks it, spends too much money, or does something they
shouldn't.

#### If you're a CEO
The LLM Architect is the person who turns AI experiments into enterprise infrastructure.
They design the platform that lets your entire company — every team, every product —
use AI safely and economically. They prevent the $2M surprise cloud bill, ensure GDPR
and HIPAA compliance, and keep you from being locked into one vendor's pricing. The wrong
design at this layer costs tens of millions. The right one makes AI a competitive moat.
This is the $200k–$250k role.

#### If you're an Engineer
LLM Architecture means systems serving thousands of AI requests per minute across multiple
providers, with full observability, cost governance, and resilience. Core skills:
multi-provider abstraction (route between Anthropic, Azure OpenAI, Bedrock based on
cost/latency/capability), distributed tracing across the full AI call chain, semantic
caching, vector database design for RAG at scale, model evaluation pipelines, and
enterprise security (tenant isolation, audit logging, PII redaction). Stack: API gateway
+ provider SDK abstraction + vector DB + observability platform + IaC.

#### If you're an Architect
The LLM Architect phase is where AI engineering meets enterprise software architecture.
The problems shift from "does this work?" to "does this work for 10,000 users at 3am
when Anthropic has a partial outage and the compliance team needs an audit trail?"
Key design decisions: provider abstraction contract, cost attribution by team and workload
class, governance policy (who deploys new models and with what approval workflow), and
resilience (circuit breakers, retry budgets, fallback providers). The architect owns
policy, not just code — model selection criteria, cost ceilings, incident response, and
the vendor negotiation position that multi-provider portability creates.

---

## The Five Traits of Legendary Architects
*(These apply across all three phases — they're not optional in Phase 3)*

### 1. Never stop being curious
This field changes every month. New models, new tricks, new problems.
The best architects are like 8-year-olds — they ask "but why?" and
"but how?" all day. **The day you think you know enough is the day you
start falling behind.**

### 2. Understand people, not just AI
The hardest part isn't the tech. It's figuring out what people actually
need. Sit with doctors. Listen to teachers. Watch customers struggle.
*Then* design the AI.

Bad architects build cool tech nobody asked for. **Great ones build
things that change lives.**

### 3. Care about doing it right, even when it's hard
Sometimes the fast way is unsafe. Sometimes the cheap way leaves people
behind. The best architects have a backbone — they tell their CEO
"no, we shouldn't ship this yet."

That courage is rare. **It's what separates good from legendary.**

### 4. Explain things simply — at all four levels
If you can explain it to a 10-year-old, you understand it.
If you can explain the ROI to a CEO, you can sell it.
If you can explain the implementation to an engineer, you can delegate it.
If you can explain the tradeoffs to an architect, you can defend it.

**Anyone who can only explain in one register hasn't fully owned the concept.**

### 5. Build, build, build
Reading about AI is like reading about swimming. You don't get good
until you jump in.

**Every great architect has a graveyard of failed experiments. That
graveyard is where the wisdom comes from.**

---

## Daily Posture Check

At the end of each roadmap day, answer these five questions in
`Day-NNN/04-posture-check.md`. Answer Q4 at all four levels.

1. **Whose problem did I actually solve today?**
   (Name a specific human role. "The platform" is not an answer.)

2. **What would I refuse to ship if I were the only one in the room?**
   (Name the corner you were tempted to cut. Did you cut it?)

3. **What did I try, fail at, and learn?**
   (Add it to the Graveyard below. The graveyard is the portfolio nobody
   sees but every senior interviewer detects.)

4. **Could I explain today's work at all four levels?**
   - **10-year-old:** analogy-first, no jargon
   - **CEO:** business value, ROI, or risk — two sentences
   - **Engineer:** exact code, APIs, how-to
   - **Architect:** tradeoffs, enterprise implications, what the wrong choice costs

   If any level is missing, the concept isn't fully owned — schedule a teach-back.

5. **Which pillar took the most damage today, and what's the minimum fix?**
   (Name the weakest WAF pillar or Responsible AI gap from today's changes.
   State whether it was fixed before deploy — GREEN — accepted as documented
   debt — YELLOW — or left open — RED. A day with all GREENs is either a
   great day or a shallow audit. Be honest about which.
   Full audit checklist: `.claude/skills/pillars-audit/SKILL.md`)

---

## Phase Awareness

**You're in Phase 1 (AI Engineer) while:**
- You're still discovering how the API behaves in edge cases
- Your "why" for a decision is "because the docs said so"
- You can explain what you built to another engineer but not to a stakeholder

**You cross into Phase 2 (Forward-Deployed) when:**
- You can predict model behavior before running it
- You instinctively ask "whose problem does this solve?" before starting
- You've built something a non-engineer actually uses and values

**You cross into Phase 3 (LLM Architect) when:**
- Decisions you make affect multiple teams, not just your own feature
- You think "what happens when this scales 100x?" before shipping
- You're writing governance policies, not just following them

---

## Token Discipline as Architecture Practice

The cheapest model that solves the problem is the right model. Default to Sonnet.
Escalate to the latest Opus only when reasoning depth IS the deliverable.

This is not frugality theater. It is rehearsing the FinOps discipline that separates
senior architects from juniors. Token cost is a first-class architectural constraint
in any real AI workload. Internalize it now on your own dime; apply it on the
company's dime.

---

## Working in Three Modes

### Build mode — make it run
Posture: pragmatic, fast, copy-paste-ready.
Discipline: don't over-architect. Don't draft an ADR mid-deploy.
Tool: Claude Code in terminal.

### Study mode — own the concept
Posture: curious, patient, repeatable.
Discipline: explain at all four levels. Map to a cert domain.
Tool: chat.

### Perform mode — reason at architect level
Posture: structured, honest, slow.
Discipline: surface tradeoffs. Write the ADR before the code.
Posture-check before marking complete.
Tool: chat, latest Opus.

### The integration test
Did I move between modes deliberately, or drift? Build without reasoning = shipped
under-thought work. Reason without building = shipped vapor. Rotate consciously.

---

## The Graveyard

A running list of experiments, dead-ends, and "this didn't work but I
learned X" moments. Keep this honest. The graveyard is the most credible
thing on your resume — every senior architect has one, and the ones who
pretend they don't are the ones you can't trust.

| Date | What I tried | What broke | What I learned |
|---|---|---|---|
| Day 5 | `az webapp deploy` | TLS reset mid-stream | Kudu publish API is the primitive; wrappers hide failures |
| Day 5 | Placeholder namespace `YourNamespace` | Build broken | Always replace template namespaces with the actual project namespace before first build |
| Day 5 | Forgot `using Microsoft.Extensions.Options;` | IOptions<T> compile error | Some namespaces aren't auto-included; the IOptions pattern requires explicit import |
| Day 5 | Anthropic returned 400 with vague error | Confused about wire format | Account credits issue; check billing before debugging serialization |
| Day 6 | Following Day 6 plan literally with appi-ai-lab-dev-eastus | Azure global namespace collision | Naming conventions need an ownership suffix for globally-unique resources from day one — retrofit cost is real but cheaper than recurring collisions |
| Day 6 | Migrated classic AI to workspace-based on a live deployed app | Nothing — but only because there was no historical telemetry, alerts, or dashboards yet | Migrations on live resources are cheap when you do them early. Every day you wait, the cost compounds. Architect-level practice: do the disruptive thing while it's still cheap. |
| Day 6 | OpenTelemetry.Extensions.Hosting 1.10.0 from memory | Azure.Monitor.OpenTelemetry.AspNetCore 1.4.0 already requires >= 1.14.0 transitively | Always check the existing dependency graph before pinning a version. dotnet list package --include-transitive is the source of truth, not memory. |
| Day 6 | Ran heavy roadmap project on Opus 4.7 with everything in project knowledge | Hit usage limits within days | Model selection is a budget decision, not a quality decision; project knowledge is hot context and must be pruned aggressively. This pattern applies directly to ClaudeChatModelProvider — future enhancement: add a model-tier selector so callers can route between Sonnet and Opus by task class. |
| Day 6 | Built day-folder docs without lifecycle discipline | Project knowledge ballooned past sustainable token budget; duplicate ADR numbers; stale standards in day folders | Folder location should reflect lifecycle, not authoring date. Standards live in standards/. Logs live in notes/Day-NNN/. Architecture descriptions live in architecture/. ADRs are point-in-time decisions, never edited once Accepted. The minute a Day-NNN file is referenced from a later day, it has graduated to a standard and should be promoted. Caught and fixed on Day 6 — cost: 30 minutes. Cost on Day 60 would be a half-day. Cost on Day 200 is a portfolio repo that quietly screams junior. |
| Day 6 | Configured resilience with SamplingDuration = 30s, AttemptTimeout = 45s | App startup failed via ValidateOnStart — sampling must be >= 2x attempt timeout per AddStandardResilienceHandler invariant | Build-time validation is necessary but not sufficient. Startup validators catch a class of semantic errors no compiler sees. Always wire ValidateOnStart for options bound to libraries with mathematical or semantic invariants. |
| Day 6 | Expressed "no retries on chat POST" as MaxRetryAttempts = 0 per ADR-006 intent | Microsoft.Extensions.Http.Resilience v10 rejects 0 as invalid | Replaced with MaxRetryAttempts = 1 + Retry.ShouldHandle = _ => false. Architectural intent (no retries) is unchanged; only the encoding shifted to satisfy the validator. ADRs document intent; code expresses it within current library constraints. Both must stay in sync, but they evolve at different rates. |
| Tooling (2026-04-30) | Installed Claude Code via `npm install -g`; commands appeared to do nothing | PowerShell `Restricted` execution policy silently blocked `npm.ps1` from running | Diagnosed by running `npm --version` directly, which surfaced the underlying `UnauthorizedAccess` error. Fixed with `Set-ExecutionPolicy RemoteSigned -Scope CurrentUser`. Lesson: when a CLI tool seems to do nothing, run its own version command directly to surface the real error. Wrappers hide failures; primitives reveal them. (Same lesson as `az webapp deploy` vs Kudu API on Day 5.) |
| Day 7 | Used `claude-opus-4-6` as model ID in user-secrets | API returned HTTP 200 with valid content on every request but `cache_creation_input_tokens: 0` always — prompt caching silently inoperative | `claude-opus-4-6` is not a recognized current Anthropic model ID. Valid IDs: `claude-opus-4-8`, `claude-sonnet-4-6`, `claude-haiku-4-5-20251001`. A wrong model ID can fail silently in dimensions unrelated to HTTP status codes. When caching produces zero tokens despite a correct request format, check the model ID before debugging code. |
| Day 7 | Sent `cache_control: {"type":"ephemeral"}` without a TTL to Claude 4 models | Caching silently inactive — API accepts the request, returns `cache_creation_input_tokens: 0`, bills at full rate, no error, no warning | Claude 4 models require an explicit TTL: `{"type":"ephemeral","ttl":"1h"}` or `"ttl":"5m"`. The bare format worked for Claude 3. This is a silent behavioral regression when migrating model generations. Always verify `cache_creation_input_tokens > 0` on the first request after any model or API format change. |
| Day 7 | `/health` returned 200 while a pre-Day-6 build was live | Liveness proved a process was up, not which build was running | A health probe that cannot name the running artifact is half a probe. Emit assembly version + git SHA at startup and on `/health/info`; assert the expected SHA in the Phase 0 gate. |
| Day 8 | Authored the batch contract across two isolated chats (STEP 4 summary vs STEP 5 ADR) | Summary drafted IBatchJobProvider / SubmitAsync / BatchJobRequest; ADR reasoned to IBatchChatModelProvider / SubmitBatchAsync / BatchJob. Contracts entered STEP 7 inconsistent; resolved only by asking the mentor | Fixing the specific names fixes Day 8; it does nothing for Day 9. The cause is a missing reconciliation gate. The workflow needs a STEP 5.5 that diffs the summary's contract against the ADR's contract before BUILD. Any non-trivial contract authored across two single-shot chats with no cross-reference WILL reproduce this. The lesson is the gate, not the names. |
| Day 8 | Marked ADR-010 Accepted with ⟨confirm: offline batch evaluation / bulk model scoring⟩ still live in the Context section | An Accepted ADR contained an unresolved placeholder; caught and cleaned in STEP 6, but it should never have reached Accepted in that state | Accepted status is the signature on the contract. A placeholder in a signed contract makes the signature decorative. The fix is not "removed the token" — it's an Accepted-status gate: no ADR flips to Accepted with any ⟨...⟩ placeholder present. Status transition is the thing to guard, not the specific token. |
| Day 8 | Designed the batch endpoint (Phase A–E) with no upper bound on request count | The original design would have deployed a batch ingress where a single call could submit 10,000 requests and generate a large unintended bill. MaxBatchSize was caught only after STEP 10 was committed | THE HEADLINE FAILURE of the day, measured against my own north star (token cost as a first-class constraint; governance as north-star item 7; a prior graveyard entry already exists for blowing usage limits). MaxBatchSize is a contract-level invariant — same tier as authn and input validation — and belongs in Phase A. "All tests passed" indicted the suite: a submit-(N+1)-expect-rejection test never existed because the guardrail it would test never existed. Blast radius ("what does the worst single call cost?") is now a mandatory Phase A design question for any ingress that fans out to paid calls. |
| Day 8 | Ran Phase D rebuild without stopping the app left running from STEP 8 testing | MSB3021 — Unable to copy file, in use by another process; build flow interrupted | Operational miss, low severity. Stop the running host before any rebuild. Minor, but it cost flow at exactly the wrong moment — the close. |
| Day 9 | Initially reported p50 TTFT = 1354ms alone as the streaming success metric | p50 is the flattering percentile; p95/p99 — the churning tail — went unmeasured until the posture check forced it | The KQL cookbook Query 11 already computes p50/p95/p99. Having the query and reporting only the median is choosing the kind number. A latency claim without the tail is reputation management, not measurement. Pulled the tail baseline question before closing the day; count=3 was insufficient for reliable estimates, so the honest answer was "establish baseline as traffic accumulates" rather than fabricating confidence. |
| Day 9 | Hit CS1626 (no yield return in try/catch) in the streaming provider | The naive fix is to delete the try/catch; the correct fix is to restructure so yield lives in try/finally (no catch clause) and exceptions propagate to the controller | The compiler error is week-one C#; the architecture lesson is that the iterator restriction forces a deliberate error-propagation design in streaming providers. The catch lives at the controller layer, which emits a clean `event: error` SSE frame — same shape as the Day 8 "removed the thing in the way" entries. The error contract is not deleted, just relocated to the appropriate layer. |
| Day 9 | Three consecutive ADRs decided seam-vs-no-seam: caching IN (009), batch OUT (010), streaming IN (011) | Read cold, the trilogy could look like three coin flips unless 011 explicitly reconciles against 010 | The load-bearing test is Liskov substitutability, not "new operation." Batch breaks it (no sensible fallback for a non-batch provider); streaming does not (single-chunk degrade is correct, not a compromise). The default-degrade implementation on the interface is the proof — it compiles and behaves correctly. Wrote the reconciliation into ADR-011 citing ADR-010; the gate is "a non-trivial seam decision must cite the prior inverse decision," not the specific ADR numbers. |
| Day 9 | Pillars audit first returned Responsible AI GREEN on the streaming path | RA6 (final-usage logging) was GREEN without a disconnect test; usage lives in message_delta which RequestAborted can cut off before it is read | _principles.md warns an all-GREEN day is "either a great day or a shallow audit." The first pass was shallow — it assumed the disconnect path logs usage rather than testing it. Observed the gap in stream-test-output.txt: WRN fired on a live test where the client disconnected before message_delta arrived. Mitigated: finally block now distinguishes client disconnect (LogDebug) from unexpected stream end (LogWarning). Not fully closed: no automated fault-injection test yet. Left YELLOW in the audit trail. Audits that rubber-stamp the exact condition where the gap hides are worse than no audit. |
