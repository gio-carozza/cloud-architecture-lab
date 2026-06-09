# Cross-Cert Vocabulary & Patterns

Concepts that appear across AZ-900, AZ-104, AZ-305, and AI-102. Microsoft asks
the same idea with slightly different wording per exam — knowing the shared
vocabulary saves hours of re-learning.

## Identity & Governance
- **Microsoft Entra ID** (formerly Azure AD) — the identity plane
- **Managed Identity** (system-assigned vs user-assigned) — passwordless auth for Azure resources
- **RBAC** — role assignments at scope (mgmt group → subscription → RG → resource)
- **Azure Policy** — declarative governance and compliance enforcement
- **Management Groups** — hierarchy above subscriptions for inherited policy

## Networking
- **VNet, Subnet, NSG, ASG** — network isolation primitives
- **Private Endpoint** — bring an Azure PaaS service onto your VNet privately
- **Service Endpoint** — older, less isolated; prefer Private Endpoint for new design
- **Application Gateway / Front Door** — L7 load balancing & WAF (regional vs global)

## Compute
- **App Service vs Container Apps vs AKS** — managed PaaS → serverless containers → orchestration
- **App Service Plan** — the pricing/scale unit; Linux vs Windows
- **Deployment slots** — staging + prod with zero-downtime swap

## Storage & Data
- **Storage Account kinds** — General-purpose v2 (default), Blob, FileStorage, BlockBlobStorage
- **Access tiers** — Hot, Cool, Cold, Archive (cost vs latency tradeoff)
- **Cosmos DB consistency levels** — Strong → Bounded Staleness → Session → Consistent Prefix → Eventual

## Monitoring
- **Azure Monitor** — the umbrella service
- **Log Analytics workspace** — the storage backend for logs/metrics
- **Application Insights (workspace-based)** — APM layer; backed by Log Analytics
- **Diagnostic settings** — route platform logs/metrics to a destination
- **KQL** — Kusto Query Language; same syntax across Logs, Sentinel, Defender

## Resilience
- **Availability Zones** — within-region redundancy
- **Paired Regions** — cross-region replication pattern (older model; verify current Azure stance)
- **Circuit Breaker, Retry with Jitter, Bulkhead** — resilience patterns AZ-305 expects you to *select*, not necessarily code
- **RPO / RTO** — Recovery Point / Time Objective — the two numbers every BCP question hinges on

## AI-Specific (AI-102)
- **Azure OpenAI** — Azure-hosted OpenAI models with enterprise controls
- **Azure AI Foundry** — the unified studio/SDK for building AI solutions
- **Azure AI Search** — vector + keyword search; the default RAG retrieval layer
- **Content Safety** — moderation API for harm categories
- **Document Intelligence** (formerly Form Recognizer) — structured extraction from documents

## Cost
- **Pricing Calculator** — pre-deploy estimation
- **Azure Advisor** — post-deploy recommendations (cost, security, reliability, performance, operational excellence)
- **Cost Management + Billing** — actual spend, budgets, alerts
- **Reservations / Savings Plans** — commit-to-save for predictable workloads