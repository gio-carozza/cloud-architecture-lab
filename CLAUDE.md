Before starting ANY work, read `.claude/instructions/daily-workflow.md` (overview + rules), then the relevant step in `.claude/instructions/daily-workflow-steps.md`.

# cloud-architecture-lab

Three-phase career progression: AI Engineer → Forward-Deployed Engineer → LLM Architect.
Anchor workload is a .NET 8 AI Gateway deployed to Azure App Service,
designed for multi-provider LLM routing. Currently Claude-backed;
provider-abstracted for future Azure OpenAI, Bedrock, and Foundry.
Full career path: `docs/standards/career-path.md`

## Owner & Context

- Backend developer (.NET, APIs, SQL), MIS master's
- **Current phase: AI Engineer** — building the gateway, learning the APIs, owning cost and observability
- **Next phase: Forward-Deployed Engineer** (~Day 21) — applying AI to real business problems, rapid prototyping, CEO-level communication
- **Target phase: LLM Architect** (~Day 051+) — enterprise governance, multi-provider, compliance, $250k–$500k+
- Build-first learner; wants architect-level reasoning, not tutorials
- Current day: see `docs/notes/_index.md`

## Stack

- .NET 8 Web API (namespace: Lab.Observability.Api)
- Azure App Service (Linux, East US)
- Anthropic Claude (current LLM provider; abstracted for swap-out)
- Azure CLI + PowerShell + Kudu for deploys
- Local tooling: VS Code, Windows, `dotnet user-secrets`

## Azure Environment

Full details: `docs/standards/azure-environment.md` (in hot context — always loaded).

## Naming Convention (summary)

- Globally-unique resources end with `-gio` (App Service, App Insights, Storage, Key Vault, etc.)
- RG-scoped resources may use `-gio` for consistency (App Service Plan, Log Analytics, Action Group)
- Subscription-scoped resources are exempt (Resource Group, Budget)
- Full convention: `docs/standards/naming-conventions.md`

## Commands

- Build:    `dotnet build src/lab-observability-api/lab-observability-api.csproj`
- Run:      `dotnet run --project src/lab-observability-api`
- Publish:  `dotnet publish src/lab-observability-api/lab-observability-api.csproj -c Release -o ./publish`
- Test:     `dotnet test` (runs `lab-observability-api.Tests` — no real API calls)
- Deploy:   `/deploy` slash command (wraps the Kudu zip path)
- New day:  `/new-day <N> <slug>` slash command
- New ADR:  `/adr <kebab-title>` slash command
- Cert scaffold: `/cert-scaffold <EXAM>` (one-time per exam)
- Cert update:   `/cert-update <N>` (end of each day)
- Collab lens:   `/collab-lens <N>` (daily collaborator focus — run after STEP 6 populates 01-summary.md)
- Full daily loop: `.claude/instructions/daily-workflow.md`

## Architecture (top-level map)

- `src/lab-observability-api/`  → .NET 8 Web API (the AI Gateway)
- `docs/adr/`                   → Architecture Decision Records (ADR-NNN-*)
- `docs/architecture/`          → System diagrams & sequence flows
- `docs/notes/Day-NNN/`         → Daily roadmap artifacts (per day)
- `docs/notes/changelog.md`     → Running file-change log, all days, `## Day NNN` sections
- `docs/certifications/`        → Cert prep: AZ-900, AZ-104, AZ-305, AI-102 (retired), AI-103 (Phase 2)
- `docs/standards/`             → All standards (see index below)
- `Infra/Day-NNN/`              → IaC, app settings templates
- `.claude/skills/`             → Reusable knowledge packs (auto-invoked)
- `.claude/commands/`           → Slash commands for repetitive workflows

## Standards index (`docs/standards/`)

