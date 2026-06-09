# Collaboration Map

## How to read this

- Reading **DOWN** a collaborator block shows how the relationship evolves across phases.
- Reading **ACROSS** a phase row shows the whole active network at one career moment.
- Where a phase cell says "(absent)" or "(minor)", that emptiness is itself signal —
  that collaborator is not in your loop at that phase; acting as if they are is a category error.
- Phase labels map to `docs/standards/career-path.md`:
  AI Engineer = Phase 1, Forward-Deployed = Phase 2, LLM Architect = Phase 3.

Living standard — reviewed at phase transitions, not regenerated daily. The DAILY
touch is the lens (see `.claude/skills/collaboration-lens`), not this file.

---

## Security / AppSec / CISO

| Phase | Their problem | My posture | Crucial question | Failure mode |
|---|---|---|---|---|
| AI Engineer | Is the gateway leaking secrets or stack traces? | Occasional reviewer; I self-audit via pillars-audit because they're not in the room yet | "What would a stack trace in a 500 cost us?" | I run security as a solo checklist and miss what a real reviewer catches |
| Forward-Deployed | Will this clear the CUSTOMER's security review? | Gatekeeper-handler; surface their veto week one, not week six | "What would make you say no, and how early can you tell me?" | Late review = retrofit under load = blown timeline |
| LLM Architect | Can one tenant reach another through my platform? | Co-designer; isolation designed WITH them, not reviewed after | "Worst thing one tenant can do to another, and where's the boundary?" | Blast radius assumed by convention instead of proven |

---

## Product Manager

| Phase | Their problem | My posture | Crucial question | Failure mode |
|---|---|---|---|---|
| AI Engineer | (mostly absent — minor in P1) | n/a | — | — |
| Forward-Deployed | Did we scope the smallest thing that closes the deal? | Scoping partner; the unbuilt list is a shared deliverable | "Is this a 3-day build or 3-month project, and what did we agree NOT to build?" | "MVP" hides four silent months |
| LLM Architect | Does the roadmap match platform reality? | Constraint-setter; I tell them what's safe to promise | "What are you about to promise customers that the platform can't yet hold?" | Roadmap writes checks the architecture can't cash |

---

## Phase 1 Primary — Inward Collaborators

---

## Backend / Platform Engineers

