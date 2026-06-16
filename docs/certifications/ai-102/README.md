# AI-102 — Azure AI Engineer Associate

> ⚠️ **Retired June 30, 2026.** This folder is a historical record. Successor: **AI-103** (Azure AI Apps and Agents Developer Associate) — Phase 2, ~Day 21+. Do not schedule AI-102.

## Why this cert first (alongside build)

AI-102 is the most directly applicable cert to the AI Gateway work. Every
observability, prompt management, and Azure OpenAI integration topic in the
exam maps to a build day. Studying it in parallel with the roadmap is the
highest-leverage cert path.

> **Verify before scheduling:** Microsoft refreshes the AI-102 objective domain
> periodically. Always check the official skills-measured PDF and exam page on
> learn.microsoft.com before final prep, especially if any retirement or
> revision was announced.

## Status

- [ ] Skills-measured PDF downloaded → `objectives.md` populated
- [ ] Free Microsoft Learn path completed
- [ ] First practice exam attempted (baseline score)
- [ ] Weak-area drill complete
- [ ] Exam scheduled
- [ ] Exam passed

## Domain Map (high-level — verify against current PDF)

Domain weights and exact wording shift with revisions. The structure has
historically been:

1. **Plan and manage an Azure AI solution** (~15–20%)
2. **Implement decision support solutions** (~10–15%)
3. **Implement computer vision solutions** (~15–20%)
4. **Implement natural language processing solutions** (~30–35%)
5. **Implement knowledge mining and document intelligence** (~10–15%)
6. **Implement generative AI solutions** (~10–15%)

The generative AI domain is where build-day work most directly converts.

## Build → Cert Reinforcement

| Day | Topic | Domain Touched |
|---|---|---|
| 005 | Provider abstraction, secrets handling | Plan and manage |
| 006 | App Insights, telemetry, monitoring | Plan and manage; Generative AI |
| TBD | Azure OpenAI as second provider | Generative AI (Primary) |
| TBD | RAG with Azure AI Search | Knowledge mining (Primary) |
| TBD | Content safety / responsible AI | Plan and manage |

## Study Cadence (suggested)

- **Daily (15 min):** Read one Microsoft Learn module section
- **Weekly (1 hr):** One labbed scenario tied to roadmap work
- **Bi-weekly (1 hr):** Practice questions on covered domains
- **Pre-exam week:** Two full practice exams + weak-area drill

## Resources

- Microsoft Learn: AI-102 learning path (free, official)
- Skills-measured PDF: from the AI-102 page on learn.microsoft.com
- Practice: MeasureUp (paid, official), Tutorials Dojo, Whizlabs
- Microsoft Documentation: Azure OpenAI, AI Search, Content Safety, Document Intelligence

## Files in this folder

- `README.md` (this file)
- `objectives.md` — full skills-measured outline with checkboxes
- `study-notes/` — per-domain notes
- `practice/` — question banks, explanations
- `labs/` — hands-on labs
