---
name: repo-audit
description: End-of-day repo health check. Verifies day folder completeness, ADR structure, markdownlint, backtick conventions, CLAUDE.md accuracy, cert coverage, provider abstraction integrity, and skill drift against source code. Auto-fixes what it can. Token-optimized — skill drift uses grep-only, no full-file reads unless a mismatch is found.
allowed-tools: Bash, Read, Write, Edit, Glob, Grep
---

# repo-audit

## Usage

```text
/repo-audit <NNN>
```

Example: `/repo-audit 010`

Run at the end of every day session, after STEP 10 (Document) and before final git commit.

---

## What this checks

### 1. Day folder completeness

For `docs/notes/Day-<NNN>/`:

- `01-summary.md` — must exist and be non-empty
- `02-completion-checklist.md` — must exist; count unchecked `[ ]` items; report any that are NOT marked with an explicit deferral note
- `docs/notes/changelog.md` must have a `## Day <NNN>` section with a valid markdown table (columns: File, Step, Change) and at least one row beyond the header
- `03-architect-thinking.md` — must exist if the day is being closed (STEP 12 creates it); warn if missing
- `04-posture-check.md` — must exist if STEP 11 was run; warn if missing
- `05-audit-log.md` — must exist if STEP 8 was run; warn if missing

Report ✅ / ⚠️ / ❌ per file. Do NOT create missing files — report them so the operator knows what to add.

### 2. ADR structure

