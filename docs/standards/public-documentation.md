# Public Documentation Standard

**Phase:** all phases — applies immediately; critical before going public
**Applies to:** all `.md` files that will be rendered on the portfolio site or shared externally

---

## The challenge

This repo serves multiple audiences simultaneously:

| Audience | What they need | Tolerance for jargon |
|---|---|---|
| Engineers | Exact APIs, error codes, code patterns, architectural decisions | High |
| Architects | Tradeoffs, ADR rationale, system boundaries, non-functional requirements | High |
| Hiring managers / CTOs | Business value, judgment signals, communication clarity | Low |
| Security / Compliance auditors | Threat model, secret management, PII handling, incident response | Medium |
| DevOps / SRE | Deploy procedure, runbook, alert inventory, rollback steps | Medium |
| Junior developers learning from the repo | Concepts explained without assumed knowledge | Low |
| LinkedIn contacts | One insight, compressed | Very low |

A document that serves only engineers fails as a portfolio artifact. A document that serves only executives fails as a technical standard. The goal is layered documentation: scannable at the top, deep on scroll.

---

## The four-level rule

Every standard, ADR, and day summary that will appear on the portfolio site must be readable at four levels:

**10-year-old** — one analogy that captures the concept without jargon. Goes first. Sets the hook.

**CEO** — two sentences maximum. Business value, risk reduction, or competitive advantage. Numbers where possible.

**Engineer** — exact APIs, code patterns, common errors, correct behavior. The bulk of the document.

**Architect** — system design implications, tradeoffs, enterprise considerations, future evolution. Goes last.

Not every document needs all four as explicit sections. But every document should be written so a non-engineer can get something from the first paragraph, and an architect can get something from the last.

---

## Structural conventions for public-facing documents

### Headers

Use heading hierarchy consistently:

- `# Title` — document title only (one per file)
- `## Section` — major topic
- `### Subsection` — detail within a major topic
- `####` and deeper — avoid; restructure instead

### Opening paragraph (the hook)

Every public-facing document must open with one of:

- A statement of the problem the document solves
- A statement of why the reader should care
- A concrete analogy (10-year-old level)

Never open with a definition ("X is a...") or a table. Definitions and tables are for the body.

### Decision tables

When documenting a decision between alternatives, always show the alternatives:

```markdown
| Option | Why considered | Why rejected |
|---|---|---|
| Option A | ... | ... |
| Option B (chosen) | ... | — chosen |
| Option C | ... | ... |
```

This signals architectural maturity to hiring managers and provides audit trail for engineers.

### Status indicators

Documents describing system state (runbooks, checklists, feature status) should use clear status labels:

- `DONE` — implemented and verified
- `IN PROGRESS` — active work
- `PLANNED` — committed, not started
- `STUB` — placeholder for Phase N work
- `DEPRECATED` — no longer the standard; link to replacement

---

## markdownlint compliance (required for portfolio rendering)

All public documents must pass markdownlint with the rules in `.markdownlint.json`:

- `MD033` disabled (inline HTML allowed where needed for portfolio site rendering)
- `MD013` disabled (line length not enforced)
- All other rules enforced

The PostToolUse hook runs `markdownlint --fix` automatically on every Write/Edit. Before a document is published, run:

```bash
npx markdownlint-cli "docs/**/*.md" --ignore "docs/architecture"
```

Zero violations required before a document goes on the portfolio site.

---

## Security checklist before publication

Before any document is published externally:

- [ ] No API keys, subscription IDs, or credentials in content (even as examples)
- [ ] No internal email addresses beyond `gio.carozza@outlook.com` (already public on LinkedIn)
- [ ] No resource names that would help an attacker enumerate the subscription (resource names in `azure-environment.md` are acceptable — they are not secrets)
- [ ] No stack traces or internal error messages reproduced verbatim
- [ ] No pending TODO items that reveal unimplemented security controls as currently missing

---

## Audience-specific document review checklist

Run this before marking any standard or ADR as ready for public display:

**Engineer audience check:**

- [ ] Exact types/interfaces named and backtick-wrapped
- [ ] Code examples compilable (or labeled as pseudocode)
- [ ] Error codes and HTTP status codes explicit
- [ ] Links to related files use repo-relative paths

**CEO/hiring manager check:**

- [ ] First paragraph readable without technical background
- [ ] Business value of the feature/decision stated in plain language
- [ ] Numbers present where possible (cost savings %, latency ms, tokens saved)

**Security/Compliance check:**

- [ ] Threat model addressed if the document touches auth, secrets, or data
- [ ] PII handling stated explicitly if the feature handles user input
- [ ] Incident response path referenced if the document describes a production feature

**DevOps/SRE check:**

- [ ] Runbook steps numbered and copy-paste-ready
- [ ] Commands wrapped in fenced code blocks with language tags
- [ ] Rollback procedure present if the document describes a deployable change

---

## Writing tone

For this repo specifically:

- **Direct, not tutorial-style** — the audience is not a student following a guide; they are a practitioner evaluating a practitioner.
- **Tradeoffs explicit** — name the alternative rejected. Documents that only describe the chosen path read as incomplete.
- **Numbers over adjectives** — "reduces latency by 40%" not "significantly faster."
- **Compression over completeness** — a document that covers 80% of the important points in 300 words is more useful than a document that covers 100% in 3,000 words.
- **First-person for reflection artifacts** — `03-architect-thinking.md` is written in first person. Standards and ADRs are written in the impersonal present tense ("The gateway validates..." not "I validated...").

---

## Relationship to the portfolio site

The portfolio site (see `portfolio-strategy.md`) renders these `.md` files directly. Documents written to this standard will render correctly and read well across all audience types. Documents written only for the author will look like internal notes when rendered publicly.

Write every document as if a CTO and a senior engineer are reading it at the same time.
