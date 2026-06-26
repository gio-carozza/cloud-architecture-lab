# Daily Workflow — Step Details

> Full step-by-step prompts for each phase of the daily loop.
> Overview, rules, and command reference: `.claude/instructions/daily-workflow.md`

---

## STEP 0 — CLOSE PREVIOUS DAY  (Claude Code · Sonnet)

Before starting any new day, verify the previous day is frozen. Paste:

```text
Verify Day ___ is fully closed out:
1. Check docs/notes/Day-___/02-completion-checklist.md — all items [x]?
2. Check docs/notes/Day-___/04-posture-check.md — all five questions answered?
3. Check the current day's section in docs/notes/changelog.md — no duplicate rows, no stale text?
4. Check docs/standards/graveyard.md — new graveyard entries added for this day?
5. Check docs/notes/_index.md — Day ___ status = Complete?
6. Run git status — working tree clean?
Report any open items. Fix before proceeding.
When finished and clean, print the STEP 1 PRUNE manual reminder with this
day's number filled in.
```

→ STOP if any item is open. Complete it before moving to STEP 1.

---

## STEP 1 — PRUNE  (manual — not Claude Code)

Before opening Claude Code for the new day, update your **project knowledge**
(in claude.ai) manually:

- Move `docs/notes/Day-___/` working files (`01-summary.md`, `02-completion-checklist.md`, `04-posture-check.md`)
  **OUT** of project knowledge — the day is frozen; Claude Code reads them on demand.
- Keep only the current day's working files in hot context.
- ADR rotation: keep the 2 most recent ADRs in project knowledge; move older ones out.
- The permanent files (`CLAUDE.md`, `_principles.md`, `naming-conventions.md`,
  `azure-environment.md`) stay in project knowledge permanently.
- Do NOT add `collaboration-map.md`, `career-path.md`, `kql-cookbook.md`, or any `SKILL.md`
  file to project knowledge — these are read on demand and do not need to be in hot context.

This step is manual and takes ~2 minutes. It is not done by Claude Code.
→ Next: STEP 2 — DECIDE, in a new chat (Sonnet).

---

## STEP 2 — DECIDE  (chat · Sonnet)

Short chat to lock the workload and slug. Paste:

```text
Day ___ closes. I need to start Day ___.
North star item next in line: ___
Parking lot from Day ___: ___
Constraints or blockers: ___

Propose: (1) the Day ___ workload in one sentence, (2) a kebab-case slug
for /new-day. One response. No summary yet.
```

Get the slug confirmed. → STOP. Close chat.
→ Next: STEP 3 — SCAFFOLD, back in Claude Code.

---

## STEP 3 — SCAFFOLD  (Claude Code · Sonnet)

Open terminal → cd C:\dev\cloud-architecture-lab → open Claude Code.

First, check nothing already exists (prevents overwriting work):

```text
Does docs/notes/Day-___/ already exist? Report what's in it before I scaffold.
```

If it does NOT exist, scaffold it:

```text
/new-day ___ <kebab-slug>
```

Example: `/new-day 010 multi-turn-context`
This creates `docs/notes/Day-___/` (summary, completion-checklist,
architect-thinking, posture-check, files-changed — all empty templates),
`docs/architecture/day-___-<slug>.md`, and `Infra/Day-___/appsettings-template.md`
(stub — populate during STEP 6 or 7 once settings are known).

If it DOES exist (resuming a day), skip `/new-day` — the skeleton is already there.

After scaffolding, paste:

```text
Print the STEP 4 DRAFT prompt with this day's number filled in, prefixed with
"OPEN A NEW CHAT (Sonnet)".
```

Leave Claude Code open. → Go to STEP 4 in a new chat.

---

## STEP 4 — DRAFT  (new chat · Sonnet)

Paste:

```text
I'm starting Day ___ of my cloud-architecture-lab roadmap.
PREVIOUS DAY: Day ___ completed ___.
TODAY'S FOCUS: ___  (from STEP 2 DECIDE)
CAREER PHASE: ___ (AI Engineer | Forward-Deployed Engineer | LLM Architect)
CONSTRAINTS: ___

Produce the Day ___ 01-summary.md using the 13-section daily roadmap format
from CLAUDE.md. One response. No code.
```

Blanks: previous day = what you shipped. Focus = from STEP 2. Constraints =
anything broken/deferred. Don't know the focus? Replace CONSTRAINTS with:
"Recommend today's focus from `CLAUDE.md` north star and yesterday's parking lot."

