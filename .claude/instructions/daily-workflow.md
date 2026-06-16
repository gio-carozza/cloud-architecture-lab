# Daily Workflow — Overview

> Follow top to bottom every day. Fill the blanks marked ___.
> When you see STOP, close the chat before continuing.
> Full step prompts: `.claude/instructions/daily-workflow-steps.md`

---

## THE SHAPE OF EVERY DAY

```text
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
STEP 12 CLOSE          Claude Code · Sonnet    save posture check + commit + /repo-audit
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

---

## THE AUTO-PRINT HANDOFF (how each step hands you the next)

Every Claude Code step ends by PRINTING the next step's prompt, pre-filled
with the day number and known paths — so you never hunt for what's next.
To activate it, the last line of each Claude Code prompt already says:
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

## IF YOU GET STUCK

- Don't know a blank? It's in your `01-summary.md` from STEP 4.
- Build won't pass? Stay in Claude Code, paste the error, let it fix.
- Need to reason mid-build? New short chat (Sonnet), ask, close it, return.
- Chat feels long (>6 messages)? Close it, open a new one.
- About to type "give me the code" in chat? Stop — that's Claude Code.
- A slash command does nothing? Check it exists: "Show me `.claude/commands/<name>.md`"
- `01-summary.md` missing when BUILD needs it? You skipped STEP 6 — populate it first.
- Handoff didn't print? Open `.claude/instructions/daily-workflow-steps.md` to the next step —
  the printed prompt is a convenience, not a dependency.

---

## COMMAND REFERENCE

| Command | Step | What it does |
|---|---|---|
| `/new-day N slug` | 3 | Scaffolds the day folder + architecture file + Infra dir |
| `/adr title` | 6 | Creates next-numbered ADR file from template |
| `/deploy` | 9 | Kudu zip deploy + post-deploy verification |
| `/cert-update N` | 10 | Populates cert domains touched that day |
| `/cert-scaffold EXAM` | one-time | Builds cert domain structure from MS skills outline |
| `/collab-lens N` | 6 | Inserts collaborator lens block into `01-summary.md` |
| `/repo-audit N` | 12 | End-of-day repo health check + auto-fix |
| /devil gate | STEP 5 | Strongest counterargument to the ADR before accepting — active Day 010+ |
| /pitch gate | STEP 4 | 30-second CFO pitch validating CEO Framing — active Day 010+ |
| /10x gate | STEP 11 | Scale ceiling audit — optional Day 010–050, mandatory Day 051+ |

---

## TOKEN NOTE

The latest Opus runs on STEP 5 and STEP 11 only — the two steps where
reasoning depth is the point. Everything else is Sonnet. If you ever catch
yourself on Opus during a build, test, deploy, or doc step, you're
overspending — switch back. This mirrors your own gateway's rule: cheapest
model that solves the problem, escalate only when warranted.
