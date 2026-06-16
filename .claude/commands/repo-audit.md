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
- `07-files-changed.md` — must exist and be a valid markdown table (columns: File, Step, Change); must have at least one row beyond the header
- `03-architect-thinking.md` — must exist if the day is being closed (STEP 12 creates it); warn if missing
- `04-posture-check.md` — must exist if STEP 11 was run; warn if missing
- `05-audit-log.md` — must exist if STEP 8 was run; warn if missing

Report ✅ / ⚠️ / ❌ per file. Do NOT create missing files — report them so the operator knows what to add.

### 2. ADR structure

For any ADR whose filename contains the word "Day-<NNN>" in its date field, OR any ADR modified today (check `07-files-changed.md` for rows in `docs/adr/`):

- Has `## Status` heading with value `Accepted`, `Proposed`, or `Superseded`
- Has `## Date` heading with a date value (not blank)
- Has `## Context`, `## Decision`, `## Consequences` headings
- Has `## Alternatives Considered` heading with at least one alternative

Also verify that the ADR number sequence in `docs/adr/` has no gaps — list ADR-001 through ADR-NNN and confirm all exist.

### 3. Markdownlint

Run:

```bash
markdownlint "**/*.md" --ignore "docs/architecture/**" --ignore "node_modules/**"
```

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

### 6. 07-files-changed.md coverage

Read `docs/notes/Day-<NNN>/07-files-changed.md`. For each file path in the table:

- Verify the file actually exists in the repo (warn on orphan rows for deleted files — they are okay if intentional, but should be noted)

Then scan `07-files-changed.md` itself for any file in `docs/notes/Day-<NNN>/` that is NOT listed in the table. If a day folder file is missing from the log, add its row (step: `close-audit`, change: `created — not logged during session`).

### 7. Cert coverage

Read `docs/notes/Day-<NNN>/01-summary.md`. Find the "Certification Reinforcement" section. For each domain marked **Primary**, check whether `docs/certifications/<exam>/domains/<NNN-domain>/day-mapping.md` contains "Day-<NNN>". If not, warn that `/cert-update <NNN>` was not run for that domain.

### 8. Provider abstraction contract check

Run a quick grep to verify no Anthropic-specific types leaked into the provider-agnostic contracts:

```bash
grep -r "Anthropic\|claude\|Claude" src/lab-observability-api/Models/AI/ --include="*.cs"
```

Any match in `ChatRequest.cs`, `ChatResponse.cs`, or `ChatChunk.cs` is a ❌ violation. Matches in `ChatUsage.cs` for field names are acceptable — note them but do not flag as ❌.

### 9. Drift check — src changes reflected everywhere (git-diff-based, token-optimized)

**Purpose:** catch any case where code changed in `src/` but skill docs, ADRs, `CLAUDE.md`, notes, or certifications still reference old names, paths, or signatures. Uses git as the source of truth — not a manually-maintained log.

**Token contract:** the entire check runs on grep output and git diff output. No `Read` calls on source files or doc files unless a confirmed mismatch requires a targeted fix. On a clean day (nothing renamed or removed), total overhead is ~4 shell commands and zero reads.

---

#### Step 1 — Find today's commit range (one git command)

```bash
git log --pretty=format:"%H" --grep="[Dd]ay.0*<NNN>" | tail -1
```

This finds the hash of the **oldest commit** for Day NNN (commits whose message contains e.g. `day-010` or `Day-010`). Call this `OLDEST`.

- If `OLDEST` is empty (no commits yet): use `HEAD` as base and also check `git status --short` for uncommitted `.cs` changes.
- If `OLDEST` is found: base = `OLDEST^` (the parent commit, i.e. where the day started).

```bash
BASE=$(git log --pretty=format:"%H" --grep="[Dd]ay.0*<NNN>" | tail -1)
[ -z "$BASE" ] && BASE="HEAD" || BASE="${BASE}^"
```

---

#### Step 2 — Get all files changed today (one git command)

```bash
git diff "$BASE" HEAD --name-only --diff-filter=ACMRD
```

Flags: A=added, C=copied, M=modified, R=renamed, D=deleted. This is the complete set of files touched since the day began.

Split the output into two buckets:

- **`src/*.cs` files** → source changes that may need doc updates
- **`.md` files outside `docs/architecture/`** → doc changes that may reference source symbols

If the `src/*.cs` bucket is empty:
→ Print `✅ Check 9: skipped — no src/*.cs files changed today` and stop.

---

#### Step 3 — Extract removed/renamed public symbols from source diff (one git command)

```bash
git diff "$BASE" HEAD -- "src/" \
  | grep "^-[^-]" \
  | grep -E "\bpublic\b" \
  | grep -oE "[A-Z][A-Za-z][A-Za-z0-9]+"
```

This pipeline:

1. Gets only the diff of `src/` files
2. Keeps only removed lines (starting with `-`, excluding the `---` file header)
3. Filters to lines declaring something `public` (methods, properties, fields, classes)
4. Extracts PascalCase identifiers (the symbol names)

The result is a deduplicated list of **symbols that existed before today but may no longer exist or have been renamed**. These are the only symbols that can cause stale references in docs. Newly added symbols cannot be stale.

Also extract renamed/moved **folder paths** from the `src/` changed file list:

```bash
git diff "$BASE" HEAD --name-only --diff-filter=ACMRD \
  | grep "^src/" \
  | sed 's|src/lab-observability-api/||; s|/[^/]*$||' \
  | sort -u
```

This gives the set of subfolders touched (e.g. `Services/AI`, `Models/AI`, `Telemetry`). If any folder was renamed, the old name may appear in doc paths.

