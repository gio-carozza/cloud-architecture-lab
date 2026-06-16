# Architect Thinking Template

**Phase:** lower priority — use this template when creating `03-architect-thinking.md` each day
**Applies to:** `docs/notes/Day-NNN/03-architect-thinking.md`

---

## How to use this

When STEP 10 (Document) says "write `03-architect-thinking.md`", use the sections below as the required structure. Not every section will have deep content every day — that is fine. A brief honest answer beats a padded non-answer.

The file is a reflection artifact, not a report. Write it in first person. Write what you actually think, including what you got wrong.

---

## Template

```markdown
# Day NNN — Architect Thinking

## What I built

One paragraph. What the system can do now that it could not do before today.
Focus on the capability, not the implementation.

## The decision I made

What was the key architectural decision today? (Reference the ADR if one was written.)
What was the alternative you rejected and why?
If no explicit decision was made, what implicit assumption is now baked into the code?

## What broke or surprised me

What did not work as expected? What took longer than it should have?
What assumption turned out to be wrong?
This is the most important section — honest failure analysis compounds faster than polished success narratives.

## CEO framing

One sentence. What is the business value of what was shipped today?
Frame it in terms a non-technical executive would care about: cost, risk, speed, or competitive advantage.
Example: "Today's streaming support cuts perceived response latency by ~60%, which is the difference between
a tool users tolerate and one they prefer."

## Phase note

Which career phase did today's work reinforce? Why?
- Phase 1 (AI Engineer): building the gateway, learning the APIs, owning cost and observability
- Phase 2 (Forward-Deployed Engineer): applying AI to business problems, CEO-level communication
- Phase 3 (LLM Architect): enterprise governance, multi-provider, compliance

If today spanned phases, name both and explain why.

## What I would change

If you were starting today over with what you know now, what would you do differently?
This is not self-criticism — it is the signal that you learned something.

## The pillar that took the most damage

Which of the six Well-Architected pillars was most stressed today?
(Reliability, Security, Cost Optimization, Operational Excellence, Performance Efficiency, Experience Optimization)
What is the minimum fix needed to restore it?
```

---

## Required sections

All six sections are required. Minimum length per section: two sentences. Maximum: one paragraph.

If a section genuinely does not apply (e.g., a pure documentation day with no decision): write "N/A — [one sentence explaining why]." Do not skip it silently.

---

## What NOT to write

- Do not summarize what the code does — that is in `01-summary.md`
- Do not list the files changed — that is in `07-files-changed.md`
- Do not restate the completion checklist — that is in `02-completion-checklist.md`
- Do not write "everything went well" with no qualification — the posture check in STEP 11 will surface the real answer anyway

---

## Relationship to posture check (STEP 11)

`03-architect-thinking.md` is written in STEP 10 before the posture check. It is your honest self-assessment before Opus pushes back. The posture check result goes in `04-posture-check.md`. These two documents together form the full daily reflection:

- `03-architect-thinking.md` = what you think before being challenged
- `04-posture-check.md` = what the challenge surfaced

Over time the gap between them should narrow — that gap is a measure of architect maturity.
