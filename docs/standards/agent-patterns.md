# Agent Patterns Standard

**Phase:** 2 stub — reference now, implement starting Phase 2
**Applies to:** any multi-step, tool-using, or delegating AI workflow

---

## What is an agent

An agent is an LLM that can take actions — calling tools, querying data, writing files, calling APIs — and iterate over a sequence of steps to complete a goal. Unlike a single `SendAsync` call that produces one response, an agent loop produces a sequence of model calls where each step can observe the results of the previous step.

The gateway's `IChatModelProvider` is the seam that agents will call. Agent orchestration lives above the gateway, not inside it.

---

## Agent topologies

### Single agent with tools

The simplest topology. One LLM, a set of tool definitions, and a loop:

```text
while not done:
    response = model.call(system_prompt, history, tool_definitions)
    if response.has_tool_call:
        result = execute_tool(response.tool_call)
        history.append(tool_result)
    else:
        return response.text
```

Use for: single-user tasks, well-defined tool sets, tasks that fit in one model's context.

### Supervisor + sub-agents

One orchestrator agent that classifies a task and delegates to specialized sub-agents:

```text
Supervisor → [Research agent]
           → [Writing agent]
           → [Code agent]
           → [Verification agent]
```

Use for: tasks requiring different capabilities, tasks too large for one agent's context, when different steps need different models (cost optimization).

### Swarm (peer-to-peer delegation)

Agents pass tasks to each other based on capability. No central orchestrator.

Use for: highly parallel tasks, when the task structure is unknown upfront. Higher complexity; use only when supervisor pattern is insufficient.

### Hierarchical

Nested supervisors for enterprise-scale workflows:

```text
Top-level supervisor → Department supervisor A → Agent A1, Agent A2
                     → Department supervisor B → Agent B1, Agent B2
```

Use for: enterprise governance, multi-team workflows, compliance-gated tasks.

---

## Tool design principles

Tools are functions the model can call. Design them for agents, not for humans.

| Principle | Rule |
|---|---|
| **Idempotent** | Calling a tool twice with the same input must be safe. Agents loop; tools will be called multiple times. |
| **Typed schemas** | Use JSON Schema with strict types. Vague schemas cause model confusion and unexpected inputs. |
| **Single responsibility** | One tool does one thing. Compound tools are harder to reason about and harder to test. |
| **Observable** | Every tool call logs: tool name, input, output, duration, error. Agents that fail silently are impossible to debug. |
| **Bounded** | Tools must have timeouts and output size limits. An agent waiting on a hung tool is a stuck agent. |
| **Human-in-the-loop gate** | Irreversible actions (send email, write to database, make purchase) must have an approval step. See `responsible-ai.md`. |

---

## Failure modes and guardrails

Every agent system will encounter these. Design for them upfront.

| Failure mode | Description | Guardrail |
|---|---|---|
| Infinite loop | Agent cycles between two states without making progress | Max step limit (hard cap: 20 steps); detect repeated tool calls |
| Tool call explosion | Agent calls the same tool hundreds of times | Per-tool call budget; circuit breaker on repetition |
| Context exhaustion | History fills the context window, agent loses track of the goal | Summarize history at defined intervals; store goal separately |
| Hallucinated tool calls | Agent invents tool names that don't exist | Strict tool schema validation; return structured error, not free text |
| Prompt injection via tool output | Malicious tool output contains instructions to the model | Sanitize tool outputs before injecting into history |
| Runaway cost | Uncapped agent burns budget in one session | Per-session token budget enforced by gateway; alert on overage |

---

## Protocols (Phase 3 depth)

**Model Context Protocol (MCP)** — Anthropic's standard for exposing capabilities (tools, resources, prompts) to models. Instead of hard-coding tool definitions per agent, MCP servers expose them dynamically. A gateway with an MCP layer can plug new tools in without redeploying.

**Agent-to-Agent (A2A)** — Google's protocol (Apr 2025) for agents to delegate tasks to other agents across organizational boundaries. An agent can discover another agent's capability card and send it a task without knowing its implementation.

These protocols are Phase 3 depth but the architecture decisions made in Phase 2 should not foreclose them. Design tool interfaces as if they will eventually be exposed via MCP.

---

## Frameworks (evaluate in Phase 2)

| Framework | Style | Best for |
|---|---|---|
| `Semantic Kernel` | .NET-native, Microsoft | Integrates with this gateway's .NET stack |
| `AutoGen` | Python, Microsoft | Multi-agent research + enterprise orchestration |
| `LangGraph` | Python | Stateful agent graphs with explicit control flow |
| `CrewAI` | Python | Role-based multi-agent teams |
| Anthropic Agent SDK | Any | Direct Anthropic API; matches this gateway's provider |

For this repo: evaluate `Semantic Kernel` first (native .NET) before introducing Python-based frameworks. ADR required before adding any agent framework.

---

## Observability requirements for agents

Single model calls are one span. Agent runs are a trace:

```text
[agent.run] (root span)
    ├── [agent.step.1]
    │       ├── [claude.chat.api] (model call)
    │       └── [tool.call.search] (tool execution)
    ├── [agent.step.2]
    │       ├── [claude.chat.api]
    │       └── [tool.call.write_file]
    └── [agent.step.3]
            └── [claude.chat.api] (final response)
```

Add to telemetry (Phase 2):