| Phase | Their problem | My posture | Crucial question | Failure mode |
|---|---|---|---|---|
| AI Engineer | Shared APIs and services stay stable while you iterate fast | Peer contributor — integrate cleanly; flag breaking changes upstream before building on top | "What interfaces can I depend on not changing under me?" | You build against an unstable internal API; it shifts; you blame them — but you never asked what was stable |
| Forward-Deployed | (minor — they stay home while you're deployed outward) | Handoff-writer — leave clean, documentable code they can own after you leave | "Can they maintain this without me?" | Prototype becomes an orphan because you optimised for demo speed, not maintainability |
| LLM Architect | Developer experience on the platform you designed — their productivity is your platform metric | API contract owner — their friction is your signal; fix the platform, don't ask them to work around it | "What are they hacking around that should just work?" | Platform is technically correct but ergonomically hostile; teams build around it instead of on it |

---

## DevOps / SRE

| Phase | Their problem | My posture | Crucial question | Failure mode |
|---|---|---|---|---|
| AI Engineer | Production stays stable when new AI features ship | Good citizen — don't cause incidents; brief new failure modes at deploy time, not after an incident | "What does your on-call hate about AI services at 3am?" | You deploy without telling SRE what new failure modes you introduced; they discover it in an incident, not a briefing |
| Forward-Deployed | (minor — customer environments have their own ops teams) | Lightweight consumer of their patterns — ask what's already in place before building your own | "What's the minimum safe way to deploy in this customer's environment?" | You skip the customer's ops constraints; demo deployment stresses their prod; you leave a mess |
| LLM Architect | Platform failure contracts are designed so runbooks can exist for them | Incident co-designer — I define the failure modes; they write the runbooks; we agree before deploy | "What platform failure would make your runbook unexecutable?" | Platform introduces failure modes with no detection path; on-call discovers them by customer complaint, not alert |

---

## Frontend / Product Engineers

| Phase | Their problem | My posture | Crucial question | Failure mode |
|---|---|---|---|---|
| AI Engineer | (minor — API consumers; interaction is occasional) | API provider — document the contract, error shapes, and latency characteristics | "What latency and error model does your frontend expect?" | Frontend makes assumptions about streaming or error behavior you never documented; both teams discover the mismatch in integration testing |
| Forward-Deployed | The demo UX needs a backend that fits the user flow they've designed | Rapid collaborator — align on the UX interaction model before building the backend, not after | "What does the user actually do at each step, and what does the backend need to return?" | Backend is technically correct but doesn't fit the interaction model; retrofit costs half the remaining timeline |
| LLM Architect | Calling AI platform APIs should feel like any other internal service, not a bespoke integration | Contract owner — developer experience is a platform KPI; friction is a platform defect | "What makes calling our AI platform feel like a death march to your team?" | Platform is powerful but undocumented; frontend teams reinvent integration patterns on every new project |

---

## Data Engineers

| Phase | Their problem | My posture | Crucial question | Failure mode |
|---|---|---|---|---|
| AI Engineer | (absent — RAG and data pipelines are out of scope in P1) | n/a | — | — |
| Forward-Deployed | Data pipelines produce data your AI queries; they need to know what shapes and freshness you require | Consumer-partner — understand the data before promising the customer what the AI can do with it | "What's in this data that I should not put in a prompt?" | You build a RAG system on stale, noisy, or PII-laden data because you never read the schema or asked what the pipeline actually produces |
| LLM Architect | Vector index freshness SLAs depend on pipeline health they own | Policy partner — I set the freshness SLA; they build to it; we agree before it enters the product contract | "What latency exists between a source data change and when my AI can query it?" | Platform quotes freshness SLAs the pipeline can't hit; user-visible staleness is blamed on AI quality when the real problem is ingestion lag |

---

## Eng Manager / Tech Lead

| Phase | Their problem | My posture | Crucial question | Failure mode |
|---|---|---|---|---|
| AI Engineer | Team velocity — your blockers are their blockers; surprises hurt the sprint | Proactive communicator — report blockers early; don't surface them at sprint end | "What would make you pull me off this work mid-sprint?" | You go heads-down for two weeks; priority shifts on day three; you deliver something that's no longer needed |
| Forward-Deployed | Commitments to the customer are grounded in delivery reality | Scope negotiator — push back when "MVP" hides months of invisible scope; share that risk before the commitment is made | "Have you made a customer commitment that requires scope we haven't defined?" | Manager commits to a timeline based on their estimate of complexity; you discover the real complexity mid-engagement with no buffer |
| LLM Architect | Platform governance is enforced even when high-priority teams want to bypass it | Policy author needing executive cover — governance without enforcement authority is a blog post | "Will you enforce platform standards when a shipping team wants to bypass them?" | Governance is real on paper, optional in practice; exceptions become the rule; the platform fragments |

---

## Cloud & Model-Vendor Support (external)

| Phase | Their problem | My posture | Crucial question | Failure mode |
|---|---|---|---|---|
| AI Engineer | Support cases are solvable if the customer can reproduce them | Informed questioner — arrive with repro steps, error codes, and a hypothesis, not "it doesn't work" | "Is this behavior documented, a known bug, or a product limitation?" | You escalate vague tickets; vendor support can't help; you burn days on something a clear repro would have solved in hours |
| Forward-Deployed | Customer-specific quota, rate limits, and deployment timelines affect the engagement | Bridge — translate between the customer's requirements and the vendor's current constraints | "What's the realistic timeline for this quota increase or capability gap?" | You promise a customer a vendor capability that requires approval; approval takes three weeks; demo is in five days |
| LLM Architect | Vendor roadmap alignment determines your platform's forward compatibility | Architect-level interlocutor — vendor knows your workload shape; you know enough of their roadmap to plan against | "What are you planning to deprecate or change in the next 12 months?" | You lock a platform contract to a model version the vendor is deprecating; migration lands in production at the worst possible moment |

---

## Phase 2 Primary — Outward Collaborators

---

## Account Executive / Sales Engineer

| Phase | Their problem | My posture | Crucial question | Failure mode |
|---|---|---|---|---|
| AI Engineer | (absent — not relevant in P1) | n/a | — | — |
| Forward-Deployed | The technical demo closes the deal; the engagement timeline validates the pitch | Technical closer — scope what's actually shippable and make the demo land without making promises I can't keep | "What did you promise the customer that isn't in my current scope?" | AE closes a deal on assumptions you never validated; you're accountable for a gap that was never yours to create |
| LLM Architect | Platform capability roadmap must match what's safe to promise at enterprise scale | Constraint-setter — I tell them what the platform can hold today and the timeline for the rest | "What capability gap is costing you deals — and is it a platform gap or a positioning gap?" | Sales cites a capability that requires a platform exception every time; exceptions accumulate into a fragmented non-standard architecture |

---

## Customer Success / Implementation

| Phase | Their problem | My posture | Crucial question | Failure mode |
|---|---|---|---|---|
| AI Engineer | (absent — n/a in P1) | n/a | — | — |
| Forward-Deployed | They own adoption and support after you leave; if they can't run it, the handoff failed | Clean handoff partner — document the runbook, not just the code | "What do you need from me to own this without calling me back?" | Handoff is the repo; there's no failure playbook, no architecture explanation; CSM calls you back for every incident |
| LLM Architect | Customer adoption blockers are usually platform ergonomics, not missing features | Signal receiver — their escalations are my platform defect backlog | "What are customers trying to do that should be easy but isn't?" | Platform evolves on internal engineering opinions; CS feedback is treated as noise; real adoption blockers stay in the product for months |

---

## Customer Executive Sponsor (external)

| Phase | Their problem | My posture | Crucial question | Failure mode |
|---|---|---|---|---|
| AI Engineer | (absent — not relevant in P1) | n/a | — | — |
| Forward-Deployed | Internal political protection for the project; they need to defend it upstairs | Storyteller — translate technical progress into business progress they can defend without me in the room | "What does success look like to you personally — not the project, you?" | I brief the champion but not the sponsor; project gets cancelled at a budget review the sponsor could have protected against |
| LLM Architect | (minor — platform decisions are handled at procurement/legal level; sponsor is the escalation path only) | n/a unless contract escalation | — | — |

---

## Customer Domain Experts (external)

| Phase | Their problem | My posture | Crucial question | Failure mode |
|---|---|---|---|---|
| AI Engineer | (absent — domain expertise not required in P1) | n/a | — | — |
| Forward-Deployed | AI output accuracy in their domain can only be judged by them; I need their validation before I claim the system works | Student-first, builder-second — understand the domain before deciding what to automate | "What does the AI getting this wrong cost you, specifically?" | I build a system producing plausible-looking domain output; I never validate with an expert; it ships and damages trust on the first real error |
| LLM Architect | (minor — platform architecture doesn't require deep domain expertise unless designing evaluation pipelines) | n/a unless scoping domain accuracy evaluation | — | — |

---

## Customer End Users (external)

| Phase | Their problem | My posture | Crucial question | Failure mode |
|---|---|---|---|---|
| AI Engineer | (absent — not in scope in P1) | n/a | — | — |
| Forward-Deployed | They use the product, not the spec; their actual behavior reveals the assumptions you got wrong | Observer — watch them use the demo before finalising the build | "What did they do that I never predicted, and what assumption does that invalidate?" | I build based on the champion's description of users; I never watch real users; the product solves the problem the champion thinks users have, not the one they actually have |
| LLM Architect | (absent at P3 design level — handled via CS and product feedback loops) | n/a | — | — |

---

## Customer IT / Security / Data Owners (external)

| Phase | Their problem | My posture | Crucial question | Failure mode |
|---|---|---|---|---|
| AI Engineer | (absent — not relevant in P1) | n/a | — | — |
| Forward-Deployed | Data classification, residency, and security review are their mandate; they need time to do it properly | Pre-emptive briefer — surface security and data questions in week one, not before go-live | "What data can I legally put in a prompt, and what requires special handling?" | I demo a RAG system before IT security reviews; production blocked over a data residency issue that was always a non-starter |
| LLM Architect | Platform governance artifacts — data residency guarantees, SOC 2 controls, PII handling — are what they need to approve enterprise deployment | Governance producer — I build the evidence trail they need to say yes | "What compliance documentation do you need before you can approve this at scale?" | Engineering considers security "handled" after an internal AppSec review; customer IT rejects the platform because they need SOC 2 Type II, not a review memo |

---

## The Champion (external)

| Phase | Their problem | My posture | Crucial question | Failure mode |
|---|---|---|---|---|
| AI Engineer | (absent — not relevant in P1) | n/a | — | — |
| Forward-Deployed | They're selling the project internally while I'm building it; they need technical wins to defend it without me in the room | Keep them armed — technical progress must translate into the internal argument they're making upstairs | "What is the internal case you're making, and does what I'm building support that argument?" | Champion runs out of arguments when the sponsor asks ROI questions; I never gave them quantitative evidence they could use without me |
| LLM Architect | (minor — enterprise governance and procurement replace single-champion dynamics at P3) | n/a | — | — |

---

## Phase 3 Primary — Sideways Collaborators

---

## Compliance / Legal / Privacy

| Phase | Their problem | My posture | Crucial question | Failure mode |
|---|---|---|---|---|
| AI Engineer | (minor — not in room yet; basic self-audit via pillars-audit) | Pre-emptive designer — build GDPR/PII safeguards in before they arrive, not after | "What would make you escalate my system design to Legal?" | I log prompt content to App Insights; Legal finds it in Q4 and mandates a retrofit under deadline |
| Forward-Deployed | Customer's legal and compliance team must approve deployment before it goes live | Artifact producer — generate the documentation they need to say yes | "What is the blocking concern, and what specific document would clear it?" | Legal holds go-live for six weeks over a data processing agreement nobody thought to draft until the day before launch |
| LLM Architect | AI governance policy defines the categories of use I'm responsible for enforcing technically | Policy converter — their constraints become platform-enforced guardrails, not documentation-only rules | "Which AI uses are categorically out of scope, and how do I make that boundary technically unbypassable?" | Governance is a Word document that engineers read once; compliance discovers platform usage that violates policy and mandates a shutdown |

---

## FinOps / Finance

| Phase | Their problem | My posture | Crucial question | Failure mode |
|---|---|---|---|---|
| AI Engineer | (minor — cost awareness is self-directed in P1; FinOps not engaged yet) | n/a | — | — |
| Forward-Deployed | The customer ROI case needs specific cost-per-unit numbers, not vague efficiency claims | Metric producer — give them the cost model behind the demo, not just the demo | "What is the ROI metric that will keep this funded for another quarter?" | Engineering delivers a technically excellent prototype with no cost model; Finance can't build the business case; engagement dies in procurement |
| LLM Architect | Enterprise AI spend needs cost attribution by team, workload class, and feature — not just a total bill | Cost architecture designer — I build the attribution model that makes FinOps questions answerable | "Which team or workload is driving AI cost growth, and is the platform designed to answer that question?" | Enterprise AI spend hits the board agenda with no attribution model; FinOps cannot build a chargeback system; platform gets cancelled after the first cost review |

---

## Peer Architects

| Phase | Their problem | My posture | Crucial question | Failure mode |
|---|---|---|---|---|
| AI Engineer | (minor — peer architects review, rarely co-design at this phase) | Learner — read their ADRs, ask questions, don't assume you understand their system's constraints | — | — |
| Forward-Deployed | (minor — forward-deployed work is mostly self-contained; cross-platform coordination is rare) | n/a | — | — |
| LLM Architect | System boundaries between platforms must be agreed, not assumed — both sides need to know where their guarantee ends | Horizontal integrator — I design the seam WITH them, not for them | "Where does your architecture's guarantee end and mine begin — and have we explicitly agreed on that?" | Boundary is assumed from one side; an incident in the seam area triggers mutual blame because neither team has the full context to diagnose or fix it |

---

## Procurement / Vendor Mgmt

| Phase | Their problem | My posture | Crucial question | Failure mode |
|---|---|---|---|---|
| AI Engineer | (absent — not in scope in P1) | n/a | — | — |
| Forward-Deployed | (minor — may appear if the customer engagement involves a vendor agreement that affects the build) | n/a unless a vendor contract term directly changes what I can build | — | — |
| LLM Architect | Multi-vendor contracts require technical qualification data and exit-cost analysis that only the architect can produce | Technical briefer — give them what they need to negotiate from a position of knowledge | "What is our exit cost if this vendor doubles prices — and can the platform actually execute the switch?" | Multi-provider portability is an engineering talking point; procurement never gets the technical analysis they need; vendor negotiation happens without leverage |

---

## VP Eng / CTO

| Phase | Their problem | My posture | Crucial question | Failure mode |
|---|---|---|---|---|
| AI Engineer | (minor — not in daily loop; awareness of your work is indirect) | n/a | — | — |
| Forward-Deployed | Portfolio-level understanding of how the AI engagement fits the broader engineering strategy | Storyteller — lead with business impact; have an answer ready for "how does this become a product?" | "What does success at the portfolio level look like — not just this engagement?" | You deliver a technically excellent demo; they ask how it becomes a product; you don't have the answer; the engagement stalls |
| LLM Architect | Platform evolution requires executive sponsorship — the most important refactors never get prioritised without it | Investment case maker — translate architecture decisions into risk and ROI they can act on | "What platform technical debt is creating the most business risk right now?" | Critical refactors never reach the executive layer as business risk; they accumulate as tech debt nobody has authority to prioritise |

---

## External Auditors (external)

| Phase | Their problem | My posture | Crucial question | Failure mode |
|---|---|---|---|---|
| AI Engineer | (absent — not in scope in P1) | n/a | — | — |
| Forward-Deployed | (absent unless the customer is in a heavily regulated industry requiring audit documentation) | n/a | — | — |
| LLM Architect | Compliance controls must be evidenced, not just designed — they verify what you claim | Evidence architect — design audit trails and structured logging specifically to answer the questions they will ask | "What evidence do you require for each AI control, and is our platform producing it in a form you can actually verify?" | Platform passes internal review but fails external audit because operational logs are structured for incident response, not audit evidence — the data exists but can't answer "who approved this AI use case and when" |
