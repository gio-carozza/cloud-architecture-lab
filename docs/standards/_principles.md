# Architect Posture — The North Star

> Read this when you're stuck, demoralized, tempted to cut corners, or
> about to ship something you know isn't quite right. This document is
> the *why* behind the technical roadmap. Skills can be learned. The
> posture below is what makes someone hireable at $250k+ and trusted at
> $500k+.
>
> Four-level career description (10yo / CEO / Engineer / Architect for all three phases):
> `docs/standards/career-path.md`

---

## The North Star in One Paragraph

You are building a provider-agnostic AI gateway and a three-site enterprise
SaaS platform (Case Management, Security/Identity, Admin) designed for
subscription use by any business or industry. The gateway is the AI nervous
system — every AI feature in every application routes through it. The
platform is the product — what enterprise clients pay for. The career is
the outcome — LLM Architect as the foundation, Forward-Deployed Engineer
as the market-facing expression. The compensation target is $250k–$500k+.
Getting there requires technical depth AND real user validation AND business
fluency AND visible thought leadership. This document holds you to all four.

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
   (Add it to `docs/standards/graveyard.md`. The graveyard is the portfolio nobody
   sees but every senior interviewer detects.)

4. **Could I explain today's work at all four levels?**
   + **10-year-old:** analogy-first, no jargon
   + **CEO:** business value, ROI, or risk — two sentences
   + **Engineer:** exact code, APIs, how-to
   + **Architect:** tradeoffs, enterprise implications, what the wrong choice costs

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

+ You're still discovering how the API behaves in edge cases
+ Your "why" for a decision is "because the docs said so"
+ You can explain what you built to another engineer but not to a stakeholder

**You cross into Phase 2 (Forward-Deployed) when:**

+ You can predict model behavior before running it
+ You instinctively ask "whose problem does this solve?" before starting
+ You've built something a non-engineer actually uses and values

**You cross into Phase 3 (LLM Architect) when:**

+ Decisions you make affect multiple teams, not just your own feature
+ You think "what happens when this scales 100x?" before shipping
+ You're writing governance policies, not just following them

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

See `docs/standards/graveyard.md` — all entries live there to keep this file stable.
Add new entries at STEP 12 of the daily workflow.