Before copying the summary, validate the CEO Framing section:

  /pitch gate — paste this exactly:
  "Give me the 30-second version of today's CEO Framing for a CFO
   who does not care about technology. It must contain:

- A specific dollar amount or quantified risk (not 'improves efficiency')
- The business consequence of NOT doing this today
- One sentence a CFO would repeat to their board"

  If the pitch does not land — if it is generic, vague, or jargon-heavy —
  rewrite the CEO Framing section before copying the summary.
  Required format for CEO Framing from Day 010 onward:
  "A tenant running [N] cases/month at current token prices pays $X
   without this feature and $Y with it. That delta is [consequence]."
  Generic CEO Framing ("this improves cost efficiency") is a failed gate.
  Claude will push back on it at every posture check.
  Active from: Day 010 onward.

Copy the summary. → STOP. Close chat.
→ Next: STEP 5 (ADR) if the summary lists one, else STEP 6 (POPULATE) in Claude Code.

---

## STEP 5 — ADR REASONING  (new chat · latest Opus) — skip if summary lists no ADR

This step produces the ADR _content_ through reasoning. The file gets
created in STEP 6 with the `/adr` command.

Paste:

```text
I need an ADR for: ___.
Context: ___
Alternatives: 1) ___  2) ___  3) ___ (if any)
My lean: ___ (or "genuinely unsure")

Draft the full ADR using the template from .claude/skills/adr-writer/SKILL.md.
At least 2 alternatives with rejection reasoning.
Consequences (positive, negative, neutral).
Implementation notes naming the exact files affected.
```

Blanks come from the Architect Thinking section of your summary.

Before copying the ADR, run one final reasoning pass in the same chat:

  /devil gate — paste this exactly:
  "What is the strongest argument AGAINST this decision?
   What would a senior architect at AWS, Google, or Anthropic push back on?
   What assumption am I making that could be wrong in 18 months?
   If this ADR were wrong, what would be the first symptom and when
   would we see it?"

  If the pushback surfaces a material gap, revise the ADR before copying.
  If the pushback is addressed by existing content in the ADR, note that
  explicitly and proceed.
  A /devil gate that produces no revision is either a very strong ADR
  or a shallow challenge — be honest about which.
  Active from: Day 010 onward.

Copy the ADR content. → STOP. Close chat.
→ Next: STEP 6 — POPULATE, back in Claude Code.

---

## STEP 6 — POPULATE  (back in Claude Code)

The skeleton from STEP 3 already exists. Now fill it.

Paste the approved summary into the stub:

```text
Replace the template content in docs/notes/Day-___/01-summary.md with:
<PASTE APPROVED SUMMARY FROM STEP 4>
Then extract the completion checklist into
docs/notes/Day-___/02-completion-checklist.md.
```

Then create and fill the ADR (skip if no ADR today):

```text
/adr <kebab-case-title>
```

Example: `/adr multi-turn-context-history-contract`
This finds the next ADR number and creates the file from the template. Then:

```text
Replace the template content in docs/adr/ADR-___-<title>.md with:
<PASTE APPROVED ADR FROM STEP 5>
Set Status: Accepted, Date: today.
When finished, print the STEP 7 BUILD prompt with the day number and the
first phase's "Changes needed" bullets (from the summary) and "DO NOT modify"
list (from the ADR) filled in.
```

Then run `/collab-lens ___` to enrich the "Whose Problem Am I Solving?" section of `01-summary.md`.

---

## STEP 7 — BUILD  (Claude Code, one prompt per phase)

For each phase in the summary's Step-by-Step Execution, paste:

```text
Implement Phase ___ of Day ___.
Read first: docs/notes/Day-___/01-summary.md (Phase ___ section),
the ADR if referenced, and relevant SKILL.md files.
Then read the files you'll modify before editing.
Changes needed:
- ___
DO NOT modify: ___
After changes: build. Report errors and diff summary. Don't run or deploy.
If anything is ambiguous, stop and ask before guessing.
When the build passes clean, print the STEP 8 TEST prompt with the day number
filled in.
```

Blanks = the bullets under that phase + the ADR's "Files NOT affected."

Build fails? Paste: `Build failed with: ___ . Fix it. Rebuild. Report.`

### Test coverage requirement (every STEP 7)

After each phase that adds a new endpoint, changes input validation, or
introduces a new error path, add integration tests to
`src/lab-observability-api.Tests` before moving to STEP 8. Minimum per
new feature:

