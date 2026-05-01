# Architect Posture — The North Star

> Read this when you're stuck, demoralized, tempted to cut corners, or
> about to ship something you know isn't quite right. This document is
> the *why* behind the technical roadmap. Skills can be learned. The
> posture below is what makes someone hireable at $250k+ and trusted at
> $500k+.

## What an LLM Architect Actually Does

An LLM architect doesn't just train models. They design how AI fits into
a real business. When a hospital wants AI to help doctors, or a bank
wants one to help customers, the architect plans the whole system:

- Which model to use
- How to keep it safe
- How to plug it in
- How to keep it from making things up

The job is not "knowing about transformers." The job is using that
knowledge to build something real that helps real people.

## The Technical Foundations Translated to Job Skills

| Foundation | Why an architect uses it |
|---|---|
| Transformers & attention | When the model misbehaves, you can diagnose *why* — not just swap parts randomly |
| Hallucinations | You build safety nets: real databases, fact-checkers, "I don't know" responses |
| Context window limits | You design RAG (Retrieval-Augmented Generation) and other patterns to feed the model the right info at the right time |
| Training cost reality | You fine-tune existing models instead of training from scratch — cheaper, faster, equally effective |
| AI safety | You build guardrails: tested for harm, leakage, bias. At enterprise scale, one mistake costs millions or hurts people |

## The Five Traits of Legendary Architects

Anyone can learn the tech. Books, courses, YouTube — it's all out there.
What separates the best from the rest are five things:

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

### 4. Explain things simply
If you can explain transformers to an 8-year-old, you understand them.
If you can only explain them in fancy words, you're hiding behind jargon.

Talk to engineers. Talk to the CEO. Talk to grandma. **Everyone walks
away understanding.**

### 5. Build, build, build
Reading about AI is like reading about swimming. You don't get good
until you jump in.

The legends didn't wait for permission. They built little projects in
their bedrooms, broke things, fixed them, shared them.

**Every great architect has a graveyard of failed experiments. That
graveyard is where the wisdom comes from.**

## The Secret Nobody Tells You

The "best LLM architect" isn't the one who knows the most math. It's the
one who combines three things most people never combine:

1. Deep technical skill
2. Genuine care for people
3. Courage to do things right

If you grow into someone who loves computers AND loves people AND is
brave enough to tell the truth — you won't just be a great architect.
You'll be the kind of person who shapes how AI changes the world for
the better.

## Daily Posture Check

At the end of each roadmap day, answer these three questions in
`Day-NNN/posture-check.md`:

1. **Whose problem did I actually solve today?**
   (If the answer is "nobody specific" — interrogate that.)

2. **What would I refuse to ship if I were the only one in the room?**
   (If nothing — am I being honest, or am I deferring?)

3. **What did I try, fail at, and learn?**
   (Add it to the graveyard. The graveyard is the portfolio nobody sees
   but every senior interviewer detects.)

## Token Discipline as Architecture Practice

The cheapest model that solves the problem is the right model. This is true for
my learning roadmap and it will be true for every production AI gateway I design.

Default to Sonnet 4.6. Escalate to Opus 4.7 only when:
- The reasoning IS the deliverable (ADRs, tradeoff analysis)
- Sonnet's answer is thin and the depth matters
- I'm doing adversarial reasoning against my own conclusions

Default to Claude Code for any work where repo files matter. Default to chat for
work where conventions and reasoning matter more than code.

This is not frugality theater. This is rehearsing the FinOps discipline that
separates senior architects from juniors. Token cost is a first-class architectural
constraint in any real AI workload. Internalize it now on my own dime; apply it
later on the company's dime.

## Working in Three Modes

A senior architect doesn't have one mode. Three modes rotate, each with
its own discipline, its own tool, its own output style. Mixing them
produces sloppy work in all three.

### Build mode — make it run
Posture: pragmatic, fast, copy-paste-ready.
Discipline: don't over-architect. Don't draft an ADR mid-deploy.
The bias is *progress*. Reasoning happens before and after, not during.
Tool: Claude Code in terminal. Chat only when blocked.

### Study mode — own the concept
Posture: curious, patient, repeatable.
Discipline: explain at two levels (10-year-old AND doctorate). If only
one register works, the concept isn't owned yet — schedule a teach-back.
Always map to a cert domain (AZ-900 / AZ-104 / AZ-305 / AI-102) so
study time produces dual leverage.
Tool: chat. Cert prep notes paste-in per session.

### Perform mode — reason at architect level
Posture: structured, honest, slow.
Discipline: surface tradeoffs explicitly. Name the alternative being
rejected. Write the ADR before the code, not after — writing exposes
weak reasoning. Posture-check at end of day BEFORE marking complete.
Tool: chat. Opus 4.7 escalation most often warranted here.

### The integration test
At the end of each roadmap day, ask: did I move between modes
deliberately, or did I drift? If I built without reasoning, I shipped
under-thought work. If I reasoned without building, I shipped vapor.
The discipline is rotating consciously, not staying in whichever mode
feels easiest today.

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