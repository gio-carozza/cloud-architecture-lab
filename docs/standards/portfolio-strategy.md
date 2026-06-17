# Portfolio Strategy — Public Presence and LinkedIn

**Phase:** all phases — start building visibility now, not after Phase 3
**Applies to:** public repo, LinkedIn, personal brand, $250k–$500k+ job targeting

---

## The goal

Make this repo and the AI gateway publicly visible so that hiring managers, CTOs, and peer engineers can observe the progression from backend developer → AI Engineer → Forward-Deployed Engineer → LLM Architect. The repo is the proof of work. The portfolio site is the display case.

---

## What makes this repo portfolio-worthy

Every day of this repo demonstrates something that most engineers at the target compensation level do not document:

- **Architectural decision-making under constraints** — ADRs with explicit tradeoff analysis, alternatives rejected, consequences accepted
- **Cost-conscious engineering** — prompt caching, batch API, model routing decisions with token math
- **Provider abstraction** — a gateway that can swap LLM providers without changing the API contract
- **Observability-first design** — telemetry built in from Day 1, not bolted on after incidents
- **Daily iteration with architect reasoning** — the daily workflow itself is a differentiator

The portfolio site makes these visible to people who will not read the raw repo.

---

## Portfolio website plan

### Tech stack recommendation

**VitePress** (static site generator, Markdown-first, Vue-powered) — reasons:

- Reads `.md` files directly from the repo; no content duplication
- Zero-config syntax highlighting (matches the code-heavy content)
- Fast build; deploys to GitHub Pages or Azure Static Web Apps for free
- Single dev dependency; no React complexity added to a .NET repo

Alternative rejected: **Docusaurus** (React-based, heavier, better for large doc sites). **Blazor WebAssembly** rejected for the portfolio site — wrong tool for a static content site; use Blazor only if interactive server features are needed.

### Deployment target

**Azure Static Web Apps** — free tier, custom domain, global CDN, PR preview deployments. Naming: `stapp-portfolio-gio` (follows repo naming convention, globally unique).

GitHub Actions workflow auto-deploys on push to `main`. Add `docs/portfolio/` as the VitePress source root.

### Site structure

```text
/ (home)
├── /days                    ← day-by-day progress log
│   ├── /001                 ← Day 001 artifacts rendered
│   ├── /002
│   └── ...
├── /architecture            ← system diagrams (from docs/architecture/)
├── /standards               ← standards library (rendered from docs/standards/)
├── /certifications          ← cert coverage matrix, daily domain tags
├── /adrs                    ← all ADRs rendered with status badges
└── /about                   ← career path, contact, LinkedIn
```

### Day progress feature

Each day page renders:

- `01-summary.md` — what was built
- `02-completion-checklist.md` — what was verified
- `03-architect-thinking.md` — the reasoning
- Cert domains touched that day (from daily roadmap cert section)
- Files changed (from the day's section in `docs/notes/changelog.md`)
- ADRs written (auto-detected from `docs/adr/` by date)

This makes the learning journey transparent and shows decision-making velocity, not just output.

### Interactive architecture diagram

Render `docs/architecture/` diagrams using **Mermaid.js** (VitePress supports Mermaid natively). Sequence diagrams for request flow, component diagrams for provider abstraction, timeline diagrams for the three-phase career arc.

---

## Content that converts viewers to contacts

### For engineers (peer credibility)

- The ADR list with status badges (Proposed/Accepted/Superseded) — shows architectural maturity
- The `competitive-intelligence.md` document rendered as a "field notes" section — shows awareness beyond implementation
- Code snippets from key patterns (provider abstraction, streaming, prompt caching) — shows depth
- The test suite (unit + integration tests, coverage floor) — shows discipline

### For CTOs / hiring managers (leadership credibility)

- The career path progression with phase gates — shows long-horizon planning
- CEO framing from each day's `03-architect-thinking.md` — shows business communication
- The cost governance numbers (cache hit rate, batch API savings) — shows cost ownership
- The observability story (TTFT histogram, error rate dashboard) — shows production maturity

### For recruiters (positioning clarity)

- Phase label on every page: "Phase 1: AI Engineer | Phase 2: Forward-Deployed Engineer | Phase 3: LLM Architect"
- Target role and compensation tier stated on the `/about` page — removes ambiguity
- Certification progress bar (AZ-900, AI-103, AZ-104, AZ-305) — signals commitment to credentials

---

## LinkedIn strategy

### What to post (and when)

| Post type | Frequency | Content |
|---|---|---|
| Day milestone | Every 5 days | One architectural insight from the current day's `03-architect-thinking.md`. One sentence CEO framing. Link to day page on portfolio site. |
| ADR summary | When an ADR is accepted | "I decided X instead of Y because Z" in three sentences. Link to ADR on portfolio site. |
| Phase transition | Day 20, Day 50 | Summary of what the phase built, what it unlocked, what's next. |
| Field observation | Weekly | One insight from `competitive-intelligence.md` with a personal observation from implementation. |
| Cert milestone | When a cert is passed | Brief, specific — what the cert required vs. what the build work already covered. |

### What NOT to post

- Daily logs or checklists — too granular for LinkedIn, belongs on the portfolio site
- Code dumps — show code on the portfolio site; LinkedIn is for insight compression
- Self-promotion without a hook — the hook is always "here's what I learned" not "look what I built"

### Profile optimization

- Headline: `AI Gateway Engineer → Forward-Deployed AI Engineer → LLM Architect | Building in public | .NET + Azure + Anthropic`
- About section: link to portfolio site; three-phase arc in two sentences
- Featured: pin the portfolio site as the first featured item
- Skills: `LLM Integration`, `Azure OpenAI`, `Prompt Engineering`, `AI Gateway Architecture`, `System Design`, `.NET`, `Azure App Service`, `Application Insights`

---

## When to go public

**Milestone checklist before making the repo and site public:**

- [ ] Days 1–10 complete (multi-turn context implemented)
- [ ] `README.md` current and audience-appropriate
- [ ] All secrets removed from git history (confirm with `git log -S "sk-ant"`)
- [ ] No stack traces in API responses (verified via `responsible-ai.md` checklist)
- [ ] Portfolio site deployed and mobile-readable
- [ ] LinkedIn profile updated with portfolio link
- [ ] At least one ADR rendered with full content on the site

**Do not wait for Phase 3.** The progression from Phase 1 → Phase 2 is more interesting to hiring managers than a completed Phase 3 — it shows the work in progress, not a polished artifact.

---

## Maintenance

Update `competitive-intelligence.md` at each phase transition (Day 20, Day 50). The field moves faster than this repo; staying ahead requires deliberate updates, not just implementation.

Review this document at Day 20 (Phase 2 start). What was "emerging" in the Phase 1 version should have moved to "differentiating" or "table stakes" by then.