| Test type | What to verify |
|---|---|
| Input validation | null/empty/too-long prompt → 400 with correct error code |
| Error contract | provider exception → correct HTTP status, no stack trace, `correlationId` present |
| Happy path | valid request → 200 with expected response shape |
| Architecture seam | middleware headers present (correlation ID, SSE proxy headers) |

**CEO lens**: every untested input-validation guard is a budget control that can silently regress.
**Architect lens**: the error contract (no stack traces, typed error codes) is the public interface — it needs a test.
**AI Engineer lens**: provider swap-in tests confirm the abstraction seam holds.

Run `dotnet test` before leaving STEP 7. All tests must pass before STEP 8.

→ STOP when build passes clean.

---

## STEP 8 — TEST + AUDIT  (Claude Code)

Paste:

```text
Local verification for Day ___.
Run the app, execute the tests in docs/notes/Day-___/02-completion-checklist.md.
Report pass/fail for each. Fix failures and re-test.
```

→ Fix all failures before continuing.

Then, once local tests pass, run the pre-deploy pillars audit:

```text
Pillars audit for Day ___.
Read: docs/notes/Day-___/01-summary.md, the current "## Day ___" section in
docs/notes/changelog.md, src/lab-observability-api/Program.cs, and every source
file listed in that section that was added or modified this day.
Run all 6-pillar checks from .claude/skills/pillars-audit/SKILL.md.
Append the full per-check output (all checks, RAG per pillar, RED/YELLOW fix
sections) to docs/notes/Day-___/05-audit-log.md under "## Run: STEP 8 pre-deploy (YYYY-MM-DD)".
Fix any RED items, update the RED items → fixes section, re-audit.
Upsert YELLOW items as debt rows under the current day's section in docs/notes/changelog.md (step: audit).
When the audit is complete with no open RED items, print the STEP 9 DEPLOY prompt
with the day number filled in.
```

Then run the reference integrity and naming check:

```text
/sync-check
```

