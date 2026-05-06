# cloud-architecture-lab

12-month roadmap to Azure Cloud AI / LLM Architect ($200k–$250k tier).
Anchor workload is a .NET 8 AI Gateway deployed to Azure App Service,
designed for multi-provider LLM routing. Currently Claude-backed;
provider-abstracted for future Azure OpenAI, Bedrock, and Foundry.

## Owner & Context
- Backend developer (.NET, APIs, SQL), MIS master's
- Pivoting to Azure Cloud AI / LLM Architect role
- Build-first learner; wants architect-level reasoning, not tutorials
- Days 1–6 complete. Day 7 next.

## Stack
- .NET 8 Web API (namespace: Lab.Observability.Api)
- Azure App Service (Linux, East US)
- Anthropic Claude (current LLM provider; abstracted for swap-out)
- Azure CLI + PowerShell + Kudu for deploys
- Local tooling: VS Code, Windows, dotnet user-secrets

## Azure Environment
- Subscription: gio-architecture-lab
- Resource Group: rg-ai-lab-dev-eastus
- App Service Plan: asp-ai-lab-dev-eastus-gio
- App Service: app-ai-lab-api-dev-eastus-gio
- Application Insights: appi-ai-lab-api-dev-eastus-gio (workspace-based)
- Log Analytics Workspace: law-ai-lab-dev-eastus-gio
- Action Group: ag-ai-lab-dev-eastus-gio (AI gateway alerts → gio.carozza@outlook.com)
- Alert Rule: alert-ai-gateway-5xx-rate-dev-eastus-gio (5xx > 5% / 5 min, severity 2)
- Region: East US

## Naming Convention (summary)
- Globally-unique resources end with `-gio` (App Service, App Insights, Storage, Key Vault, etc.)
- RG-scoped resources may use `-gio` for consistency (App Service Plan, Log Analytics, Action Group)
- Subscription-scoped resources are exempt (Resource Group, Budget)
- Full convention: `docs/standards/naming-conventions.md`

## Commands
- Build:    `dotnet build src/lab-observability-api/lab-observability-api.csproj`
- Run:      `dotnet run --project src/lab-observability-api`
- Publish:  `dotnet publish src/lab-observability-api/lab-observability-api.csproj -c Release -o ./publish`
- Test:     `dotnet test`
- Deploy:   See `.claude/skills/azure-deploy/SKILL.md` (use the Kudu zip path)
- New day:  Use `/new-day` slash command

## Architecture (top-level map)
- `src/lab-observability-api/`  → .NET 8 Web API (the AI Gateway)
- `docs/adr/`                   → Architecture Decision Records (ADR-NNN-*)
- `docs/architecture/`          → System diagrams & sequence flows
- `docs/notes/Day-NNN/`         → Daily roadmap artifacts (per day)
- `docs/certifications/`        → Cert prep: AZ-900, AZ-104, AZ-305, AI-102
- `Infra/Day-NNN/`              → IaC, app settings templates
- `.claude/skills/`             → Reusable knowledge packs (auto-invoked)
- `.claude/commands/`           → Slash commands for repetitive workflows

## Conventions (DO NOT violate without an ADR)
- Day folders: `Day-NNN` (3-digit zero-padded; supports >100 days)
- Inside day folders: NO day prefix on filenames (e.g., `completion-checklist.md`)
- In shared folders: KEEP day prefix (e.g., `day-006-observability.md`)
- ADRs: `ADR-NNN-kebab-case-title.md`. Never edit accepted ADRs — supersede instead.
- Namespaces: `Lab.Observability.Api.*`
- Secrets:
  - Local: `dotnet user-secrets`
  - Azure: App Service environment variables (double underscore: `Anthropic__ApiKey`)
  - NEVER commit secrets, NEVER put them in `appsettings.json`

## Provider Abstraction Contract (do not break)
- `IChatModelProvider` — the seam. All LLM calls go through this.
- `ClaudeChatModelProvider` — current implementation.
- `ChatRequest` / `ChatResponse` — provider-agnostic contracts.
- `AnthropicOptions` — bound via `IOptions<T>` from config section "Anthropic".
- New providers (Azure OpenAI, Bedrock, Foundry) must implement `IChatModelProvider`
  without leaking provider-specific types into `ChatRequest`/`ChatResponse`.

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
  - `api.applicationinsights.io` (used by `az monitor app-insights query`) is NOT
    affected — KQL queries work without workarounds.
- **IOptions<T>:** requires `using Microsoft.Extensions.Options;` explicitly.
- **Anthropic 401:** check user-secrets binding key — must be `Anthropic:ApiKey`
  locally, `Anthropic__ApiKey` in App Service env vars.
