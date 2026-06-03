# Daily Workflow

> Follow top to bottom every day. Fill the blanks marked ___.
> When you see STOP, close the chat before continuing.
> Save as: `.claude/instructions/daily-workflow.md`

---

## THE SHAPE OF EVERY DAY

```
STEP 1  SCAFFOLD   Claude Code · Sonnet    /new-day  (build the empty skeleton FIRST)
STEP 2  PLAN       chat (new) · Sonnet     get the day's summary
STEP 3  ADR        chat (new) · latest Opus  reason out the decision (skip if none)
STEP 4  POPULATE   Claude Code · Sonnet    fill summary + checklist + /adr
STEP 5  BUILD      Claude Code · Sonnet    implement each phase
STEP 6  TEST       Claude Code · Sonnet    run locally
STEP 7  DEPLOY     Claude Code · Sonnet    /deploy + verify
STEP 8  DOCUMENT   Claude Code · Sonnet    update docs + commit + /cert-update
STEP 9  REFLECT    chat (new) · latest Opus  posture check
STEP 10 CLOSE      Claude Code · Sonnet    save posture check + commit
```

Three rules:
1. Chat for STEP 2, 3, 9 only. Claude Code for everything else.
2. Model: Sonnet everywhere EXCEPT STEP 3 and STEP 9, which use the LATEST
   Opus (currently Claude Opus 4.8 — confirm the newest Opus in the model
   picker; don't pin a version number).
3. New chat for each of STEP 2, 3, 9. Close it the moment that step ends.

Why scaffold first: the day folder and stub files must exist before anything
can be written into them. Build the empty skeleton, then plan into it, then
fill it. No chicken-and-egg.

Your slash commands (in .claude/commands/):
/new-day · /adr · /deploy · /cert-scaffold · /cert-update

---

## STEP 1 — SCAFFOLD  (Claude Code — first thing every day)

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
(stub — populate during STEP 4 or 5 once settings are known).

If it DOES exist (resuming a day), skip /new-day — the skeleton is already there.

Leave Claude Code open. → Go to STEP 2 in a new chat.

---

## STEP 2 — PLAN  (new chat · Sonnet)

Paste:
```
I'm starting Day ___ of my cloud-architecture-lab roadmap.
PREVIOUS DAY: Day ___ completed ___.
TODAY'S FOCUS: ___
CONSTRAINTS: ___

Produce the Day summary.md using the daily roadmap format from CLAUDE.md.
One response. No code.
```
Blanks: previous day = what you shipped. Focus = yesterday's parking lot or
CLAUDE.md north star. Constraints = anything broken/deferred. Don't know the
focus? Replace CONSTRAINTS with: "Recommend today's focus from CLAUDE.md and
yesterday's parking lot."

Copy the summary. → STOP. Close chat.

---

## STEP 3 — ADR REASONING  (new chat · latest Opus) — skip if summary lists no ADR

This step produces the ADR *content* through reasoning. The file gets
created in STEP 4 with the /adr command.

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

---

## STEP 4 — POPULATE  (back in Claude Code)

The skeleton from STEP 1 already exists. Now fill it.

Paste the approved summary into the stub:
```
Replace the template content in docs/notes/Day-___/summary.md with:
<PASTE APPROVED SUMMARY FROM STEP 2>
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
<PASTE APPROVED ADR FROM STEP 3>
Set Status: Accepted, Date: today.
```

---

## STEP 5 — BUILD  (Claude Code, one prompt per phase)

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
```
Blanks = the bullets under that phase + the ADR's "Files NOT affected."

Build fails? Paste: `Build failed with: ___ . Fix it. Rebuild. Report.`

→ STOP when build passes clean.

---

## STEP 6 — TEST  (Claude Code)

Paste:
```
Local verification for Day ___.
Run the app, execute the tests in docs/notes/Day-___/completion-checklist.md.
Report pass/fail for each. Fix failures and re-test.
```
→ STOP when local tests pass.

---

## STEP 7 — DEPLOY  (Claude Code)

Settings are applied automatically: `/deploy` reads `Infra/Day-___/appsettings-template.md`
and applies any settings there before publishing. You don't add them manually.
Just confirm the template is populated (it's written during STEP 5 build) and run:
```
/deploy
```
This runs the Kudu zip path from .claude/skills/azure-deploy/SKILL.md and
verifies /health, /swagger, and POST /api/ai/chat post-deploy.

Then telemetry verification (if today added telemetry):
```
Wait 3 minutes, then run the KQL check from the completion checklist.
Report results.
```
→ STOP when Azure tests pass.

---

## STEP 8 — DOCUMENT  (Claude Code)

Paste:
```
Day ___ documentation pass.
1. Update docs/standards/kql-cookbook.md with new queries
2. Update docs/standards/azure-environment.md if resources/settings changed
3. Finalize docs/notes/Day-___/completion-checklist.md (mark [x])
4. Write docs/notes/Day-___/architect-thinking.md with key insights
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

---

## STEP 9 — REFLECT  (new chat · latest Opus)

Paste:
```
Posture check for Day ___.
What I built: ___
What broke: ___
Ask me the four posture questions from _principles.md:
1. Whose problem did I actually solve today?
2. What would I refuse to ship?
3. What did I try, fail at, and learn?
4. Can I explain this at 10yo AND architect level?
Push back hard if my answers are weak or self-congratulatory.
```
Copy the result. → STOP. Close chat.

---

## STEP 10 — CLOSE  (Claude Code)

Paste:
```
Update docs/notes/Day-___/posture-check.md:
<PASTE POSTURE CHECK>
Add graveyard entries to docs/standards/_principles.md:
<PASTE ENTRIES>
git add -A && git commit -m "docs(day-___): posture check and graveyard"
```

Done. Tomorrow → Step 1.

---

## ONE-TIME SETUP (not part of the daily loop)

Run once, early — ideally end of Day 7 — to build the cert study structure:
```
/cert-scaffold AZ-900
/cert-scaffold AZ-104
/cert-scaffold AZ-305
/cert-scaffold AI-102
```
After this, /cert-update in STEP 8 populates domains as you touch them.

---

## IF YOU GET STUCK

- Don't know a blank? It's in your summary.md from Step 2.
- Build won't pass? Stay in Claude Code, paste the error, let it fix.
- Need to reason mid-build? New short chat (Sonnet), ask, close it, return.
- Chat feels long (>6 messages)? Close it, open a new one.
- About to type "give me the code" in chat? Stop — that's Claude Code.
- A slash command does nothing? Check it exists: "Show me .claude/commands/<name>.md"
- summary.md missing when BUILD needs it? You skipped STEP 4 — populate it first.

---

## COMMAND REFERENCE

| Command | Step | What it does |
|---|---|---|
| /new-day N slug | 1 | Scaffolds the day folder + architecture file + Infra dir |
| /adr title | 4 | Creates next-numbered ADR file from template |
| /deploy | 7 | Kudu zip deploy + post-deploy verification |
| /cert-update N | 8 | Populates cert domains touched that day |
| /cert-scaffold EXAM | one-time | Builds cert domain structure from MS skills outline |

---

## TOKEN NOTE

The latest Opus runs on STEP 3 and STEP 9 only — the two steps where
reasoning depth is the point. Everything else is Sonnet. If you ever catch
yourself on Opus during a build, test, deploy, or doc step, you're
overspending — switch back. This mirrors your own gateway's rule: cheapest
model that solves the problem, escalate only when warranted.
