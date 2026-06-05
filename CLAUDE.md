Before starting ANY work, follow `.claude/instructions/daily-workflow.md`

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
- **Target phase: LLM Architect** (~Day 51+) — enterprise governance, multi-provider, compliance, $200k–$250k tier
- Build-first learner; wants architect-level reasoning, not tutorials
- Days 1–9 complete. Day 10 next.

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
- Deploy:   `/deploy` slash command (wraps the Kudu zip path)
- New day:  `/new-day <N> <slug>` slash command
- New ADR:  `/adr <kebab-title>` slash command
- Cert scaffold: `/cert-scaffold <EXAM>` (one-time per exam)
- Cert update:   `/cert-update <N>` (end of each day)
- Full daily loop: `.claude/instructions/daily-workflow.md`

## Architecture (top-level map)
- `src/lab-observability-api/`  → .NET 8 Web API (the AI Gateway)
- `docs/adr/`                   → Architecture Decision Records (ADR-NNN-*)
- `docs/architecture/`          → System diagrams & sequence flows
- `docs/notes/Day-NNN/`         → Daily roadmap artifacts (per day)
- `docs/certifications/`        → Cert prep: AZ-900, AZ-104, AZ-305, AI-102
- `docs/standards/career-path.md` → Three-phase career progression detail
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
- **`files-changed.md` — per-day file change audit log:**
  Every day folder contains `docs/notes/Day-NNN/files-changed.md`. Whenever docs are
  updated in response to any step completion ("update all necessary docs", "update docs",
  "update the checklist", or equivalent), upsert rows in this file. **Dedup key is the
  file path** — if the file already has a row for that path, update it in place; never
  add a duplicate row. Format is a single flat markdown table:

  ```markdown
  # Day NNN — Files Changed

  | File | Step | Change |
  |---|---|---|
  | `path/to/file.ext` | verification | what changed and why |
  ```

  The `Step` column names which workflow step triggered the change
  (`scaffold`, `build`, `verification`, `deploy`, `docs pass`, etc.).
  Update this file as the last action of every doc-update pass.

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
  - **Exception — App Service appsettings PUT/PUT:** `/config/appsettings` PUT fails via
    both paths on this network (Day 7). **Workaround:** PATCH the parent site resource
    instead — `Invoke-RestMethod -Method PATCH` to `.../sites/{name}?api-version=2022-03-01`
    with body `{properties:{siteConfig:{appSettings:[{name,value},...]}}}`. PATCH goes
    through where PUT does not. Include ALL settings in the array — siteConfig.appSettings
    PATCH replaces the entire array, not merges. Portal is the fallback if PATCH also fails.
  - `api.applicationinsights.io` (used by `az monitor app-insights query`) is NOT
    affected — KQL queries work without workarounds.
- **IOptions<T>:** requires `using Microsoft.Extensions.Options;` explicitly.
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

## Certification Tracks (parallel to build work)

| Exam | Phase | Status |
|---|---|---|
| AZ-900 (Foundations) | Phase 1 | In progress |
| AI-102 (AI Engineer Associate) | Phase 1–2 | In progress — ⚠️ retires June 30, 2026 |
| AZ-104 (Administrator) | Phase 2–3 | Starts ~Day 10–15 |
| AZ-305 (Solutions Architect Expert) | Phase 2–3 | Post AZ-104 |

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
- This file (CLAUDE.md)
- `_principles.md` (architect posture)
- `career-path.md` (three-phase career progression)
- `naming-conventions.md`, `azure-environment.md` (live environment facts)
- The SKILL.md files (`.claude/skills/*/SKILL.md`)
- The CURRENT day's working artifacts (summary.md, completion-checklist.md, posture-check.md)
- The most recent 1-2 ADRs
- NOT in hot context (read on demand): `.claude/instructions/*`,
  `.claude/commands/*`, `.claude/hooks/*` — these are operational, not reasoning context

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

**When `docs/standards/` grows beyond 6-8 files:**
- That's the signal to consider sub-foldering
- Don't pre-optimize; let it accrete first

## Certification Tooling
- `/cert-scaffold <exam>` — run once per exam to build domain structure
- `/cert-update <day>` — run at end of each day session to populate
  domains touched that day (reads summary.md cert section automatically)
- Hook: `.claude/hooks/cert-tag.json` auto-tags domains as you work
- Coverage matrix: `docs/certifications/domain-coverage.md`
- Content: 10yo + CEO + Engineer + Architect explanations, synthesized practice questions,
  curated MS Learn + community resource links
- Do NOT reproduce paid exam bank content