This runs `symbol-drift-check.js` (C# symbols, telemetry strings, `src/` paths) and
`name-check.js` (broader type suffixes, Azure resource names, command refs, `ADR-NNN` refs),
then verifies markdown link targets, standards-index paths, ADR sequence, cert day-mapping
existence, changelog path integrity, and command index. Fix every ❌ finding with a targeted
`Edit`. Legitimate "renamed `OLD` → `NEW`" narrations and rejected-ADR alternatives are not
violations — re-run to confirm only those remain.

→ STOP when local tests pass AND audit returns no RED items AND `/sync-check` has no unaddressed findings.

---

## STEP 9 — DEPLOY  (Claude Code)

Settings are applied automatically: `/deploy` reads `Infra/Day-___/appsettings-template.md`
and applies any settings there before publishing. You don't add them manually.
Just confirm the template is populated (it's written during STEP 7 build) and run:

```text
/deploy
```

This runs the Kudu zip path from `.claude/skills/azure-deploy/SKILL.md` and
verifies `/health`, `/swagger`, and `OpenTelemetry` post-deploy.

Then telemetry verification (if today added telemetry):

```text
Wait 3 minutes, then run the KQL check from the completion checklist.
Report results.
When Azure tests pass, print the STEP 10 DOCUMENT prompt with the day number
filled in.
```

→ STOP when Azure tests pass.

---

## STEP 10 — DOCUMENT  (Claude Code)

Paste:

```text
Day ___ documentation pass.
1. Update docs/standards/kql-cookbook.md with new queries
2. Update docs/standards/azure-environment.md if resources/settings changed
3. Finalize docs/notes/Day-___/02-completion-checklist.md (mark [x])
4. Write docs/notes/Day-___/03-architect-thinking.md with key insights
   — include a "CEO Framing" subsection: one sentence on the business value of today's change
   — include a "Phase Note" subsection: which career phase did this day reinforce and why
5. Update CLAUDE.md (north star done items, new conventions)
6. Update docs/notes/changelog.md — upsert one row per file touched this pass
   under the current day's section; dedup on file path; label Step column "docs pass"
7. git add -A && git commit -m "feat(day-___): ___"
Report what changed.
```

Then update cert study materials for domains touched today:

```text
/cert-update ___
```

After `/cert-update`, paste:

```text
Print the STEP 11 REFLECT prompt with the day number filled in, prefixed with
"OPEN A NEW CHAT (latest Opus)".
```

---

## STEP 11 — REFLECT  (new chat · latest Opus)

Paste:

```text
Posture check for Day ___.
What I built: ___
What broke: ___
Ask me the five posture questions from _principles.md and then provide the answers to those questions in a way that a 10 year old, a CEO, an Engineer, and an LLM Architect could understand:
1. Whose problem did I actually solve today?
2. What would I refuse to ship?
3. What did I try, fail at, and learn?
4. Can I explain this at 10yo, CEO, Engineer, AND Architect level?
5. Which pillar took the most damage today, and what's the minimum fix?
If the day's collaboration lens named a primary collaborator, confirm posture Q1 names that same specific role.
Push back hard if my answers are weak or self-congratulatory.
```

Phase 3 onward (Day 051+), add a sixth posture question:

  /10x gate — paste this exactly:
  "This platform currently targets [N] tenants. What are the first
   three architectural decisions that break or become expensive at
   1,000 tenants? For each:

- Name the specific component or seam that fails
- Estimate the order-of-magnitude cost or latency impact
- State whether it requires a redesign or a configuration change
   This is not hypothetical — name real components from the codebase."

  A /10x gate answer that says "everything scales fine" is a red flag,
  not a green light. Every architecture has a ceiling. Name it honestly.
  Document findings in architect-thinking.md under a "Scale Ceiling"
  subsection. This becomes the Phase 3 refactoring backlog.

  Activation:

- Phase 1 and 2 (Days 001–050): OPTIONAL — run only when the day
    introduces a new seam, a new data store, or a new tenant-facing surface
- Phase 3 and Platform Build (Days 051–200): MANDATORY every day
  Active from: Day 010 (optional) / Day 051 (mandatory).

Copy the result. → STOP. Close chat.
→ Next: STEP 12 — CLOSE, back in Claude Code.

---

## STEP 12 — CLOSE  (Claude Code)

Paste:

```text
Update docs/notes/Day-___/04-posture-check.md:
<PASTE POSTURE CHECK>
Add graveyard entries to docs/standards/graveyard.md:
<PASTE ENTRIES>
Update docs/notes/_index.md — set Day ___ status = Complete.
git add -A && git commit -m "docs(day-___): posture check and graveyard"
When committed, print: "Running repo-audit…"
```

Then run the full repo health check:

```text
/repo-audit ___
```

This checks day folder completeness, ADR structure, markdownlint, backtick conventions,
`CLAUDE.md` accuracy, `changelog.md` coverage, cert domain coverage, and provider
abstraction integrity. It auto-fixes what it can (markdownlint, backtick wrap) and reports
everything else. Fix any ❌ items before closing. If only ⚠️ items remain, confirm with
the user before proceeding.

If `repo-audit` made changes, commit them:

```text
git add -A && git commit -m "chore(day-___): repo-audit fixes"
```

Optionally, run the cross-doc naming audit (not required every day — run after significant
documentation work, class renames, or at phase transitions):

```text
/name-audit
```

This uses AI reasoning to catch naming inconsistencies that scripts cannot detect: variant
spellings, missing `I`-prefixes on interfaces, cross-file disagreements on the same entity's
name, and stale Azure resource names. If it makes edits, commit them:

```text
git add -A && git commit -m "chore(day-___): name-audit fixes"
```

Then run a final pillars audit pass to catch anything introduced since STEP 8:

```text
Close-of-day audit for Day ___.
Read the current "## Day ___" section in docs/notes/changelog.md and identify
any files edited AFTER the "audit" step row was written (docs pass edits,
posture-gap fixes, graveyard additions that touched source files).
Re-run only the pillar checks relevant to those files. If no source files
changed after STEP 8, run only O4 (changelog.md coverage complete for this day?) and
O5 (KQL cookbook updated for any new signals?).
Append the output (re-checked pillars only) to docs/notes/Day-___/05-audit-log.md
under "## Run: STEP 12 close-audit (YYYY-MM-DD)".
If RED: fix and commit before closing; update RED items → fixes section in 05-audit-log.md.
If YELLOW: upsert a debt row under the current day's section in docs/notes/changelog.md (step: close-audit) and record
disposition in YELLOW items → fixes section in 05-audit-log.md.
If all GREEN or N/A: append "close-audit: GREEN" to 05-audit-log.md and proceed.
```

Done. Tomorrow → STEP 0.

---

## ONE-TIME SETUP (not part of the daily loop)

Run once, early — ideally end of Day 7 — to build the cert study structure:

```text
/cert-scaffold AZ-900
/cert-scaffold AZ-104
/cert-scaffold AZ-305
# AI-103 scaffold runs at Phase 2 start (~Day 21) — beta exam, domains not finalized
# /cert-scaffold AI-103
```

After this, `/cert-update N` in STEP 10 populates domains as you touch them.
