---
name: adr-writer
description: Write a new Architecture Decision Record (ADR) following this repo's conventions. Use when the user asks to document an architectural decision, create an ADR, or capture a technical tradeoff. Also use when introducing a new pattern, framework, dependency, or major refactor.
allowed-tools: Read, Write
---

# ADR Writer

## When to use
- User says "write an ADR", "document this decision", "ADR for..."
- Introducing a new framework, library, pattern, or service
- Making a tradeoff that future-you will need to understand
- Reversing or revisiting a prior decision (use "Supersedes" link)

## Why ADRs — by career phase

**AI Engineer:** "This is how I prove I thought before I coded." An ADR
written before implementation forces you to name the alternatives you
rejected — which is the difference between a decision and a guess.

**Forward-Deployed Engineer:** "This is how I show the customer we made a
real decision, not a guess." Customers trust engineers who can explain not
just what they built but why they didn't build the three other things they
considered. The ADR is that explanation in writing.

**LLM Architect:** "This is the audit trail for governance." At enterprise
scale, decisions affect multiple teams and persist for years. ADRs are how
you defend choices in design reviews, onboard senior engineers without
re-explaining history, and pass compliance audits. They are not optional.

ADRs are written BEFORE the implementation in all phases — the act of
writing exposes weak reasoning that code alone never surfaces.

## Filename convention
- Path: `docs/adr/ADR-NNN-kebab-case-title.md`
- NNN = 3-digit zero-padded, sequential (don't reuse retired numbers)
- Title = kebab-case, verb-led when possible
- Examples:
  - `ADR-005-introduce-provider-abstraction-for-claude-integration.md`
  - `ADR-006-adopt-serilog-with-application-insights-sink.md`

## Status lifecycle
- **Proposed** — drafted, not yet accepted
- **Accepted** — current truth; do not edit, supersede instead
- **Superseded by ADR-NNN** — replaced; keep file for history
- **Deprecated** — no longer relevant but kept for context

## Template (use this exactly)

```markdown
# ADR-NNN: <Title — verb-led, present tense>

## Status
Proposed | Accepted | Superseded by ADR-XXX | Deprecated

## Date
YYYY-MM-DD

## Context
What problem are we solving? What forces are at play (technical,
organizational, regulatory, cost)? What constraints exist?
Be specific — name the system, the file, the boundary.

## Decision
The decision, in one paragraph. Imperative mood. "We will..."
Name the chosen option clearly.

## Alternatives Considered
For each alternative, state:
- What it is
- Why we did NOT choose it
- What would have to change for us to revisit

## Consequences
### Positive
- ...
### Negative
- ...
### Neutral / Tradeoffs
- ...

## Implementation Notes
- Files affected
- Migration steps (if any)
- Rollback strategy

## References
- Related ADRs
- Documentation links
- Issue / PR numbers
```

## Quality checklist (before marking Accepted)
- [ ] Title is verb-led and specific (not "Logging" — use "Adopt Serilog with Application Insights Sink")
- [ ] Context names the actual problem, not a generic concern
- [ ] At least 2 alternatives are documented and rejected with reason
- [ ] Consequences include negatives and tradeoffs (not just upsides)
- [ ] Implementation notes are concrete enough that a new engineer could execute

## Common mistakes (avoid)
- Writing the ADR after the code is merged (decision is rationalized, not reasoned)
- Listing only positives — every decision has a cost
- Vague alternatives like "we could use something else" — name them
- Editing an Accepted ADR — supersede with a new one instead
- Missing the date — ADRs are temporal artifacts; the date matters