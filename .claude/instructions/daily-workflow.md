# Daily Workflow

> Follow top to bottom every day. Fill the blanks marked ___.
> When you see STOP, close the chat before continuing.
> Save as: `.claude/instructions/daily-workflow.md`

---

## THE SHAPE OF EVERY DAY

```
STEP 0  CLOSE (prev)   Claude Code · Sonnet    verify previous day frozen (audit, posture-check, files-changed)
STEP 1  PRUNE          (manual)                move previous day's files out of hot context; rotate ADRs
STEP 2  DECIDE         chat · Sonnet           lock the workload and slug for the new day
STEP 3  SCAFFOLD       Claude Code · Sonnet    /new-day N <slug>  (build the empty skeleton)
STEP 4  DRAFT          chat (new) · Sonnet     full Day N summary in 13-section format
STEP 5  ADR            chat (new) · latest Opus  reason out the decision (skip if none)
STEP 6  POPULATE       Claude Code · Sonnet    fill summary + checklist + /adr
STEP 7  BUILD          Claude Code · Sonnet    implement each phase
STEP 8  TEST + AUDIT   Claude Code · Sonnet    local tests + 6-pillar gate before deploy
STEP 9  DEPLOY         Claude Code · Sonnet    /deploy + verify
STEP 10 DOCUMENT       Claude Code · Sonnet    update docs + commit + /cert-update
STEP 11 REFLECT        chat (new) · latest Opus  posture check
STEP 12 CLOSE          Claude Code · Sonnet    save posture check + commit
```

