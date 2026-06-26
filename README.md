# Cloud Architecture Lab

A three-phase career progression toward LLM Architect: AI Engineer → Forward-Deployed Engineer → LLM Architect.

Architect posture: [`docs/standards/_principles.md`](docs/standards/_principles.md)  
Career path: [`docs/standards/career-path.md`](docs/standards/career-path.md)

---

## Anchor Workload

**.NET 8 AI Gateway** deployed to Azure App Service (East US).  
Provider-abstracted LLM routing — currently Anthropic Claude; designed for Azure OpenAI, Bedrock, and Foundry.

**Phase 1 (Days 1–9, complete):**

- Provider abstraction (`IChatModelProvider` seam) — ADR-005
- Observability: OpenTelemetry + Application Insights, correlation IDs, structured errors — ADR-006, ADR-008
- Prompt caching (90% input token cost reduction) — ADR-009
- Batch API for async cost control (`IBatchChatModelProvider`) — ADR-010
- SSE streaming with TTFT telemetry — ADR-011

**Day 10 next** — multi-turn context management.

---

## Azure Environment

| Resource | Name |
|---|---|
| Subscription | `gio-architecture-lab` |
| Resource Group | `rg-ai-lab-dev-eastus` |
| App Service | `app-ai-lab-api-dev-eastus-gio` |
| App Insights | `appi-ai-lab-api-dev-eastus-gio` |
| Log Analytics | `law-ai-lab-dev-eastus-gio` |

Region: East US

---

## Repository Structure

```text
src/lab-observability-api/   .NET 8 AI Gateway (anchor workload)
docs/adr/                    Architecture Decision Records (ADR-001 through ADR-NNN)
docs/architecture/           System diagrams and sequence flows (per day)
docs/notes/Day-NNN/          Daily roadmap artifacts (01-summary, 02-checklist, etc.)
docs/notes/changelog.md      Running file-change log across all days
docs/certifications/         Cert prep: AZ-900, AZ-104, AZ-305, AI-103 (Phase 2)
docs/standards/              Principles, naming conventions, KQL cookbook, collaboration map
Infra/Day-NNN/               Per-day IaC and appsettings templates
.claude/commands/            Slash commands (/deploy, /new-day, /cert-update, /adr, etc.)
.claude/skills/              Reusable knowledge packs (auto-invoked by Claude Code)
.claude/instructions/        Daily workflow procedure
```

---

## Certification Tracks

| Exam | Phase | Status |
|---|---|---|
| AZ-900 (Fundamentals) | Phase 1 | In progress |
| AZ-104 (Administrator) | Phase 1–2 | In progress |
| AZ-305 (Solutions Architect Expert) | Phase 2–3 | Scheduled |
| AI-103 (Azure AI Apps and Agents Developer) | Phase 2 | Beta — starts ~Day 21 |

---

## Key Commands

```bash
dotnet build src/lab-observability-api/lab-observability-api.csproj
dotnet run --project src/lab-observability-api
dotnet test
```

Deploy: `/deploy` (Kudu zip path — see `.claude/skills/azure-deploy/SKILL.md`)
