# /new-day

Scaffold a new day in the LLM Architect roadmap.

## Usage
`/new-day 6 observability-and-resilience`

## What this does

Given a day number and a kebab-case slug, create:

1. `docs/notes/Day-NNN/` directory with:
   - `summary.md` (Day overview)
   - `completion-checklist.md` (concrete done criteria)
   - `architect-thinking.md` (tradeoffs & enterprise reasoning)
   - `posture-check.md` (the daily honesty audit)
   - `files-changed.md` (running audit log of every file modified during the day)
   - Note: KQL queries go directly to `docs/standards/kql-cookbook.md`, NOT a day-local kql.md

2. `docs/architecture/day-NNN-<slug>.md` (architecture changes for the day)

3. `Infra/Day-NNN/` directory containing `appsettings-template.md` — a stub
   documenting any new App Service settings this day introduces. If the day
   adds no new settings, the stub says "No new app settings this day." This
   file is the single source of truth `/deploy` reads before publishing.

4. Append a "## Day NNN" entry to `docs/notes/_index.md`

## Required sections in summary.md

```markdown
# Day NNN — <Title>

## Track
<Build | Cert | Hybrid>

## Focus
<one-line>

## Why This Matters
<enterprise context — production reality>

## Whose Problem Am I Solving?
<the specific human user — doctor, customer, ops engineer, future-me-on-call>
<if you can't name them, stop and rethink the day>

## What I Will Build
- ...

## Step-by-Step Execution
1. ...

## Architect Thinking
<tradeoffs, alternatives rejected, what elite architects do differently>

## Artifacts
- Code:
- Docs:
- Infra:

## Portfolio Value
<what this proves to a hiring panel>

## Completion Checklist
See completion-checklist.md

## Certification Reinforcement
- AZ-900: <Primary | Secondary | None> — <concepts>
- AZ-104: <Primary | Secondary | None> — <concepts>
- AZ-305: <Primary | Secondary | None> — <concepts>
- AI-102: <Primary | Secondary | None> — <concepts>

## Architect Posture Check
See posture-check.md (filled at end of day, BEFORE marking complete)
```

## Required sections in posture-check.md

```markdown
# Day NNN — Posture Check

> Honest answers only. The graveyard is more valuable than the trophy case.

## 1. Whose problem did I actually solve today?
<name a specific human role; "the platform" is not an answer>

## 2. What would I refuse to ship if I were the only one in the room?
<name the corner you were tempted to cut, and whether you cut it>

## 3. What did I try, fail at, and learn?
<add to docs/standards/_principles.md "Graveyard" table>

## 4. Could I explain today's work to a 10-year-old AND defend it at a doctorate level?
<if no, the work isn't owned yet — schedule a teach-back session>
```

## Required initial content for appsettings-template.md

Scaffold with the no-new-settings stub. Replace with real settings during STEP 6
(populate) once the day's summary is written and it's clear what config changes
the day introduces. `/deploy` reads this file at step 1b — it must exist.

```markdown
# Day NNN — App Service Settings Template

No new app settings this day.
```

## Required initial content for files-changed.md

Scaffold with the header and an empty table. Claude Code populates rows during
every doc-update pass; dedup key is the file path.

```markdown
# Day NNN — Files Changed

| File | Step | Change |
|---|---|---|
```

## Reminders
- Use 3-digit zero-padded day numbers (Day-006, not Day-6)
- Inside the day folder, no day prefix on filenames
- In shared folders (architecture/, etc.), KEEP the day prefix
- Posture check is filled at the END of the day, before commit
- `files-changed.md` is scaffolded empty and populated progressively — never pre-fill it