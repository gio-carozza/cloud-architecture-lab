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

## Why ADRs

- **AI Engineer:** proves you thought before you coded — alternatives named before implementation
- **Forward-Deployed Engineer:** shows the customer a real decision was made, not a guess
- **LLM Architect:** audit trail for governance; defense in design reviews, onboarding, compliance

Write the ADR BEFORE the implementation — the act exposes weak reasoning that code never surfaces.

## Filename convention

- Path: `docs/adr/ADR-NNN-kebab-case-title.md`
- NNN = 3-digit zero-padded, sequential (don't reuse retired numbers)
- Title = kebab-case, verb-led when possible
- Examples:
  - `ADR-005-introduce-provider-abstraction-for-claude-integration.md`
  - `ADR-006-harden-ai-gateway-with-resilience-and-observability.md`

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