---

#### Step 4 — Search the entire repo's .md files for removed symbols (grep, file-list only)

For each removed symbol extracted in Step 3, run one grep across ALL `.md` files in the repo (excluding `docs/architecture/`):

```bash
grep -rln "<SYMBOL>" \
  --include="*.md" \
  --exclude-dir="docs/architecture" \
  --exclude-dir="node_modules" \
  .
```

The `-l` flag returns **file names only** — not content. This is the cheapest possible search: it tells you which files reference the symbol without reading their content.

For each **file returned**:

```bash
# Check if the NEW name (from + lines of the diff) is also present in that file
grep -c "<NEW_SYMBOL>" "<file>"
```

- Count > 0 → file already has the new name → ✅ already updated
- Count = 0 → file references the old name but not the new name → ❌ stale reference

To get `NEW_SYMBOL` (what replaced the removed symbol):

```bash
git diff "$BASE" HEAD -- "src/" \
  | grep "^+[^+]" \
  | grep -E "\bpublic\b" \
  | grep -oE "[A-Z][A-Za-z][A-Za-z0-9]+"
```

Same pipeline on `+` lines. Pair removed vs. added symbols by proximity in the diff (lines within 5 of each other are likely a rename).

---

#### Step 5 — Check for stale folder paths in docs

For each subfolder that appears in the diff (Step 3), check whether any `.md` file references a path that **no longer exists** in `src/`:

```bash
# Build list of current src/ subfolders
find src/lab-observability-api -type d \
  | sed 's|src/lab-observability-api/||' \
  | sort > /tmp/current_folders.txt

# For each folder referenced in docs, check it still exists
grep -rohn "src/lab-observability-api/[A-Za-z/]*" \
  --include="*.md" \
  --exclude-dir="docs/architecture" \
  . \
  | grep -vF -f /tmp/current_folders.txt
```

Any line returned is a `.md` file referencing a `src/` path that no longer exists — flag as ❌.

---

#### Step 6 — Targeted fix (only runs on confirmed drift)

When Step 4 or Step 5 finds a stale reference:

1. Run `grep -n "<OLD_SYMBOL>" "<file>"` to get the exact line number.
2. Read only that line range (`Read` tool with `offset` and `limit: 3`).
3. Make the targeted `Edit` — one line, not the whole file.
4. Re-run `grep -c "<OLD_SYMBOL>" "<file>"` to confirm count is now 0.

**No full-file reads. No full-file rewrites.**

---

#### Step 7 — Also check changed .md files for broken src references

For `.md` files that were themselves modified today (from the Step 2 `.md` bucket), verify that any `src/` path or C# symbol they mention still exists:

```bash
# Get only the added lines from changed .md files
git diff "$BASE" HEAD -- "*.md" \
  | grep "^+[^+]" \
  | grep -oE "src/lab-observability-api/[A-Za-z0-9/_.-]+" \
  | sort -u
```

For each path extracted, check it exists:

```bash
# Verify each referenced src path actually exists on disk
# (run as a loop or pipe to xargs test -e)
```

Any path that doesn't exist on disk → ❌ broken reference introduced in today's doc edits.

---

#### Cost summary

| Scenario | Commands | Reads |
|---|---|---|
| No `src/*.cs` changed today | 2 git commands | 0 |
| `src/*.cs` changed, no symbols removed | 3 git commands + 1 grep | 0 |
| Symbols removed, no stale doc refs found | 3 git + N symbol greps (file-list only) | 0 |
| Stale ref found in 1 file | Same + 1 targeted line grep + 1 Edit | 1 (3-line range) |
| Stale refs in 3 files | Same + 3 targeted line greps + 3 Edits | 3 (3-line ranges each) |

A clean day costs under 500 tokens for this entire check. A day with one rename costs under 1,000.

---

#### Output format for Check 9

```text
### 9. Drift check
  Scope: 4 src/*.cs files changed, 3 .md files changed
  Removed symbols: StreamFirstTokenMs, TryExtractUsage (2-tuple)
  Added symbols:   StreamTtftMs, TryExtractUsage (4-tuple)

  StreamFirstTokenMs → .claude/skills/pillars-audit/SKILL.md line 186: stale ❌ → fixed ✅
  TryExtractUsage    → .claude/skills/observability-net/SKILL.md line 181: stale ❌ → fixed ✅
  Providers/         → docs/adr/ADR-010 line 253: stale path ❌ → fixed ✅

  .md files modified today — src/ path refs: all valid ✅
```

---

## Output format

```text
## repo-audit — Day NNN — YYYY-MM-DD

### 1. Day folder completeness
  ✅ 01-summary.md
  ✅ 02-completion-checklist.md (all items checked)
  ✅ 07-files-changed.md (N rows)
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

### 6. 07-files-changed.md coverage
  ✅ All logged files exist
  ⚠️  docs/notes/Day-NNN/03-architect-thinking.md not in log — row added

### 7. Cert coverage
  ✅ AZ-900 Domain 001 — Day-NNN in day-mapping.md
  ❌ AZ-104 Domain 002 — /cert-update NNN not run for this domain

### 8. Provider abstraction
  ✅ No Anthropic types in ChatRequest / ChatResponse / ChatChunk

---
RESULT: ✅ N checks passed · ⚠️ N warnings · ❌ N blocking items
```

If any ❌ items exist, do NOT print "Day closed." Fix them first, then re-run `/repo-audit <NNN>`.

If only ⚠️ items remain, print a warning and ask the user whether to proceed.

If all green: print `repo-audit PASS — Day <NNN> is clean. Proceed to git commit.`
