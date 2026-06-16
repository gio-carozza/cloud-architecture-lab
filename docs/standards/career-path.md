# Career Path: AI Engineer → Forward-Deployed Engineer → LLM Architect

## Where You Are Now

**Current phase: AI Engineer.** Current day: see `docs/notes/_index.md`.
Phase 1 target completion: ~Day 20.

---

## The Three-Phase Progression

### Phase 1: AI Engineer (Days 1–20)

#### If you're 10 years old

An AI Engineer is like a chef who learns to use a really powerful new kitchen tool —
a robot that can write recipes, answer questions, and help cook. Your job is to plug
that robot into the restaurant and make sure the food it helps create is actually good.
You learn what the robot can do, where it makes mistakes, and how to give it the right
instructions so it doesn't burn the soup.

#### If you're a CEO

An AI Engineer is the person who makes AI work inside your product. They know which AI
model to call, how to write prompts that produce reliable output, and how to wire the
model into your existing application without breaking everything. The ROI of a strong
AI Engineer: faster feature velocity on AI-powered products, lower hallucination rates,
and a team that can ship AI features without calling a consultant.

#### If you're an Engineer

An AI Engineer integrates LLM APIs (Anthropic, OpenAI, Azure AI) into production systems.
Core skills: prompt engineering (system prompts, few-shot examples, output structuring),
API integration (streaming, tool use, function calling, batch APIs), context window
management, token cost optimization, and debugging model behavior. You understand the
difference between a 200 OK with a hallucination and a real error. You know how to
measure model quality — not just "does it run" but "does it produce correct output."
Stack: any language + Anthropic/OpenAI SDK + structured logging + basic eval framework.

#### If you're an Architect

The AI Engineer phase is where you build intuition about model behavior before designing
systems around it. You cannot architect an AI platform you haven't debugged at the API
level. Key milestones: successfully integrated one LLM provider with a provider
abstraction seam (so you can swap it later), implemented prompt caching, instrumented
token usage as a metric, and built at least one async/batch processing path. You
understand the difference between a model's capability ceiling and a prompt engineering
problem. This phase ends when you can build reliably, not just experimentally.

**Phase 1 exit criteria:**

- [x] Provider abstraction in place (IChatModelProvider or equivalent) — Day 5, ADR-005
- [x] Prompt caching implemented and verified with token metrics — Day 7, ADR-009
- [x] Batch API path implemented with cost controls — Day 8, ADR-010
- [x] Observability: structured logs with correlation IDs, token usage per request — Day 6, ADR-006
- [ ] Can explain any AI behavior at 10yo, CEO, Engineer, and Architect level (ongoing — assessed at phase close)
- [ ] Multi-turn context management
- [ ] Eval framework basics

---

### Phase 2: Forward-Deployed Engineer (Days 21–50)

#### If you're 10 years old

A Forward-Deployed Engineer is like a helper who goes to different businesses and builds
them their own special robot assistant. One week you're at a hospital helping doctors,
the next week you're at a bank helping customers. You have to be really good at listening
— because each place has different problems, and you can't just build the same thing
everywhere. You also have to explain what you built to the boss, the workers, and the
customers — all in different ways that make sense to each of them.

#### If you're a CEO

A Forward-Deployed Engineer is your most valuable AI hire for enterprise sales.
They sit with your customer, understand the actual business problem (not the stated
technical requirement), build a working prototype in days — not months — and iterate
until the customer says "this is exactly what we needed." They're the people Palantir,
Anthropic, and Scale AI send to close 7-figure deals. The skill is rare: most engineers
can build; few can sell through building. ROI: shorter sales cycles, higher contract
values, faster time-to-value for customers.

#### If you're an Engineer

Forward-Deployed Engineering means scoping and shipping AI solutions in days, not sprints.
Key skills: rapid prototyping (working demo in 48 hours), solution scoping (what's the
minimum AI that solves this specific problem?), prompt engineering for domain-specific
tasks (legal, medical, financial), integration with customer's existing data sources,
presenting technical tradeoffs to non-technical stakeholders. You learn to ask "what
decision does this AI need to help the user make?" before writing a single line of code.
You document your solutions so the customer's team can own them after you leave.

#### If you're an Architect

