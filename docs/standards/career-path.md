# Career Path: AI Engineer → Forward-Deployed Engineer → LLM Architect

## Where You Are Now
**Current phase: AI Engineer — Day 8 complete, Day 9 next.**
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
- [ ] Provider abstraction in place (IChatModelProvider or equivalent)
- [ ] Prompt caching implemented and verified with token metrics
- [ ] Batch API path implemented with cost controls
- [ ] Observability: structured logs with correlation IDs, token usage per request
- [ ] Can explain any AI behavior at 10yo, CEO, Engineer, and Architect level

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
| AI-102 (Azure AI Engineer) | Phase 1–2 | Directly tests AI integration, generative AI, cost management |
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
- Cert reinforcement: map every day to AI-102 domains

### Phase 2 (Forward-Deployed)
- Every `architect-thinking.md`: add a "CEO Framing" section — how would you present this to a CTO?
- Posture check question 1: name the specific human role whose problem you solved
- Daily: write one sentence explaining today's work to a non-technical stakeholder
- Cert reinforcement: AI-102 + AZ-305 domains

### Phase 3 (LLM Architect)
- Every `architect-thinking.md`: add a "Governance Implication" section — what policy does this decision require?
- Posture check question 2: what would you refuse to ship and why?
- Daily: identify one design decision that affects more than your own code
- Cert reinforcement: AZ-305 + AZ-104 domains