| File | Phase | What it governs |
|---|---|---|
| `_principles.md` | All | Architect posture, 5 traits, daily questions, phase awareness (hot context) |
| `graveyard.md` | All | Running log of experiments, failures, and lessons — add at STEP 12 |
| `career-path.md` | All | Three-phase progression, cert mapping, exit criteria (read on demand) |
| `naming-conventions.md` | All | Azure resource naming rules |
| `azure-environment.md` | All | Live resource names, region, subscription |
| `collaboration-map.md` | All | Collaborator matrix per phase |
| `kql-cookbook.md` | All | Telemetry query patterns |
| `architect-thinking-template.md` | All | Required structure for `03-architect-thinking.md` each day |
| `competitive-intelligence.md` | All | 2025 field state: AI Eng, FDE, LLM Arch — what differentiates at $300k+ |
| `public-documentation.md` | All | Multi-audience writing standard for portfolio-visible documents |
| `portfolio-strategy.md` | All | Public site plan, LinkedIn strategy, go-public checklist |
| `testing-standard.md` | 1 | What must be tested, unit vs. integration, naming, coverage floor |
| `commit-convention.md` | 1 | Type/scope/subject rules, day-close commit sequence |
| `cost-governance.md` | 1 | Token budgets, caching requirements, batch vs. real-time rules |
| `error-handling-standard.md` | 1 | Error taxonomy, classification rules, retry policy, logging |
| `provider-onboarding.md` | 1 | Step-by-step playbook for adding a new LLM provider |
| `multi-turn-context.md` | 1 | Context window budget, truncation strategy, history contract, caching rules |
| `eval-framework.md` | 1/2 | Evaluation dimensions, LLM-as-judge, golden test set, regression gates |
| `monitoring-runbook.md` | 1 | Alert inventory, daily monitoring check, incident response P1–P4 |
| `responsible-ai.md` | 2 | Content filtering, PII, audit logging, bias, incident response |
| `security-standard.md` | 2 | Input validation, secrets, OWASP checklist, threat model |
| `slo-performance.md` | 2 | Latency targets, availability SLO, alert rules, review cadence |
| `api-versioning.md` | 2 | Breaking vs. non-breaking, deprecation policy, Swagger rules |
| `dependency-policy.md` | 2 | When to add packages, evaluation checklist, update cadence |
| `rag-patterns.md` | 2 stub | RAG architecture, chunking strategy, Azure AI Search integration plan |
| `agent-patterns.md` | 2 stub | Agent topologies, tool design, failure modes, MCP/A2A protocols |

## Conventions (DO NOT violate without an ADR)

- Day folders: `Day-NNN` (3-digit zero-padded; supports >100 days)
- Inside day folders: numeric reading-order prefix, NO day prefix (e.g., `01-summary.md`, `02-completion-checklist.md`)
- In shared folders: KEEP day prefix (e.g., `day-006-observability.md`)
- ADRs: `ADR-NNN-kebab-case-title.md`. Never edit accepted ADRs — supersede instead.
  Exception: factual corrections to implementation notes (broken file paths, renamed files, typos) are exempt — the decision itself must not change.
- Namespaces: `Lab.Observability.Api.*`
- Secrets:
  - Local: `dotnet user-secrets`
  - Azure: App Service environment variables (double underscore: `Anthropic__ApiKey`)
  - NEVER commit secrets, NEVER put them in `appsettings.json`
- **`docs/notes/changelog.md` — running file change audit log:**
  One file for the whole project, with a `## Day NNN` section per day (replaced
  the per-day `Day-NNN/07-files-changed.md` files — 2026-06-16; that convention
  forced a "which day owns this file" lookup before logging any cross-cutting
  fix, which scattered rows across closed historical folders). Whenever docs are
  updated in response to any step completion ("update all necessary docs", "update docs",
  "update the checklist", or equivalent), upsert a row under the **current day's**
  section — the day the edit actually happened, not necessarily the day that
  originally created the file. **Dedup key is the file path within that day's
  section** — if the file already has a row for that path under the same
  heading, update it in place; never add a duplicate row for the same day. The
  same file may legitimately appear under multiple different day headings (it
  was touched on more than one day) — that's not a duplicate. Format:

  ```markdown
  ## Day NNN

  | File | Step | Change |
  |---|---|---|
  | `path/to/file.ext` | verification | what changed and why |
  ```

  The `Step` column names which workflow step triggered the change
  (`scaffold`, `build`, `verification`, `deploy`, `docs pass`, etc.).
  Update this file as the last action of every doc-update pass.