| Metric | Name |
|---|---|
| Steps per agent run | `ai.agent.steps_total` (histogram) |
| Tool calls per run | `ai.agent.tool_calls_total` (histogram) |
| Token spend per run | reuse existing token metrics, tag with `agent.run_id` |
| Agent error rate | `ai.agent.errors_total` (counter, tag with error type) |

---

## ADR requirement

Adding agent orchestration is an ADR-worthy decision. The ADR must address:

- Framework selection (`Semantic Kernel` vs. custom vs. other)
- Tool registry design (hard-coded vs. MCP)
- Human-in-the-loop checkpoint placement
- Per-run budget enforcement
- How agent runs are traced end-to-end

---

> Governing standard for AI Agent and Agentic AI implementation
> across the platform. All agents route through the gateway.
> No site calls a provider SDK directly.

## The Three Paradigms

See `docs/standards/career-path.md` → AI Paradigm Map for full definitions.
This document covers implementation patterns and governance rules.

## Pattern 1: Single-Turn Generative (foundation)

Request → IChatModelProvider.SendAsync / StreamAsync → Completion

No tool use. No loop. One request, one response.
Governed by: model tier entitlement, token budget, safety guardrails.
First used: Day 001.

## Pattern 2: Single-Agent Tool Use (AI Agent)

Request → Model reasons → Selects tool → Gateway executes tool → Result returned to model → Model reasons → Final response

One agent, one task, one or more tool calls, bounded loop.
Governed by: tool authorization matrix, loop iteration limit,
cost per tool call, full action audit log.
First used: Day 025 (foundations), Day 026 (case routing agent).

Implementation rules:

- Tool schemas defined in the gateway, not in the site
- Tool authorization checked at gateway before execution
- Every tool call logged with: agent id, tenant id, tool name,
  input hash, output hash, latency, token cost
- Loop limit: default 10 iterations; configurable per tenant per agent
- On limit reached: return partial result with explicit truncation signal,
  never silently drop work

## Pattern 3: Multi-Agent Orchestration (Agentic AI)

User request → Orchestrator agent
→ Subtask A → Specialist agent A (tool use)
→ Subtask B → Specialist agent B (tool use)
→ Orchestrator synthesizes → Final response

One orchestrator, N specialist agents, one correlation ID.
Governed by: agent identity boundaries, handoff contracts,
per-workflow cost attribution, failure mode definitions.
First used: Day 027 (research workflow).

Implementation rules:

- Orchestrator and specialists are separate registered identities
  in the gateway — not the same agent calling itself recursively
- Handoff contracts are versioned structs, not free-form strings
- One correlation ID is assigned at workflow entry and propagated
  to every agent call, every tool call, every log entry
- Agent A cannot invoke tools authorized only for agent B —
  authorization is per-agent, not per-workflow
- Failure modes must be defined before the workflow is deployed:
  - Tool timeout → retry once → escalate to human review
  - Specialist failure → orchestrator logs and continues with
    available results, flags gap in output
  - Loop limit reached → return partial workflow result with
    explicit gap annotation, never a silent empty response
  - Tenant budget exceeded mid-workflow → pause workflow,
    notify tenant admin, await resume or cancellation

## Human-in-the-Loop Checkpoints

Agentic workflows in the case management site support configurable
human approval checkpoints. Defined per tenant per workflow stage.

Checkpoint types:

- REVIEW: human sees the agent's proposed action before it executes
- NOTIFY: human is informed after the action executes
- OVERRIDE: human can cancel or redirect before next stage

Checkpoint configuration lives in the Admin Site.
Checkpoint state is stored in the case management database.
Gateway logs checkpoint events with the same correlation ID
as the surrounding workflow.

## Cost Attribution Rules

| Paradigm | Attribution Unit | Logged Field |
|---|---|---|
| Generative AI | Per request | `llm.tokens.input`, `llm.tokens.output`, `llm.latency_ms` |
| AI Agent | Per tool call + per loop iteration | `llm.agent.tool_calls`, `llm.agent.iterations`, `llm.agent.total_tokens` |
| Agentic AI | Per workflow + per agent within workflow | `llm.workflow.id`, `llm.workflow.agent_id`, `llm.workflow.total_tokens` |

All fields follow the OpenTelemetry naming convention established
in `docs/architecture/observability-architecture.md`.

**Naming inconsistency, not yet reconciled:** the table above uses `llm.*`
prefixes (`llm.tokens.input`, `llm.agent.tool_calls`, `llm.workflow.id`), but
this file's own Observability Requirements section above (and every metric
actually built in `GatewayTelemetry.cs`) uses the `ai.provider.*` / `ai.agent.*`
convention. Reconcile this — pick one prefix — before any of these fields are
implemented at Day 025+; don't ship both namespaces.

## Anti-Patterns (do not do these)

- **Recursive self-calls:** an agent calling itself to simulate
  multi-agent behavior. Use the orchestrator pattern instead.
- **Ad hoc handoffs:** passing free-form strings between agents.
  Use versioned structs with explicit field contracts.
- **Tool authorization at the site level:** sites do not authorize
  tool calls. The gateway does. Always.
- **Silent failure:** an agent that returns an empty response when
  a tool times out. Always return a partial result with an explicit
  gap annotation.
- **Unbounded loops:** no loop limit defined. Every agent loop
  must have an explicit iteration ceiling registered at deploy time.
- **Shared correlation IDs across unrelated workflows:** one
  correlation ID per workflow entry. Never reuse across cases.
