---
name: collab-lens
description: Daily collaborator lens — connects today's build work to the 1 most relevant collaborator and at most 2 in-frame secondaries; inserts a bounded block into Day-NNN 01-summary.md
allowed-tools: Read, Write
---

# /collab-lens

**Usage:** `/collab-lens NNN`

Where `NNN` is the zero-padded day number (e.g., `010`).

---

## What this does

Executes `.claude/skills/collaboration-lens/SKILL.md` with day `NNN` substituted throughout.

Steps 0–6 in that file:
1. **Precondition check (STEP 0)** — confirms `docs/notes/Day-NNN/01-summary.md` exists and contains `## Whose Problem Am I Solving?`. Halts with an explicit message if not.
2. **Phase detection (STEP 1)** — reads `CLAUDE.md` for `**Current phase:`.
3. **Context extraction (STEP 2)** — reads ONLY the `## Whose Problem Am I Solving?` section of 01-summary.md.
4. **Phase-row filter (STEP 3)** — reads `docs/standards/collaboration-map.md`; loads only the phase rows matching today's phase.
5. **Collaborator selection (STEP 4)** — selects 1 PRIMARY + at most 2 secondary.
6. **Block emission + insertion (STEPs 5–6)** — writes the `### Collaboration Lens (Day NNN)` subsection into 01-summary.md; upserts 07-files-changed.md.

---

## Token discipline

Reads exactly two named sections of 01-summary.md (the `## Whose Problem Am I Solving?` body and, optionally, the ADR artifact line). Does NOT read the full 01-summary.md. Does NOT read any other day's folder. Reads collaboration-map.md once, phase-filtered.

**Total new files read this run:** 01-summary.md (partial), CLAUDE.md (one line), collaboration-map.md (phase rows only).

---

## Step-0 precondition note

If `## Whose Problem Am I Solving?` is absent, the skill STOPS immediately. This section is written during STEP 6 of the daily workflow. Run `/collab-lens` after STEP 6 completes, not before.

---

## Output contract

The inserted block is always ≤ 12 content lines:
- 1 PRIMARY: posture (1 line) + crucial question (1 line) + 4-level compression (4 lines, 1 sentence each)
- AT MOST 2 secondary: 1 line each
- Omits the "Also in frame" heading if no secondaries apply

No new files are created. Two existing files are modified: 01-summary.md (insert) and 07-files-changed.md (upsert).