Four rules:
1. Chat for STEP 2, 4, 5, 11 only. STEP 1 is manual. Claude Code for everything else.
2. Model: Sonnet everywhere EXCEPT STEP 5 and STEP 11, which use the LATEST
   Opus (currently Claude Opus 4.8 — confirm the newest Opus in the model
   picker; don't pin a version number).
3. New chat for each of STEP 2, 4, 5, 11. Close it the moment that step ends.
4. STEP 0 is not optional. Never start a new day without verifying the previous
   day is fully frozen.

Why scaffold first: the day folder and stub files must exist before anything
can be written into them. Build the empty skeleton, then draft into it, then
fill it. No chicken-and-egg.

Your slash commands (in .claude/commands/):
/new-day · /adr · /deploy · /cert-scaffold · /cert-update

---

## THE AUTO-PRINT HANDOFF (how each step hands you the next)

Every Claude Code step ends by PRINTING the next step's prompt, pre-filled
with the day number and known paths — so you never hunt for what's next.
To activate it, the last line of each Claude Code prompt below already says:
"When finished, print the next step's prompt..." Leave that line in.

- Claude Code steps (0, 3, 6, 7, 8, 9, 10, 12) auto-print the next prompt.
- Chat steps (2, 4, 5, 11) can't be auto-fed (different surface) — the prior
  step prints "OPEN A NEW CHAT" plus the prompt to paste there. You open the
  chat yourself.
- STEP 1 (PRUNE) is manual in claude.ai — the prior step prints a reminder.
- Content blanks (___) stay yours: focus, the phase's changes, what broke.
  No file can pre-know them; they come from your summary or your memory of
  the day.

---

## STEP 0 — CLOSE PREVIOUS DAY  (Claude Code · Sonnet)

Before starting any new day, verify the previous day is frozen. Paste:
```
Verify Day ___ is fully closed out:
1. Check docs/notes/Day-___/completion-checklist.md — all items [x]?
2. Check docs/notes/Day-___/posture-check.md — all five questions answered?
3. Check docs/notes/Day-___/files-changed.md — no duplicate rows, no stale text?
4. Check docs/standards/_principles.md — graveyard entries added?
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

- Move `docs/notes/Day-___/` working files (summary, checklist, posture-check)
  **OUT** of project knowledge — the day is frozen; Claude Code reads them on demand.
- Keep only the current day's working files in hot context.
- ADR rotation: keep the 2 most recent ADRs in project knowledge; move older ones out.
- The standards files (CLAUDE.md, _principles.md, naming-conventions.md,
  azure-environment.md, kql-cookbook.md) stay in project knowledge permanently.

This step is manual and takes ~2 minutes. It is not done by Claude Code.
→ Next: STEP 2 — DECIDE, in a new chat (Sonnet).

---

## STEP 2 — DECIDE  (chat · Sonnet)

Short chat to lock the workload and slug. Paste:
```
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
```
Does docs/notes/Day-___/ already exist? Report what's in it before I scaffold.
```

If it does NOT exist, scaffold it:
```
/new-day ___ <kebab-slug>
```
Example: /new-day 8 batch-api-for-offline-workloads
This creates docs/notes/Day-___/ (summary, completion-checklist,
architect-thinking, posture-check, files-changed — all empty templates),
docs/architecture/day-___-<slug>.md, and Infra/Day-___/appsettings-template.md
(stub — populate during STEP 6 or 7 once settings are known).

If it DOES exist (resuming a day), skip /new-day — the skeleton is already there.

After scaffolding, paste:
```
Print the STEP 4 DRAFT prompt with this day's number filled in, prefixed with
"OPEN A NEW CHAT (Sonnet)".
```
Leave Claude Code open. → Go to STEP 4 in a new chat.

---

## STEP 4 — DRAFT  (new chat · Sonnet)

Paste:
```
I'm starting Day ___ of my cloud-architecture-lab roadmap.
PREVIOUS DAY: Day ___ completed ___.
TODAY'S FOCUS: ___  (from STEP 2 DECIDE)
CAREER PHASE: ___ (AI Engineer | Forward-Deployed Engineer | LLM Architect)
CONSTRAINTS: ___

Produce the Day ___ summary.md using the 13-section daily roadmap format
from CLAUDE.md. One response. No code.
```
Blanks: previous day = what you shipped. Focus = from STEP 2. Constraints =
anything broken/deferred. Don't know the focus? Replace CONSTRAINTS with:
"Recommend today's focus from CLAUDE.md and yesterday's parking lot."

Copy the summary. → STOP. Close chat.
→ Next: STEP 5 (ADR) if the summary lists one, else STEP 6 (POPULATE) in Claude Code.

---

## STEP 5 — ADR REASONING  (new chat · latest Opus) — skip if summary lists no ADR

This step produces the ADR *content* through reasoning. The file gets
created in STEP 6 with the /adr command.

Paste:
```
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

Copy the ADR content. → STOP. Close chat.
→ Next: STEP 6 — POPULATE, back in Claude Code.

---

## STEP 6 — POPULATE  (back in Claude Code)

The skeleton from STEP 3 already exists. Now fill it.

Paste the approved summary into the stub:
```
Replace the template content in docs/notes/Day-___/summary.md with:
<PASTE APPROVED SUMMARY FROM STEP 4>
Then extract the completion checklist into
docs/notes/Day-___/completion-checklist.md.
```

Then create and fill the ADR (skip if no ADR today):
```
/adr <kebab-case-title>
```
Example: /adr implement-prompt-caching-inside-provider-boundary
This finds the next ADR number and creates the file from the template. Then:
```
Replace the template content in docs/adr/ADR-___-<title>.md with:
<PASTE APPROVED ADR FROM STEP 5>
Set Status: Accepted, Date: today.
When finished, print the STEP 7 BUILD prompt with the day number and the
first phase's "Changes needed" bullets (from the summary) and "DO NOT modify"
list (from the ADR) filled in.
```

---

## STEP 7 — BUILD  (Claude Code, one prompt per phase)

For each phase in the summary's Step-by-Step Execution, paste:
```
Implement Phase ___ of Day ___.
Read first: docs/notes/Day-___/summary.md (Phase ___ section),
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

→ STOP when build passes clean.

---

## STEP 8 — TEST + AUDIT  (Claude Code)

Paste:
```
Local verification for Day ___.
Run the app, execute the tests in docs/notes/Day-___/completion-checklist.md.
Report pass/fail for each. Fix failures and re-test.
```
→ Fix all failures before continuing.

Then, once local tests pass, run the pre-deploy pillars audit:
```
Pillars audit for Day ___.
Read: docs/notes/Day-___/summary.md, docs/notes/Day-___/files-changed.md,
src/lab-observability-api/Program.cs, and every source file listed in
files-changed.md that was added or modified this day.
Run all 6-pillar checks from .claude/skills/pillars-audit/SKILL.md.
Report GREEN/YELLOW/RED per pillar with specific evidence from the code.
List any RED items — these block deploy and must be fixed first.
Document any YELLOW items as known debt in files-changed.md (step: audit).
When the audit is complete with no RED items, print the STEP 9 DEPLOY prompt
with the day number filled in.
```
→ STOP when local tests pass AND audit returns no RED items.

---

## STEP 9 — DEPLOY  (Claude Code)

Settings are applied automatically: `/deploy` reads `Infra/Day-___/appsettings-template.md`
and applies any settings there before publishing. You don't add them manually.
Just confirm the template is populated (it's written during STEP 7 build) and run:
```
/deploy
```
This runs the Kudu zip path from .claude/skills/azure-deploy/SKILL.md and
verifies /health, /swagger, and POST /api/ai/chat post-deploy.

Then telemetry verification (if today added telemetry):
```
Wait 3 minutes, then run the KQL check from the completion checklist.
Report results.
When Azure tests pass, print the STEP 10 DOCUMENT prompt with the day number
filled in.
```
→ STOP when Azure tests pass.

---

## STEP 10 — DOCUMENT  (Claude Code)

Paste:
```
Day ___ documentation pass.
1. Update docs/standards/kql-cookbook.md with new queries
2. Update docs/standards/azure-environment.md if resources/settings changed
3. Finalize docs/notes/Day-___/completion-checklist.md (mark [x])
4. Write docs/notes/Day-___/architect-thinking.md with key insights
   — include a "CEO Framing" subsection: one sentence on the business value of today's change
   — include a "Phase Note" subsection: which career phase did this day reinforce and why
5. Update CLAUDE.md (day status, new ADRs, new conventions)
6. Update docs/notes/Day-___/files-changed.md — upsert one row per file touched
   this pass; dedup on file path; label Step column "docs pass"
7. git add -A && git commit -m "feat(day-___): ___"
Report what changed.
```

Then update cert study materials for domains touched today:
```
/cert-update ___
```
(Reads the day's summary cert section, populates only the domains touched.)

After /cert-update, paste:
```
Print the STEP 11 REFLECT prompt with the day number filled in, prefixed with
"OPEN A NEW CHAT (latest Opus)".
```

---

## STEP 11 — REFLECT  (new chat · latest Opus)

Paste:
```
Posture check for Day ___.
What I built: ___
What broke: ___
Ask me the five posture questions from _principles.md and then provide the answers to those questions in a way that a 10 year old, a CEO, an Engineer, and an LLM Architect could understand:
1. Whose problem did I actually solve today?
2. What would I refuse to ship?
3. What did I try, fail at, and learn?
4. Can I explain this at 10yo, CEO, Engineer, AND Architect level?
5. Which pillar took the most damage today, and what's the minimum fix?
Push back hard if my answers are weak or self-congratulatory.
```
Copy the result. → STOP. Close chat.
→ Next: STEP 12 — CLOSE, back in Claude Code.

---

## STEP 12 — CLOSE  (Claude Code)

Paste:
```
Update docs/notes/Day-___/posture-check.md:
<PASTE POSTURE CHECK>
Add graveyard entries to docs/standards/_principles.md:
<PASTE ENTRIES>
Analyze and update all documents in the .claude, docs, and infra directories and subdirectories and both CLAUDE.md files so that the entire repo remains in tact without loop holes or missing or incomplete elements 
git add -A && git commit -m "docs(day-___): posture check and graveyard"
When committed, print: "Day ___ closed. Tomorrow → STEP 0 to verify this day
is frozen, then begin Day ___+1."
```

Done. Tomorrow → STEP 0.

---

## ONE-TIME SETUP (not part of the daily loop)

Run once, early — ideally end of Day 7 — to build the cert study structure:
```
/cert-scaffold AZ-900
/cert-scaffold AZ-104
/cert-scaffold AZ-305
/cert-scaffold AI-102
```
After this, /cert-update in STEP 10 populates domains as you touch them.

---

## IF YOU GET STUCK

- Don't know a blank? It's in your summary.md from STEP 4.
- Build won't pass? Stay in Claude Code, paste the error, let it fix.
- Need to reason mid-build? New short chat (Sonnet), ask, close it, return.
- Chat feels long (>6 messages)? Close it, open a new one.
- About to type "give me the code" in chat? Stop — that's Claude Code.
- A slash command does nothing? Check it exists: "Show me .claude/commands/<name>.md"
- summary.md missing when BUILD needs it? You skipped STEP 6 — populate it first.
- Handoff didn't print? Just open daily-workflow.md to the next step yourself —
  the printed prompt is a convenience, not a dependency.

---

## COMMAND REFERENCE

| Command | Step | What it does |
|---|---|---|
| /new-day N slug | 3 | Scaffolds the day folder + architecture file + Infra dir |
| /adr title | 6 | Creates next-numbered ADR file from template |
| /deploy | 9 | Kudu zip deploy + post-deploy verification |
| /cert-update N | 10 | Populates cert domains touched that day |
| /cert-scaffold EXAM | one-time | Builds cert domain structure from MS skills outline |

---

## TOKEN NOTE

The latest Opus runs on STEP 5 and STEP 11 only — the two steps where
reasoning depth is the point. Everything else is Sonnet. If you ever catch
yourself on Opus during a build, test, deploy, or doc step, you're
overspending — switch back. This mirrors your own gateway's rule: cheapest
model that solves the problem, escalate only when warranted.