- **Markdown formatting in `.md` files:**
  - Wrap all type names, method names, interface names, and CLI commands in backticks (e.g., `IChatModelProvider`, `StreamAsync`, `dotnet build`).
  - Use fenced code blocks with a language tag — never bare fences (e.g., ` ```csharp `, ` ```bash `, ` ```json `).
  - Do not use bare HTML tags in prose — angle-bracket generics like `IAsyncEnumerable<T>` must be inside backticks.
  - Line length is not enforced (MD013 disabled via `.markdownlint.json`); other markdownlint rules apply.

## Provider Abstraction Contract (do not break)

- `IChatModelProvider` — the seam. All LLM calls go through this.
  - `SendAsync(ChatRequest, CancellationToken)` — buffered response (added Day 5)
  - `StreamAsync(ChatRequest, CancellationToken)` — SSE streaming, returns `IAsyncEnumerable<ChatChunk>` (added Day 9, ADR-011); default implementation degrades to a single terminal `ChatChunk` for non-streaming providers
- `ClaudeChatModelProvider` — current implementation (both methods).
- `ChatRequest` / `ChatResponse` — provider-agnostic buffered contracts.
- `ChatChunk` — provider-agnostic streaming delta: text delta, nullable stop reason, nullable end-of-stream `ChatChunkUsage`. No Anthropic SSE types.
- `AnthropicOptions` — bound via `IOptions<T>` from config section "Anthropic".
- New providers (Azure OpenAI, Bedrock, Foundry) must implement `IChatModelProvider`
  without leaking provider-specific types into `ChatRequest`/`ChatResponse`/`ChatChunk`.

## Gotchas (things you can't infer from the code)

- **App Service deploy:** use the Kudu zip publish API, NOT `az webapp deploy`.
  The latter hits "remote host forcibly closed" errors on this network.
  See `.claude/skills/azure-deploy/SKILL.md`.
- **WEBSITE_RUN_FROM_PACKAGE=1** must be set BEFORE zip deploy.
- **Zip structure:** files at root, not nested in a `/publish` folder.
- **Azure CLI SSL reset on `management.azure.com`:** TLS inspection on this network
  resets connections during the handshake. Two workarounds:
  1. `az config set core.disable_ssl_certificate_verification=true` — works for most
     read/list commands; restore with `=false` after.
  2. `Invoke-RestMethod` with `az account get-access-token` bearer token — uses
     Windows' native TLS stack; required for PUT/POST writes that the CLI still resets
     even with SSL verification disabled. Preferred for creating Azure resources.
  - **Exception — App Service appsettings PUT/PUT:** `/config/appsettings` PUT fails via
    both paths on this network (Day 7). **Workaround:** PATCH the parent site resource
    instead — `Invoke-RestMethod -Method PATCH` to `.../sites/{name}?api-version=2022-03-01`
    with body `{properties:{siteConfig:{appSettings:[{name,value},...]}}}`. PATCH goes
    through where PUT does not. Include ALL settings in the array — siteConfig.appSettings
    PATCH replaces the entire array, not merges. Portal is the fallback if PATCH also fails.
  - `api.applicationinsights.io` (used by `az monitor app-insights query`) is NOT
    affected — KQL queries work without workarounds.
- **`IOptions<T>`:** requires `using Microsoft.Extensions.Options;` explicitly.
- **Anthropic 401:** check user-secrets binding key — must be `Anthropic:ApiKey`
  locally, `Anthropic__ApiKey` in App Service env vars.
- **Anthropic 400 with vague error:** check account credits FIRST before debugging code.
- **Anthropic prompt caching — TTL required for Claude 4 models:** `{"type":"ephemeral"}`
  without a TTL is silently ignored on Claude 4 models (0 cache tokens, full billing, no error).
  Always use `{"type":"ephemeral","ttl":"1h"}` or `"ttl":"5m"`. Claude 3 models worked without TTL.
- **Anthropic prompt caching — wrong model ID = silent zero:** `claude-opus-4-6` is not a
  valid current model (valid: `claude-opus-4-8`, `claude-sonnet-4-6`, `claude-haiku-4-5-20251001`).
  The API returns 200 with content but `cache_creation_input_tokens: 0` always. Check the model ID
  before debugging cache logic.
- **Anthropic usage response format (Claude 4):** cache creation tokens appear in BOTH the flat
  `cache_creation_input_tokens` field AND a new nested `cache_creation.ephemeral_*_input_tokens`
  object. `TryExtractUsage` handles both via fallback logic (see `ClaudeApiClient.cs`).
- **Stack traces:** never return them in API responses. Production-grade error
  contracts only (correlationId + safe message).

## What I'm Building Toward (north star)

Three-phase career progression — see `docs/standards/career-path.md` for full detail.

### Phase 1: AI Engineer (Days 1–20) — IN PROGRESS

Build the gateway. Learn the APIs. Own cost and observability.

- Provider abstraction (done — Day 5, ADR-005)
- Observability & resilience (done — Day 6, ADR-006, ADR-008)
- Cost controls — prompt caching (done — Day 7, ADR-009); batch API (done — Day 8, ADR-010)
- Streaming responses — SSE on interactive path, TTFT histogram, ADR-011 (done — Day 9)
- **Remaining:** multi-turn context management, eval framework basics

### Phase 2: Forward-Deployed Engineer (Days 21–50)

Apply AI to real business problems. Communicate at CEO level. Rapid prototyping.

- RAG with Azure AI Search (connect AI to real data)
- Agent patterns (multi-step, tool-using workflows)
- Evaluation framework (automated quality measurement)
- Business case framing for every technical decision
- Responsible AI: content filtering, audit logging, PII handling

### Phase 3: LLM Architect (Days 51+)

Enterprise governance. Multi-provider. Compliance at scale.

- Multi-model routing & evaluation pipeline
- Governance: cost attribution, security, compliance
- Multi-provider strategy (Azure OpenAI, Bedrock, Foundry)
- Enterprise agent platform
- Platform-scale RAG

## End Product North Star

### The Architecture in One Sentence

A provider-agnostic AI gateway serving as the AI nervous system for a
multi-tenant enterprise SaaS platform — starting with Security/Identity,
Case Management, and Admin sites — unified by shared logging, shared
authentication, and shared AI consumption governance, designed for
subscription use by any business or industry.

### Layer 1: AI Gateway (cloud-architecture-lab) — THE ENGINE

- Provider-agnostic: Claude (current), OpenAI, Bedrock, Foundry (roadmap)
- Every AI feature in every application routes through this gateway
- No application ever calls a provider SDK directly
- Gateway governs: model selection, cost per tenant per app, rate limits,
  audit logging, provider failover
- Build sequence:
  Streaming (Day 009), database (Day 012), OpenAI (Day 016),
  RAG (Day 022), tool use foundations (Day 025), case routing agent (Day 026),
  multi-agent orchestration (Day 027), Bedrock (Day 030),
  Foundry (Day 031), multi-provider routing (Day 032),
  multi-tenant governance (Day 036), Generative AI governance (Day 051),
  Agent governance / circuit breakers (Day 052),
  Agentic AI workflow governance (Day 053), SaaS hardening (Day 161+)

### Layer 2: Platform Services — SHARED INFRASTRUCTURE

#### Security / Identity Site

- Platform-level service: issues tokens, manages roles and permissions
  for ALL registered applications — current and future
- Any new application registers here and inherits auth/authz
- RBAC with feature-level permission granularity per role per application
- Multi-tenant: each subscribing company has isolated role definitions
- AI features (all via gateway):
  - Generative AI: natural language role configuration
  - AI Agent: access anomaly detection — monitors login patterns, flags
    unusual permission usage, recommends remediation actions
  - Agentic AI: incident response workflow — detect anomaly → assess risk
    → notify admin → apply temporary restriction → log decision
- Designed to scale to N applications beyond the initial three
- Build begins: Day 071

#### Unified Logging

- One Azure Log Analytics workspace for all sites and the gateway
- Consistent correlation IDs across every HTTP request, AI call,
  role change, and case state transition — regardless of originating site
- One KQL query spans gateway telemetry + case events + identity audit
- AI-augmented: anomaly detection, usage summarization (via gateway)
- Foundations built: Day 064–065

### Layer 3: Application Suite — WHAT CUSTOMERS USE

#### Case Management Application

- External customer-facing and internal user-facing case submission/tracking
- Role-based with feature-level permissions (sourced from Security/Identity)
- Industry-agnostic; fully configurable via Admin Site
- AI features (all via gateway):
  - Generative AI: case summaries, suggested responses, draft communications,
    streaming delivery to UI
  - AI Agents: case intake agent (classify + route), resolution drafter agent,
    SLA breach alerting agent
  - Agentic AI: end-to-end case handling workflow — intake → classify → route
    → research → draft → notify → close, with human approval checkpoints
    at configurable stages per tenant
- Schematics: provided by owner when database training is complete (~Day 012)
- Build begins: Day 101

#### Admin Site

- Tenant onboarding: case categories, subcategories, types, workflows, SLAs
- AI features (all via gateway):
  - Generative AI: suggest category structures, workflow descriptions,
    SLA recommendations by industry
  - AI Agent: tenant onboarding agent — asks setup questions, configures
    categories and workflows, validates against existing tenant patterns
  - Agentic AI: multi-step tenant configuration workflow — intake industry
    type → suggest structure → validate → provision → confirm
- Build begins: Day 131

#### Future Applications (N+1, N+2, ...)

- Register with Security/Identity, inherit the full platform stack
- Each is a tenant-configurable module, not a fork

### Layer 4: SaaS Model — HOW IT GOES TO MARKET

- Subscription-based, multi-tenant
- Gateway enforces per-tenant AI cost budgets and model tier entitlements
- Target: at least 1 beta client using the platform by Day 150
- Target: at least 1 paying or committed client by Day 175
- Forward-Deployed angle: owner configures and deploys for enterprise clients
- LLM Architect angle: governs the platform at scale, owns the AI policy

### Career Target

Primary training: LLM Architect
— governance, multi-provider design, enterprise AI systems, compliance

Applied role: Forward-Deployed Engineer
— sit with enterprise clients, configure the platform, close with working software

Compensation target: $250k–$500k+

These are the same person at different moments in the same engagement.

### The Three Fixes (non-negotiable from Day 009 onward)

#### Fix 1: CEO Framing is mandatory from Day 009

Every `architect-thinking.md` from Day 009 forward includes a CEO Framing
section that names a specific dollar amount, a specific risk, or a specific
competitive consequence. Generic statements ("this improves efficiency") are
rejected. Required format:
"A tenant running [N] cases/month at current token prices pays $X without
this feature and $Y with it. That delta is [the business consequence]."
Claude enforces this. Weak or generic CEO Framing gets pushed back on.

#### Fix 2: Market Track (parallel to Build Track, starts Day 050)

- Day 050: Write technical post 1 — Why provider abstraction is non-negotiable for enterprise AI
- Day 075: Write technical post 2 — How multi-tenant AI cost governance works
- Day 100: Write technical post 3 — What a real RAG implementation looks like vs. tutorials
- Day 120: Identify 1–2 beta clients for the SaaS platform
- Day 150: At least 1 non-engineer using the platform in a real context
- Day 175: At least 1 paying or committed enterprise client

#### Fix 3: Real User Validation added to Phase 2 exit criteria

Phase 2 is not complete until:

- At least one non-engineer has used something built here and found it valuable
- At least one technical decision has been presented as a business case
  to a non-technical audience (real or simulated with documented output)

## Certification Tracks (parallel to build work)

| Exam | Phase | Status |
|---|---|---|
| AZ-900 (Foundations) | Phase 1 | In progress |
| AI-102 (AI Engineer Associate) | Phase 1–2 | Retired June 30, 2026 — superseded by AI-103 |
| AI-103 (Azure AI Apps and Agents Developer) | Phase 2 | Beta — sequenced to Phase 2 (~Day 21+) |
| AZ-104 (Administrator) | Phase 2–3 | Starts ~Day 035 — target Day 070 |
| AZ-305 (Solutions Architect Expert) | Phase 2–3 | Starts ~Day 070 — target Day 091 |

Each daily roadmap day MUST include a Certification Reinforcement section
mapping that day's activities to specific cert domains (Primary / Secondary).
Full cert-to-phase mapping: `docs/standards/career-path.md` → Certification Mapping table.

## Working Style

- Direct, structured. No hand-holding on basics.
- Exact file paths, exact commands, copy-paste-ready content.
- Surface tradeoffs explicitly. Name the alternative being rejected.
- **Always explain at all four levels: 10-year-old, CEO, Engineer, Architect.**
  - 10yo: analogy-first, no jargon, one paragraph
  - CEO: business value, ROI, risk — two sentences max
  - Engineer: exact APIs, code patterns, common errors
  - Architect: system design, tradeoffs, enterprise implications
- Build-first; architecture reasoning layered on top.
- Don't repeat completed days. Build on the live system.

## Context Discipline & Model Selection

### Project knowledge scope (hot context)

The project knowledge holds ONLY:

- This file (`CLAUDE.md`)
- `_principles.md` (posture, 5 traits, daily questions, phase awareness — no career descriptions, no graveyard)
- `naming-conventions.md`, `azure-environment.md` (live environment facts)
- The CURRENT day's working artifacts (`01-summary.md`, `02-completion-checklist.md`, `04-posture-check.md`)
- The most recent 1-2 ADRs
- NOT in hot context (read on demand): all `SKILL.md` files (lazy-loaded at skill invocation),
  `career-path.md`, `collaboration-map.md`, `kql-cookbook.md`, `graveyard.md`,
  `.claude/instructions/daily-workflow-steps.md`, `.claude/commands/*`, `.claude/hooks/*`

Everything else lives in the repo and is read by Claude Code on demand.
This is RAG-by-discipline. Treat token cost as a first-class architectural constraint.

### When to use Claude Code vs Chat

- **Claude Code (terminal):** writing/editing code, running commands, debugging, deploys.
  Reads files on-demand. Far more token-efficient for build work.
- **Chat (claude.ai):** architecture reasoning, ADR drafting, daily roadmap planning,
  posture reflection, cert prep. Conversational work where the reasoning is the artifact.

### Work modes (signal which I'm in)

**Build mode** — implementing, deploying, debugging.
Primary tool: Claude Code in terminal. Chat used only for pre-build reasoning.
Output style: short, direct, copy-paste-ready.

**Study mode** — preparing for AZ-900, AZ-104, AZ-305, AI-103.
Primary tool: chat. Cert prep notes pasted in per session.
Output style: all four explanation levels. Include exam-domain mappings.

**Perform mode** — architecture reasoning, ADR drafting, posture checks,
roadmap planning, tradeoff analysis.
Primary tool: chat. This is where the latest Opus (currently 4.8) escalation most often applies.
Output style: structured. Surface tradeoffs explicitly. Name the alternative rejected.

### When to use Sonnet vs the latest Opus

Default to **Sonnet 4.6**. It handles ~80% of roadmap work at a fraction of the quota cost.

Use **the latest Opus (currently 4.8)** only for:

- ADR drafting where the decision logic IS the deliverable
- Architecture tradeoff analysis with non-obvious answers
- Pushback on my own reasoning when I suspect I'm wrong
- Any moment where Sonnet's first answer feels thin and the depth genuinely matters

If unsure, start with Sonnet. Escalate to Opus only when Sonnet's answer doesn't carry
the weight the question deserves. This mirrors the model-routing decision you'll make
in production AI gateways: cheapest model that solves the problem, escalate on need.

The canonical model-routing rule lives in `.claude/instructions/daily-workflow.md`:
Sonnet everywhere except STEP 5 (ADR reasoning) and STEP 11 (posture check),
which use Opus. If this file and the workflow ever disagree, the workflow wins.

### Quick reference: modes × models × tools

| Work mode  | Default model | Primary tool   | Escalate to Opus when |
|------------|---------------|----------------|------------------------|
| Build      | Sonnet 4.6    | Claude Code    | Architectural detour mid-build |
| Study      | Sonnet 4.6    | Chat           | Concept genuinely doesn't click |
| Perform    | Sonnet 4.6    | Chat           | ADR / tradeoff / pushback / depth |

### Lifecycle: pruning the project as days complete

**When a roadmap day completes:**

1. Move that day's `01-summary.md`, `02-completion-checklist.md`, `04-posture-check.md`
   OUT of project knowledge
2. Files remain in the repo at `docs/notes/Day-NNN/` (Claude Code reads on demand)
3. Bring next day's working files IN to project knowledge
4. Project knowledge stays bounded forever, regardless of roadmap length

**When a new ADR is accepted:**

- Keep the most recent 2-4 ADRs in project knowledge
- Older ADRs move out (still in `docs/adr/`, readable by Claude Code)

**When a day-folder file is referenced from later days:**

- That file has graduated from "log" to "standard"
- Promote it to `docs/standards/` (rename if needed)
- Update references in `CLAUDE.md` and any consuming docs

**When `docs/standards/` grows beyond 6-8 files:**

- That's the signal to consider sub-foldering
- Don't pre-optimize; let it accrete first

**When a phase transition occurs (Day 20, Day 50):**

- Review `docs/standards/collaboration-map.md` — update any phase rows that no longer match lived reality
- Update the `**Current phase:**` line in `CLAUDE.md` before the first day of the new phase
- The `/collab-lens` skill reads `**Current phase:**` as its phase filter; stale phase = wrong collaborator selected

## Certification Tooling

- `/cert-scaffold <exam>` — run once per exam; `/cert-update <N>` — run at end of each day (STEP 10)
- Coverage matrix: `docs/certifications/domain-coverage.md`; do NOT reproduce paid exam bank content

## Collaboration Tooling

- `/collab-lens <N>` — run in STEP 6 after 01-summary.md is populated; inserts a bounded
  `### Collaboration Lens` block into 01-summary.md under "Whose Problem Am I Solving?"
- Skill procedure: `.claude/skills/collaboration-lens/SKILL.md`
- Reference map: `docs/standards/collaboration-map.md` — one block per collaborator, one row per phase
- Output cap: 1 PRIMARY (posture + question + 4-level compression) + at most 2 secondary = ≤ 12 lines
- DAILY touch is the lens; the map is reviewed at phase transitions only