- **Anthropic 400 with vague error:** check account credits FIRST before debugging code.
- **Stack traces:** never return them in API responses. Production-grade error
  contracts only (correlationId + safe message).

## What I'm Building Toward (north star)
A multi-provider, observable, governed enterprise AI gateway:
1. Provider abstraction (done — Day 5)
2. Observability & resilience (done — Day 6)
3. Cost controls (prompt caching, batch API)
4. Multi-model routing & evaluation
5. RAG with Azure AI Search
6. Enterprise agent platform
7. Governance: cost, security, compliance

## Certification Tracks (parallel to build work)
- AZ-900: Foundations (foundational concepts surface throughout)
- AZ-104: Administrator (parallel track starting ~Day 10–15)
- AZ-305: Solutions Architect Expert (post AZ-104)
- AI-102: AI Engineer Associate (paced with AI gateway work)
Each daily roadmap day MUST include a Certification Reinforcement section
mapping that day's activities to specific cert domains (Primary / Secondary).

## Working Style
- Direct, structured, architect-level. No hand-holding on basics.
- Exact file paths, exact commands, copy-paste-ready content.
- Surface tradeoffs explicitly. Name the alternative being rejected.
- Always include "why this matters in enterprise" + "what elite architects
  do differently" + "common beginner mistakes."
- Build-first; architecture reasoning layered on top.
- Don't repeat completed days. Build on the live system.

## Context Discipline & Model Selection

### Project knowledge scope (hot context)
The project knowledge holds ONLY:
- This file (CLAUDE.md)
- _principles.md (architect posture)
- naming-conventions.md, azure-environment.md (live environment facts)
- The four SKILL.md files
- The CURRENT day's working artifacts (summary.md, completion-checklist.md, posture-check.md)
- The most recent 1-2 ADRs

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

**Study mode** — preparing for AZ-900, AZ-104, AZ-305, AI-102.
Primary tool: chat. Cert prep notes pasted in per session.
Output style: explanations at 10-year-old AND doctorate level.
Include exam-domain mappings.

**Perform mode** — architecture reasoning, ADR drafting, posture checks,
roadmap planning, tradeoff analysis.
Primary tool: chat. This is where Opus 4.7 escalation most often applies.
Output style: structured, architect-level. Surface tradeoffs explicitly.

### When to use Sonnet 4.6 vs Opus 4.7
Default to **Sonnet 4.6**. It handles ~80% of roadmap work at a fraction of the quota cost.

Use **Opus 4.7** only for:
- ADR drafting where the decision logic IS the deliverable
- Architecture tradeoff analysis with non-obvious answers
- Pushback on my own reasoning when I suspect I'm wrong
- Any moment where Sonnet's first answer feels thin and the depth genuinely matters

If unsure, start with Sonnet. Escalate to Opus only when Sonnet's answer doesn't carry
the weight the question deserves. This mirrors the model-routing decision you'll make
in production AI gateways: cheapest model that solves the problem, escalate on need.

### Quick reference: modes × models × tools

| Work mode  | Default model | Primary tool   | Escalate to Opus when |
|------------|---------------|----------------|------------------------|
| Build      | Sonnet 4.6    | Claude Code    | Architectural detour mid-build |
| Study      | Sonnet 4.6    | Chat           | Concept genuinely doesn't click |
| Perform    | Sonnet 4.6    | Chat           | ADR / tradeoff / pushback / depth |

If unsure: start Sonnet, escalate on need. Mirrors production AI gateway
routing: cheapest model that solves the problem, escalate when warranted.

### Lifecycle: pruning the project as days complete

**When a roadmap day completes:**
1. Move that day's `summary.md`, `completion-checklist.md`, `posture-check.md`
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
- Day-005's `kql.md` → `docs/standards/kql-cookbook.md` is the canonical example

**When `docs/standards/` grows beyond 6-8 files:**
- That's the signal to consider sub-foldering
- Don't pre-optimize; let it accrete first

## Certification Tooling
- `/cert-scaffold <exam>` — run once per exam to build domain structure
- `/cert-update <day>` — run at end of each day session to populate
  domains touched that day (reads summary.md cert section automatically)
- Hook: `.claude/hooks/cert-tag.json` auto-tags domains as you work
- Coverage matrix: `docs/certifications/domain-coverage.md`
- Content: 9yo + architect explanations, synthesized practice questions,
  curated MS Learn + community resource links
- Do NOT reproduce paid exam bank content