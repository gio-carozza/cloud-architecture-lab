---
name: collaboration-lens
description: Daily lens connecting current day's work to the relevant collaborators from collaboration-map.md; inserts a bounded block into summary.md under "Whose Problem Am I Solving?"
allowed-tools: Read, Write
---

# Collaboration Lens Skill

## STEP 0 — Precondition (STOP here if not met)

Read `docs/notes/Day-NNN/summary.md`.

Confirm:
1. The file exists.
2. The file contains the section heading `## Whose Problem Am I Solving?`

If either condition fails, STOP. Print exactly:

> `Day-NNN summary.md missing or does not yet contain "## Whose Problem Am I Solving?" — run /collab-lens only after STEP 6 populates summary.md.`

Do not proceed. Do not attempt to create or populate the section yourself.

---

## STEP 1 — Identify current phase

Read `CLAUDE.md`. Find the line beginning `**Current phase:`. Extract the phase name (AI Engineer / Forward-Deployed / LLM Architect).

---

## STEP 2 — Read today's context

Read `docs/notes/Day-NNN/summary.md`. Extract:
- The body of the `## Whose Problem Am I Solving?` section.
- The primary ADR decision line if present in the day's artifact list.

Do NOT read any other section of summary.md.

---

## STEP 3 — Load the collaborator map for this phase

Read `docs/standards/collaboration-map.md`. For each collaborator block, read ONLY the table row matching the current phase. Ignore all other rows.

---

## STEP 4 — Select collaborators

From the phase rows read in STEP 3, identify:

**1 PRIMARY** — the collaborator whose phase problem most directly overlaps with the work described in STEP 2. Choose one. If two are equally close, prefer the one that is often overlooked at this phase (signal value matters).

**AT MOST 2 secondary** — collaborators who are touched peripherally by today's work. If no secondary is genuinely activated, emit none. Do not pad to reach 2.

---

## STEP 5 — Emit the lens block

Build the block below. **HARD CAPS — count every line before writing:**
- PRIMARY block: posture line (1) + question line (1) + four level-lines (4) = **6 lines**
- Each secondary: 1 line each
- Blank lines between sections do NOT count toward the cap
- **Total content lines ≤ 12**

If the block would exceed 12 content lines, cut a secondary or shorten a level-line to one sentence. Never cut the PRIMARY.

```
### Collaboration Lens (Day NNN)

**Primary — [Collaborator Name]**
Posture: [phase row "My posture" cell, made specific to today's work in ≤15 words]
Today's question: [phase row "Crucial question", made specific to today's actual decisions]

**10yo:** [one sentence — analogy anchored in today's work]
**CEO:** [one sentence — business framing of today's decision]
**Engineer:** [one sentence — the exact technical choice that matters to this collaborator]
**Architect:** [one sentence — the system-design implication]

**Also in frame:**
- [Secondary 1] — [why activated today, one line, ≤15 words]
- [Secondary 2] — [why activated today, one line, ≤15 words]
```

Omit the `**Also in frame:**` heading if no secondaries were selected.

---

## STEP 6 — Insert and audit

**Insert** the block into `docs/notes/Day-NNN/summary.md` immediately after the `## Whose Problem Am I Solving?` heading line, before any existing body text. If a `### Collaboration Lens` subsection already exists, replace it in place — do not duplicate.

**Upsert** `docs/notes/Day-NNN/files-changed.md`. Dedup key is the file path. If a row for `docs/notes/Day-NNN/summary.md` already exists, update the Change cell in place. Add a row for `files-changed.md` itself as the final action.

| File | Step | Change |
|---|---|---|
| `docs/notes/Day-NNN/summary.md` | collab-lens | Collaboration Lens block inserted under "Whose Problem Am I Solving?" — primary: [Collaborator Name] |
| `docs/notes/Day-NNN/files-changed.md` | collab-lens | This file — collab-lens rows upserted |