For any ADR whose filename contains the word `Day-<NNN>` in its date field, OR any ADR modified today (check the current day's `## Day <NNN>` section in `docs/notes/changelog.md` for rows in `docs/adr/`):

- Has `## Status` heading with value `Accepted`, `Proposed`, or `Superseded`
- Has `## Date` heading with a date value (not blank)
- Has `## Context`, `## Decision`, `## Consequences` headings
- Has `## Alternatives Considered` heading with at least one alternative

Also verify that the ADR number sequence in `docs/adr/` has no gaps — list ADR-001 through ADR-NNN and confirm all exist.

### 3. Markdownlint

Run:

```bash
markdownlint "**/*.md" ".claude/**/*.md" --ignore "docs/architecture/**" --ignore "node_modules/**"
```

The `.claude/**/*.md` glob is required in addition to `**/*.md` — `**` does not match
dotfolders by default, so `**/*.md` alone silently skips everything under `.claude/`
(skills, commands, hooks instructions) and reports a false "0 violations." Confirmed
during the 2026-06-16 audit: two pre-existing MD033 violations in
`.claude/commands/repo-audit.md` itself had never been caught by this check.

If violations found: run `markdownlint --fix` on each offending file, then re-run to confirm zero.
Report which files were auto-fixed vs. which still have violations.

### 4. Backtick conventions

Run:

```bash
node scripts/md-backtick-wrap.js
```

Report which files were updated (if any). This is a safe, idempotent operation — running it when there is nothing to fix produces no changes.

### 5. CLAUDE.md accuracy

Read `CLAUDE.md` and verify:

- `docs/notes/_index.md` shows Day-`<NNN>` with status `Complete` (`CLAUDE.md` has no day counter — `_index.md` is the source of truth)
- Any new ADR created today is mentioned in the Phase 1 / Phase 2 / Phase 3 section with the correct day number
- Any new skills or commands added today are listed in `## Commands` or `## Architecture` sections if relevant

### 6. changelog.md coverage

Read `docs/notes/changelog.md`, find the current day's `## Day <NNN>` section. For each file path in that section's table:

- Verify the file actually exists in the repo (warn on orphan rows for deleted files — they are okay if intentional, but should be noted)

Then scan that section for any file in `docs/notes/Day-<NNN>/` that is NOT listed. If a day folder file is missing from the log, add its row under the current day's section (step: `close-audit`, change: `created — not logged during session`).

**Scope discipline:** this check (and any row you add) is about files `Day-<NNN>`'s own session actually touched — but the row always goes under the **current day's** section in `changelog.md`, regardless of which day's folder the file lives in. If a drift fix found elsewhere in this audit (Check 9) touches a file owned by a *different*, already-closed day (e.g. fixing a stale path in `Day-006/01-summary.md` during a Day-009 audit), log that row under `## Day 009`, not `## Day 006`. Git already has the authoritative timestamp for when the edit happened; the single-file format with day-of-edit sections (not day-of-file-creation sections) is exactly what replaced the old per-day-file convention, which forced this same lookup as a real cost (lesson from the 2026-06-16 audit).

### 7. Cert coverage

Read `docs/notes/Day-<NNN>/01-summary.md`. Find the "Certification Reinforcement" section. For each domain marked **Primary**, check whether `docs/certifications/<exam>/domains/<NNN-domain>/day-mapping.md` contains `Day-<NNN>`. If not, warn that `/cert-update <NNN>` was not run for that domain.

### 8. Provider abstraction contract check

Run a quick grep to verify no Anthropic-specific types leaked into the provider-agnostic contracts:

```bash
grep -r "Anthropic\|claude\|Claude" src/lab-observability-api/Models/AI/ --include="*.cs"
```

Any match in `ChatRequest.cs`, `ChatResponse.cs`, or `ChatChunk.cs` is a ❌ violation. Matches in `ChatUsage.cs` for field names are acceptable — note them but do not flag as ❌.

### 9. Drift check — doc references vs. actual source (script-based)

**Purpose:** catch any case where a doc (skill, standard, day note, or cert file)
references a C# type/member name, an `ai.*` telemetry string, or a `src/` path that
doesn't exist in current source — whether it was renamed, or simply never built
(a class a skill describes that was never actually created). Run:

```bash
node scripts/symbol-drift-check.js
```

This replaces the previous git-diff-range heuristic, which relied on finding "the
oldest commit for Day NNN" to define a diff base — that assumption breaks in this
repo because work is committed in bulk consolidation commits, not one commit per
day (confirmed during the 2026-06-16 audit: the heuristic's diff range spanned
nearly the entire repo history). The script instead rebuilds the full symbol/string
table from `src/` on every run and checks doc references against current truth —
no git history dependency, and it catches "never existed" drift that a diff-based
approach structurally cannot (nothing was ever removed, so there's no diff to find).

**Scope (by design, not a bug):** only inline single-backtick spans are checked —
fenced ` ```csharp ` examples are not parsed (too much noise from abbreviated
illustrative code). Only symbol names ending in a recognized suffix (`Controller`,
`Provider`, `Client`, `Options`, `Middleware`, `Telemetry`, `Exception`, `Handler`,
`Chunk`, `Usage`, `Job`, `Status`, `Result`) are checked, to avoid flagging generic
prose. `docs/adr/`, `docs/architecture/`, `*-log.md`, `docs/notes/changelog.md`,
`commit-convention.md`, `graveyard.md`, and explicit Phase-2/future-scope stub
standards are excluded — those genres intentionally name rejected or not-yet-built
designs. A small allowlist in the script covers common BCL/framework types and
designs explicitly rejected in an ADR.

**Output:** the script prints `file:line: \`span\` — reason` for each finding, or
`clean — 0 findings`. For each finding:

1. Read the line (`grep -n` or a 3-line `Read`) to confirm it's real drift and not
   a legitimate "renamed `OLD` → `NEW`" or "rejected alternative" narration (those
   read as correct in context even though the script can't tell the difference —
   check the sentence before flagging it as a problem).
2. If real: make the targeted `Edit`.
3. If a recurring false-positive pattern emerges (a new BCL type, a new rejected
   design name, a new stub file), add it to the relevant allowlist/exclude-list in
   `scripts/symbol-drift-check.js` rather than re-triaging it every run.

Re-run after fixes to confirm the finding count dropped to only legitimate cases.

---

## Output format

```text
## repo-audit — Day NNN — YYYY-MM-DD

### 1. Day folder completeness
  ✅ 01-summary.md
  ✅ 02-completion-checklist.md (all items checked)
  ✅ changelog.md — Day NNN section present (N rows)
  ⚠️  04-posture-check.md — missing (run STEP 11 if not done)

### 2. ADR structure
  ✅ ADR-001 through ADR-NNN — no gaps
  ✅ ADR-NNN — Status, Date, all required headings present

### 3. Markdownlint
  ✅ 0 violations (or: auto-fixed N files; 0 remaining)

### 4. Backtick conventions
  ✅ No changes (or: updated N files)

### 5. CLAUDE.md accuracy
  ✅ Day status current
  ⚠️  ADR-NNN not mentioned in Phase 1 section

### 6. changelog.md coverage
  ✅ All logged files exist
  ⚠️  docs/notes/Day-NNN/03-architect-thinking.md not in log — row added

### 7. Cert coverage
  ✅ AZ-900 Domain 001 — Day-NNN in day-mapping.md
  ❌ AZ-104 Domain 002 — /cert-update NNN not run for this domain

### 8. Provider abstraction
  ✅ No Anthropic types in ChatRequest / ChatResponse / ChatChunk

### 9. Drift check
  ✅ symbol-drift-check.js: clean — 0 findings (or: N findings → M fixed, K legitimate-as-is)

---
RESULT: ✅ N checks passed · ⚠️ N warnings · ❌ N blocking items
```

If any ❌ items exist, do NOT print "Day closed." Fix them first, then re-run `/repo-audit <NNN>`.

If only ⚠️ items remain, print a warning and ask the user whether to proceed.

If all green: print `repo-audit PASS — Day <NNN> is clean. Proceed to git commit.`
