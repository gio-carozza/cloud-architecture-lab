# Competitive Intelligence — AI Engineering Landscape

**Phase:** all phases — update at each phase transition
**Last updated:** 2026-06-15 (grounded in state-of-field through mid-2025)
**Applies to:** skill prioritization, ADR reasoning, portfolio positioning

---

## What separates $300k–$500k+ practitioners

These are the skills that appear at the top of compensation surveys and in the hiring criteria of Anthropic, Palantir, OpenAI, and enterprise AI platform teams. They are listed in rough order of rarity.

1. **Eval-driven development** — writing evaluations before writing prompts. Engineers who ship without evals are guessing. This is the LLM equivalent of TDD and currently the sharpest credibility signal in the field.

2. **Cost fluency as a design constraint** — not a finance exercise. Knowing token pricing, cache hit math, batch vs. real-time tradeoffs, and being able to defend every architectural decision with a cost model. This gateway already demonstrates this (ADR-009, ADR-010).

3. **Protocol-level knowledge** — MCP (Model Context Protocol, Anthropic, Nov 2024) and A2A (Agent-to-Agent, Google, Apr 2025) are 2025's TCP/IP for AI. Practitioners who understand the wire format and security model design interoperable systems that others build on top of.

4. **Own the failure modes** — hallucination mitigation (structural: grounding, citations, verifiers), agent loops, context poisoning, tool call explosions. The $300k practitioner is who the team calls when the system does something inexplicable.

5. **Communication compression** — explaining a multi-agent architecture to a CFO in 90 seconds and then going four levels deeper with the platform team in the same meeting. Currently the rarest skill.

6. **Security and compliance literacy** — EU AI Act Article 9–14 risk classification, SOC 2 Type II for AI systems, PII detection and redaction in the prompt pipeline.

---

## AI Engineering: field state as of mid-2025

### What is now table stakes (no longer differentiating)

- RAG with a vector DB (everyone has this; the art is in chunking strategy and eval quality, not the retrieval call)
- Basic prompt chaining / LangChain-style sequential chains
- Single-model chatbot over enterprise data
- PDF ingestion + Q&A

### What is actively differentiating (Phase 1–2 scope)

**LLM evaluation as a production system** — evals are not a notebook exercise. The pattern is offline eval + production shadow eval + regression gate on every deploy. Tools: `Braintrust`, `LangSmith`, `PromptFoo`, `RAGAS`. This gateway's `eval-framework.md` is the foundation.

**Structured output reliability** — JSON mode is table stakes. The elite pattern is constrained decoding with typed schemas (`System.Text.Json` + `JsonSchema` in .NET, or `Instructor` in Python). Know the failure modes: schema drift, token budget truncation, model refusal on tight schemas.

**Semantic caching** — cache-aware routing: hit the semantic cache before touching the model. `GPTCache`, Azure Cache for Redis with vector search, `pgvector`. Reduces cost 30–60% on repeated queries.

**AI Observability beyond basic telemetry** — token-level tracing (input/output histograms), TTFT histograms for streaming, prompt version tracking, cost attribution per user/feature/tenant. Tools: `Langfuse`, `Helicone`, `OpenLLMetry` (OTel extension for LLMs), `Arize Phoenix`. This gateway has the OTel foundation; Phase 2 adds `Langfuse` for eval tracing.

**Guardrails as a pipeline stage** — not bolted on. `Guardrails AI`, `NeMo Guardrails`, Azure Content Safety API wired into the gateway seam before and after model calls. Phase 2 scope; foundation in `responsible-ai.md`.

**Model routing and cost-aware dispatch** — routing by capability tier (fast/cheap vs. slow/capable), by context length, by task type. Classify the query, route to cheapest model that can handle it, fall back up the tier. This gateway's provider abstraction (`IChatModelProvider`) is the routing seam.

### What is emerging (12-month horizon — start now)

**Prompt caching as a first-class budget line** — already implemented (ADR-009). Will be required knowledge for any cost-conscious gateway by 2026.

**Continuous online eval** — LLM-as-judge running in production on a sampled real-traffic stream, feeding back into the eval pipeline. `Braintrust` and `Langfuse` both moving toward this.

**Speculative decoding and token budget control** — Claude's `max_tokens` and `thinking` budget controls. Trades latency for cost at the infrastructure level.

---

## Forward-Deployed Engineering: field state as of mid-2025

### What is now table stakes

- Basic chatbot on top of any frontier model
- PDF ingestion + Q&A demo