The Forward-Deployed phase builds the skill the AI Engineer phase doesn't teach: connecting
technical decisions to business outcomes. Every architectural choice becomes a story —
"we chose provider abstraction because if Anthropic raises prices, you can switch to Azure
OpenAI without rewriting your application." The Forward-Deployed engineer learns to
communicate at CEO, CFO, and CTO levels simultaneously. Architecturally, this phase
focuses on: RAG patterns (connecting AI to customer data), agent frameworks (multi-step
reasoning for real workflows), evaluation frameworks (how do you prove the AI is working?),
and responsible AI practices (what happens when it's wrong?). The phase ends when you can
take any business problem, scope the AI solution, build it, and present the results to a
boardroom.

**Phase 2 exit criteria:**

- [ ] Built and deployed at least one RAG-based feature (AI connected to real data)
- [ ] Implemented at least one agent (multi-step, tool-using workflow)
- [ ] Can present any technical decision as a business case (ROI, risk, alternatives)
- [ ] Implemented an evaluation framework (not just "does it run" — "is it correct?")
- [ ] Responsible AI: content filtering, audit logging, PII handling

---

### Phase 3: LLM Architect (Days 51+)

#### If you're 10 years old

An LLM Architect is like the person who designs the whole AI factory, not just one
machine. Instead of building one robot for one restaurant, you design the system that
lets every restaurant in the country have their own robot — and makes sure they're all
safe, not too expensive, and can be upgraded when better robots come out. You think about
what happens when a million people use the system at once, and you make sure nobody
accidentally breaks it or spends too much money.

#### If you're a CEO

The LLM Architect is the person who turns AI experiments into enterprise infrastructure.
They design the platform that lets your entire company — every team, every product —
use AI safely and economically. They implement cost governance (you don't get a surprise
$2M cloud bill), security and compliance (you don't violate GDPR or HIPAA), and
multi-provider strategy (you're not locked into one vendor's pricing). They make AI a
competitive moat, not a technical liability. This is the $200k–$250k role because
the wrong design at this layer can cost tens of millions.

#### If you're an Engineer

LLM Architecture means designing systems that serve thousands of AI requests per minute
across multiple providers, with full observability, cost governance, and resilience.
Core skills: multi-provider abstraction (route between Anthropic, Azure OpenAI, Bedrock
based on cost/latency/capability), distributed tracing across the full AI call chain,
semantic caching (avoid paying for the same inference twice), vector database design
for RAG at scale, model evaluation pipelines (automated quality regression detection),
and enterprise security patterns (tenant isolation, audit logging, PII redaction).
Stack: API gateway + provider SDK abstraction + vector DB + observability platform +
IaC (Bicep/Terraform).

#### If you're an Architect

The LLM Architect phase is where AI engineering meets enterprise software architecture.
The problems shift from "does this work?" to "does this work for 10,000 users at 3am
when Anthropic has a partial outage and the compliance team needs an audit trail?"
Key design decisions: provider abstraction contract (how do you swap models without
breaking callers?), cost attribution (which team, which product, which request class
is spending what?), governance (who can deploy a new model, and what approval workflow
do they need?), and resilience (circuit breakers, retry budgets, fallback providers).
The LLM Architect doesn't just build the gateway — they own the policy: model selection
criteria, cost ceilings per workload class, incident response for AI failures, and
the vendor negotiation position enabled by multi-provider portability.

**Phase 3 entry criteria (same as Phase 2 exit).**
Phase 3 is continuous — you're always an architect once you cross the threshold.
The work shifts from building new capabilities to governing and scaling existing ones.

---

## Skills Matrix

| Skill | AI Engineer | Forward-Deployed | LLM Architect |
|---|---|---|---|
| LLM API integration | ✅ Core | ✅ Applied | ✅ Governed |
| Prompt engineering | ✅ Core | ✅ Domain-specific | ✅ At scale |
| Provider abstraction | ✅ Built | ✅ Explained to customers | ✅ Governed across teams |
| Observability | ✅ Implemented | ✅ Demoed to customers | ✅ Platform-level |
| Cost controls | ✅ Per-feature | ✅ Per-customer ROI | ✅ Enterprise budget |
| RAG | 🔄 Learning | ✅ Customer data | ✅ Platform-scale |
| Agents | 🔄 Learning | ✅ Workflow automation | ✅ Governed agent platform |
| Business communication | ❌ Not required | ✅ Core | ✅ Executive level |
| Compliance / security | ❌ Basic | ✅ Customer requirements | ✅ Enterprise policy |
| Multi-provider routing | ❌ Single provider | ❌ Not required | ✅ Core |
| Model evaluation | ❌ Manual | ✅ Per-deployment | ✅ Automated pipeline |

---

## Certification Mapping

| Exam | Primary Phase | Why |
|---|---|---|
| AI-102 (Azure AI Engineer) | retired | Retired June 30, 2026 — superseded by AI-103 |
| AI-103 (Azure AI Apps and Agents Developer) | Phase 2 | Tests Foundry, agents, RAG, Azure AI Search, eval, responsible AI — maps directly to Phase 2 workload |
| AZ-305 (Solutions Architect) | Phase 2–3 | Tests architecture decisions, infrastructure design, governance |
| AZ-104 (Administrator) | Phase 2–3 | Operations knowledge needed for Forward-Deployed work and platform ownership |
| AZ-900 (Fundamentals) | Phase 1 | Foundation — complete early |

---

## Phase Awareness: How to Know Where You Are

**You're still in Phase 1 if:**

- You're still discovering how the API behaves in edge cases
- Your "why" for a decision is "because the docs said so"
- You can explain what you built to another engineer but not to a business stakeholder
- You haven't yet had to defend a cost tradeoff

**You're ready for Phase 2 when:**

- You can predict how a model will behave before running it
- You can scope a solution ("this is a 3-day build, not a 3-month project") before writing code
- You instinctively ask "whose problem does this solve?" before starting
- You've built something someone outside engineering actually uses

**You're in Phase 3 when:**

- You're making decisions that affect multiple teams or products, not just your own feature
- You're thinking about "what happens when this scales 100x?"
- You're writing governance policies, not just following them
- You're designing for unknown future consumers, not a specific current customer

---

## Daily Rituals by Phase

### Phase 1 (AI Engineer)

- Every `architect-thinking.md`: explain the technical decision and the alternative you rejected
- Posture check question 3: what model behavior surprised you today?
- Cert reinforcement: AZ-900 domains (AI-102 retired June 30, 2026; AI-103 sequenced to Phase 2)

### Phase 2 (Forward-Deployed)

- Every `architect-thinking.md`: add a "CEO Framing" section — how would you present this to a CTO?
- Posture check question 1: name the specific human role whose problem you solved
- Daily: write one sentence explaining today's work to a non-technical stakeholder
- Cert reinforcement: AI-103 + AZ-305 domains

### Phase 3 (LLM Architect)

- Every `architect-thinking.md`: add a "Governance Implication" section — what policy does this decision require?
- Posture check question 2: what would you refuse to ship and why?
- Daily: identify one design decision that affects more than your own code
- Cert reinforcement: AZ-305 + AZ-104 domains

---

## The 200-Day Roadmap

### Phase 1: AI Engineer — Days 001–020 (~6 weeks)

Goal: Build the gateway engine. Own every API behavior.
Establish the seams everything else plugs into.

Days 001–008 — COMPLETE: Provider abstraction, observability, resilience, prompt caching, batch API

| Day | Focus |
|---|---|
| Day 009 | SSE Streaming |
| Day 010 | Multi-turn context management |
| Day 011 | Token budget enforcement + cost controls |
| Day 012 | Database connection (Azure SQL) — **UNLOCK: platform build can begin** |
| Day 013 | Structured output + function calling |
| Day 014 | Content filtering + Responsible AI guardrails |
| Day 015 | Eval framework basics |
| Day 016 | OpenAI provider (second `IChatModelProvider`) — **UNLOCK: multi-provider gateway is real** |
| Day 017 | Provider routing logic |
| Day 018 | Key Vault integration + secrets governance |
| Day 019 | Rate limiting + tenant isolation foundations |
| Day 020 | Phase 1 capstone + AZ-900 completion target |

Certifications: AZ-900 completes Day 020. AI-102/AI-103 in parallel.

### Phase 2: Forward-Deployed Engineer — Days 021–050 (~10 weeks)

Goal: Connect AI to real data. Build things non-engineers use.
Learn to communicate at CEO level.

| Day | Focus |
|---|---|
| Day 021 | Azure AI Search setup + vector indexing |
| Day 022 | RAG pipeline: embed → search → augment → respond |
| Day 023 | RAG in the gateway: `/api/ai/chat/rag` endpoint |
| Day 024 | Multi-turn with memory (conversation context store) |
| Day 025 | Agent foundations: tool use + function calling |
| Day 026 | Agent: case routing agent (first real agent) |
| Day 027 | Agent: research agent (multi-step reasoning) |
| Day 028 | Evaluation framework: automated quality scoring |
| Day 029 | Evaluation: regression detection pipeline |
| Day 030 | Bedrock provider (third `IChatModelProvider`) — **UNLOCK: three providers, real routing decisions** |
| Day 031 | Foundry provider (fourth `IChatModelProvider`) |
| Day 032 | Multi-provider routing: cost vs latency vs capability |
| Day 033 | Semantic caching |
| Day 034 | PII detection + redaction pipeline |
| Day 035 | Audit logging: every AI call traceable to tenant + user |
| Day 036 | Multi-tenant cost attribution |
| Day 037 | Tenant isolation: data boundaries in the gateway |
| Day 038 | SLA management: latency budgets per tenant tier |
| Day 039 | Business case framing (FDE skill) |
| Day 040 | Phase 2 capstone: RAG + agents + multi-provider live |
| Day 050 | Market Track begins: Write technical post 1 |

**Phase 2 exit criteria (ALL required):**

- RAG endpoint live and returning grounded responses
- At least one agent running a multi-step workflow
- Multi-provider routing live (at least Claude + OpenAI)
- At least one non-engineer has used something built here
- At least one technical decision presented as a business case to a non-technical audience

AZ-104 begins ~Day 035. AI-102/AI-103 targets completion ~Day 035.

### Phase 3: LLM Architect — Days 051–100 (~16 weeks)

Goal: Enterprise governance. Platform-scale design. Multi-tenant SaaS hardening.

**Days 051–060 — Governance Foundations**

| Day | Focus |
|---|---|
| Day 051 | Cost governance dashboard |
| Day 052 | Model evaluation pipeline |
| Day 053 | Compliance framework (GDPR, HIPAA considerations) |
| Day 054 | Circuit breaker + fallback provider patterns |
| Day 055 | Incident response playbook for AI failures |
| Day 056 | Canary deployments for model updates |
| Day 057 | A/B testing for prompt variants |
| Day 058 | Semantic versioning for prompts (prompt registry) |
| Day 059 | Token budget enforcement at platform scale |
| Day 060 | Governance capstone: policy document + enforcement |

**Days 061–070 — Platform Infrastructure**

| Day | Focus |
|---|---|
| Day 061 | Azure AD B2C / custom identity foundations |
| Day 062 | RBAC implementation: roles, claims, feature flags |
| Day 063 | Multi-application identity federation |
| Day 064 | Unified logging: one sink for all sites |
| Day 065 | Cross-site correlation IDs |
| Day 066 | Platform-wide alerting strategy |
| Day 067 | IaC: Bicep for the full platform |
| Day 068 | CI/CD pipeline for all sites |
| Day 069 | Blue/green deployment patterns |
| Day 070 | AZ-104 completion target |

**Days 071–085 — Security / Identity Site Build**

| Day | Focus |
|---|---|
| Day 071 | Database schema + API scaffold |
| Day 072 | User management CRUD |
| Day 073 | Role and permission management |
| Day 074 | Application registration |
| Day 075 | Token issuance + validation |
| Day 076 | AI-augmented policy suggestions (via gateway) |
| Day 077 | Audit log UI |
| Day 078 | Multi-tenant isolation |
| Day 079 | UI build (first real frontend) |
| Day 080 | Deployed + tested end-to-end |

**Days 075–100 — Market Track (parallel)**

| Day | Focus |
|---|---|
| Day 075 | Write technical post 2 |
| Day 086 | Platform architecture review |
| Day 087 | SaaS cost model and pricing |
| Day 088 | Compliance audit simulation |
| Day 089 | Disaster recovery and failover testing |
| Day 090 | Performance benchmarking at simulated scale |
| Day 091 | AZ-305 completion target |
| Day 092 | Portfolio documentation |
| Day 093 | Present platform to a fictional CTO (documented) |
| Day 094 | Gap analysis: what does a Fortune 500 still need? |
| Days 095–100 | Capstone hardening + Phase 3 close |

AZ-305 begins ~Day 070. Target completion Day 091.

### Platform Build — Days 101–200 (~34 weeks)

Goal: Build the three sites. Ship a real SaaS product. Get a paying client.

**Days 101–130 — Case Management Application**

| Day | Focus |
|---|---|
| Day 101 | Database schema (cases, users, roles, tenants) |
| Day 102 | Case API: submit, retrieve, update, close |
| Day 103 | Case routing: manual + AI-suggested |
| Day 104 | Case states and workflow engine |
| Day 105 | File attachments + document handling |
| Day 106 | RAG integration: related cases + resolutions |
| Day 107 | Agent integration: auto-classification + escalation |
| Day 108 | Notification system (email, in-app) |
| Day 109 | SLA tracking and breach alerting |
| Day 110 | Case reporting and analytics |
| Days 111–120 | Case Management UI: list, detail, create, search; real-time streaming AI summaries; role-based feature visibility; tenant-branded experience |
| Days 121–125 | Integration testing |
| Days 126–130 | Deployed to Azure, end-to-end tested |

Market milestones: Day 100 — Write technical post 3. Day 120 — Identify 1–2 beta clients.

**Days 131–160 — Admin Site Build**

| Day | Focus |
|---|---|
| Day 131 | Admin schema (categories, types, flows) |
| Day 132 | Category and subcategory management API |
| Day 133 | Case type and workflow configuration API |
| Day 134 | Tenant onboarding flow |
| Day 135 | SLA configuration per case type |
| Day 136 | AI-assisted setup suggestions (via gateway) |
| Day 137 | Workflow builder UI |
| Days 138–145 | Admin Site UI |
| Days 146–150 | Deployed + integrated with Case Management |
| Day 150 | At least 1 non-engineer using the platform |
| Days 151–160 | Integration hardening across all three sites |

**Days 161–185 — SaaS Hardening**

| Day | Focus |
|---|---|
| Day 161 | Multi-tenant onboarding automation |
| Day 162 | Subscription tier enforcement |
| Day 163 | Tenant data isolation audit |
| Day 164 | GDPR / compliance review pass |
| Day 165 | Performance testing at multi-tenant scale |
| Day 166 | Billing signal instrumentation |
| Day 167 | Usage dashboard per tenant |
| Day 168 | Support tooling |
| Day 169 | Disaster recovery test |
| Day 170 | Security penetration testing simulation |
| Days 171–180 | UI polish across all three sites |
| Day 175 | At least 1 paying or committed enterprise client |
| Days 181–185 | End-to-end SaaS scenario with a fictional company |

**Days 186–200 — Portfolio + Launch Prep**

| Day | Focus |
|---|---|
| Day 186 | Full system architecture documentation |
| Day 187 | Enterprise client onboarding runbook |
| Day 188 | Sales deck (what it does, who it's for, pricing) |
| Day 189 | Technical blog post: the gateway architecture decisions |
| Day 190 | Case study: law firm vs. insurer use case |
| Day 191 | Forward-Deployed demo script (live demo in 20 minutes) |
| Day 192 | LLM Architect interview prep (defend every ADR) |
| Day 193 | Open source decision |
| Day 194 | Domain, branding, landing page |
| Day 195 | Beta deployment — platform is live and accessible |
| Days 196–200 | Final hardening, documentation, Day 200 capstone |

---

## Certification Timeline

| Exam | Start | Target | Purpose |
|---|---|---|---|
| AZ-900 | Day 001 | Day 020 | Foundation |
| AI-102/103 | Day 001 | Day 035 | AI integration |
| AZ-104 | Day 035 | Day 070 | Administrator |
| AZ-305 | Day 070 | Day 091 | Solutions Architect |

---

## Applied Deliverable: The Platform

Every training concept maps to a platform layer:

| Concept | Platform Application |
|---|---|
| `IChatModelProvider` abstraction | Every AI call in every app |
| OpenAI / Bedrock / Foundry | Provider failover, cost routing |
| Streaming | Real-time case summaries in UI |
| Prompt caching | Cost reduction across all tenants |
| Batch API | Bulk AI processing for reports |
| RAG | Related case retrieval, resolutions |
| Agents | Automated case routing, escalation |
| Multi-tenant governance | Per-tenant cost, model tier, audit |
| Unified logging | One KQL query across the platform |
| RBAC / identity | Every application's auth layer |
| Cost attribution | Per-tenant subscription billing signal |
| Provider routing | Cheapest model that solves the problem |
| Compliance / audit | Defensible in a Fortune 500 review |

Certifications prove the knowledge. The platform proves the application.

---

## What Gets You to $250k–$500k+

Technical execution alone reaches $180k–$220k Senior AI Engineer.
The following additions are required for the $250k–$500k tier:

1. **CEO Framing from Day 009** — every decision translated to dollars and business risk, not just technical correctness
2. **Real user validation** — at least one non-engineer using something you built and finding it valuable (required for FDE)
3. **Thought leadership** — three public technical posts demonstrating LLM Architect-level thinking to an audience beyond your network
4. **Revenue signal** — at least one paying or committed enterprise client using the platform by Day 175
