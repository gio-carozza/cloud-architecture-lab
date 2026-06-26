---
name: sync-check
description: Day-agnostic reference integrity and naming consistency check. Runs symbol-drift-check.js and name-check.js, then verifies markdown link targets, standards-index paths, ADR sequence, cert day-mapping existence, changelog path integrity, and command index. Replaces the inline `node scripts/symbol-drift-check.js` call in STEP 8.
allowed-tools: Bash, Read, Glob, Grep
---

# sync-check

## Usage

```text
/sync-check
```

No arguments. Run any time — does not require a day number.

In the daily loop: use in **STEP 8** in place of the inline `node scripts/symbol-drift-check.js` call.
On-demand: run whenever you want a fast integrity pass without closing the day.

---

## Checks

### 1. Symbol drift

```bash
node scripts/symbol-drift-check.js
```

Flags backtick-wrapped C# type names (suffix-filtered), `ai.*` telemetry strings, and `src/` paths in docs that no longer exist in source. Full rationale in `.claude/commands/repo-audit.md` Check 9.

Fix every real finding with a targeted `Edit`. Legitimate "renamed `OLD` → `NEW`" narrations and rejected-ADR alternatives are not violations — re-run to confirm only those remain.

### 2. Naming consistency

```bash
node scripts/name-check.js
```

Extends Check 1 with four additional checks:

- **Broader C# suffix list** — Service, Manager, Builder, Factory, Request, Response, Context, Pipeline, Validator, etc.
- **Azure resource names** — backtick spans carrying lab markers (`-gio`, `-dev-`, `-eastus`) validated against `docs/standards/azure-environment.md`
- **Slash command refs** — `/command-name` spans with known prefixes validated against `.claude/commands/`
- **ADR references** — `ADR-NNN` spans validated against `docs/adr/`

Same fix protocol as Check 1.

### 3. Markdown link integrity

Grep all `.md` files for markdown link targets of the form `[text](path)`. For every relative path found (does not start with `http://` or `https://`, is not an anchor-only `#ref`):

- Verify the file exists on disk.
- Report ❌ for each path that does not resolve.

Skip glob patterns, ellipsis-abbreviated paths (`...`), and template placeholders (`Day-NNN`, `ADR-NNN`).

### 4. Standards index integrity

Read `CLAUDE.md`. Find the `## Standards index` table. For every path in the `File` column, verify it exists under `docs/standards/`. Report ❌ for any missing file.

### 5. ADR sequence

List `docs/adr/`. Extract ADR numbers. Verify:

- Sequence is contiguous (no gaps between `ADR-001` and the highest number present)
- Every filename matches `ADR-NNN-kebab-case-title.md`

Report any gaps or malformed names.

### 6. Cert day-mapping existence

For every `day-mapping.md` under `docs/certifications/` (skip `ai-102/` — retired, historical record):

- Extract all `Day-NNN` references.
- For each, verify `docs/notes/Day-NNN/` exists on disk.
- Report ❌ for day folders referenced but not yet created (expected for future days — note these as ⚠️ rather than ❌ if the day number is ahead of the current day).

### 7. Changelog path integrity

Read `docs/notes/changelog.md`. For every file path in the `File` column of any `## Day NNN` table:

- Verify the file exists.
- Report ❌ orphan rows for paths that no longer exist. These may be intentional deletions — note them but do not auto-fix. The human confirms whether the row should be removed.

### 8. Command index

List `.claude/commands/`. Cross-reference with the `## Commands` table in `CLAUDE.md`. Report:

- ⚠️ Commands in `.claude/commands/` **not** listed in `CLAUDE.md` (undocumented)
- ❌ Commands listed in `CLAUDE.md` that have **no file** in `.claude/commands/` (broken reference)

---

## Output format

```text
## sync-check — YYYY-MM-DD

### 1. Symbol drift
  ✅ symbol-drift-check.js: clean — 0 findings
  (or: N findings — M fixed, K legitimate-as-is)

### 2. Naming consistency
  ✅ name-check.js: clean — 0 findings
  (or: N findings listed)

### 3. Markdown link integrity
  ✅ N relative links checked — all resolve
  (or: ❌ broken link: path/to/missing.md referenced in docs/foo.md:42)

### 4. Standards index integrity
  ✅ All N standards files exist
  (or: ❌ docs/standards/foo.md not found — listed in CLAUDE.md standards table)

### 5. ADR sequence
  ✅ ADR-001 through ADR-NNN — no gaps, all filenames valid

### 6. Cert day-mapping existence
  ✅ All day references resolve
  (or: ⚠️ docs/notes/Day-015/ not yet created — future day, expected)

### 7. Changelog path integrity
  ✅ All logged paths exist
  (or: ❌ orphan row: src/foo.cs — file deleted; confirm row removal)

### 8. Command index
  ✅ All commands documented and present
  (or: ⚠️ /sync-check in .claude/commands/ not listed in CLAUDE.md Commands table)

---
RESULT: ✅ N checks passed · ⚠️ N warnings · ❌ N blocking items
```

All ❌ items must be resolved before closing the day. ⚠️ items (undocumented commands, future-day cert references, orphan changelog rows for intentionally deleted files) are noted and reviewed but do not block.