### What is actively differentiating (Phase 2 scope)

**Rapid prototype-to-demo loop under 48 hours** — measured from problem statement to working demo. Stack: hosted model + thin gateway + `Streamlit`/`Gradio` frontend + pre-wired enterprise connectors. This repo is the "thin gateway" layer.

**Business case framing on demand** — FDEs write the ROI doc, not just the tech spec. Pattern: "This feature saves X hours/week at Y FTE cost = $Z/year. Implementation cost is A days. Payback in B months." Know this formula cold.

**Enterprise data connector fluency** — SharePoint Graph API, Salesforce SOQL, SAP OData, Snowflake SQL. Pattern recognition, not deep expertise. FDEs know which connector applies and can wire it in a day.

**Retrieval quality over retrieval quantity** — owning the "why is the answer wrong" conversation with the client. Chunking strategies (semantic vs. fixed-size), reranking (`Cohere Rerank`, `FlashRank`), hybrid search (BM25 + vector).

**Pilot-to-production handoff playbook** — documented runbook, eval suite, cost projection that the client team can own. FDEs who can't hand off create dependency; handoff-ready FDEs command premium.

### Emerging (12-18 month horizon)

**Voice-first enterprise AI** — OpenAI Realtime API and Azure Speech with LLM integration moving FDE prototypes into voice.

**Computer use / browser automation** — Claude's computer use, Operator-style agents navigating enterprise ERP workflows. FDEs who can demo "the AI navigates your system" will command premium rates.

---

## LLM Architecture: field state as of mid-2025

### Protocols (must know deeply by Phase 3)

**Model Context Protocol (MCP)** — Anthropic's open standard (Nov 2024, rapidly adopted through 2025) for exposing tools, resources, and prompts to models. Architects who can design an MCP server topology for an enterprise — capability placement, security boundaries, versioning — are 12 months ahead of the field.

**Agent-to-Agent (A2A) protocol** — Google's protocol (Apr 2025) for agent delegation across organizational boundaries. MCP = model-to-tool. A2A = agent-to-agent. Designing systems where agents delegate to sub-agents across boundaries requires understanding where these protocols compose vs. conflict.

### Architecture patterns

**Multi-agent orchestration topologies:**

- `Supervisor` — one orchestrator delegates to specialized sub-agents; good for coordination
- `Swarm` — agents communicate peer-to-peer; good for parallelism
- `Hierarchical` — nested supervisors; required for enterprise governance at scale

Frameworks: `AutoGen` (Microsoft), `CrewAI`, `LangGraph`, Anthropic agent SDK

**RAG architecture — second generation:**

- `GraphRAG` (Microsoft, 2024) — graph-structured knowledge base, better for multi-hop
- Multi-hop retrieval — iterative refinement before final answer
- Agentic RAG — the model decides when to retrieve, not the pipeline

**Compound AI systems** (Berkeley LMSYS framing) — the unit of deployment is not a model call but a compound system: retriever + ranker + model + verifier + memory. Architects who design at this abstraction level are 12–18 months ahead.

### Governance (Phase 3 core)

- Tenant-level cost tracking and per-model budget caps
- Audit logging for compliance: EU AI Act, SOC 2 Type II
- Multi-provider routing matrix: task type × cost × latency × data residency × compliance tier
- Azure OpenAI (data residency), Anthropic Claude 4 family, Amazon Bedrock, Azure AI Foundry

### Agent memory architectures (emerging)

`Mem0`, `Zep` — structured long-term memory stores that persist across agent sessions. Will be foundational for enterprise agents by 2026.

### Tools/protocols to know by name

`MCP`, `A2A`, `AutoGen`, `CrewAI`, `LangGraph`, `GraphRAG`, `Mem0`, `Zep`, Azure AI Foundry, Amazon Bedrock, `Semantic Kernel` (Microsoft's enterprise agent framework)

---

## How to stay ahead

1. **Build the thing before you read about it** — implementation reveals what the docs omit. This repo is the vehicle.

2. **Protocol-first learning** — when MCP or A2A ship a new feature, read the spec, not just a blog post about the spec.

3. **Eval everything you build** — the `eval-framework.md` standard exists for this reason. Don't skip it.

4. **Watch the cost math change** — token prices drop ~40%/year historically. An architecture that is cost-optimal today may not be in 12 months. Cache hit rates and batch routing decisions need to be re-evaluated quarterly.

5. **Surface the failure modes** — every new capability has a failure mode that most practitioners don't know yet. Being the person who documents it first is a credibility multiplier.
