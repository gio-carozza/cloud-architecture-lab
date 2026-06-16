# AZ-305 — Exam Objectives

> Verify against the official Skills Measured PDF before exam prep:
> <https://learn.microsoft.com/en-us/credentials/certifications/azure-solutions-architect/>
> AZ-305 requires passing AZ-104 first (or equivalent operational fluency).
> Weights shift with exam updates. Last verified against training data: ~early 2025.

## Domain 1 — Design Identity, Governance, and Monitoring Solutions (~25–30%)

### Design identity and access management solutions

- Recommend identity solutions using Microsoft Entra ID, B2B, B2C, External ID
- Design for Managed Identities (system-assigned, user-assigned) — eliminate credential management
- Design for service principals, app registrations, workload identity
- Recommend RBAC vs. Azure AD roles; design custom role scope hierarchy

### Design governance solutions

- Design management group and subscription hierarchy for enterprise scale
- Design Azure Policy assignments — initiative definitions, compliance, remediation
- Design tagging strategy for cost attribution, lifecycle, environment classification
- Design for Azure Blueprints (landing zone scaffolding)
- Recommend resource locking strategy (ReadOnly vs. CanNotDelete)

### Design monitoring solutions

- Recommend Azure Monitor strategy — metrics, logs, traces, distributed tracing
- Design Log Analytics workspace architecture (centralized vs. decentralized)
- Design for Application Insights — workspace-based; sampling; availability tests
- Design alert strategy — metric alerts, log alerts, activity log alerts, action groups
- Design for Azure Service Health and resource health integration
- Design cost monitoring strategy using Cost Management + Budgets + alerts

---

## Domain 2 — Design Data Storage Solutions (~25–30%)

### Design for relational data

- Recommend Azure SQL Database vs. SQL Managed Instance vs. SQL Server on VM
- Design for high availability: zone redundancy, geo-replication, failover groups
- Design for performance: service tiers, elastic pools, read replicas
- Design backup and restore strategy: automated backups, long-term retention, PITR

### Design for non-relational data

- Recommend Azure Cosmos DB API (Core SQL, MongoDB, Cassandra, Gremlin, Table)
- Design for Cosmos DB consistency levels — tradeoffs between consistency and availability
- Design partitioning strategy for Cosmos DB — throughput, hot partitions
- Recommend Azure Table Storage vs. Cosmos DB for low-cost key-value workloads
- Recommend Azure Cache for Redis — cache-aside, session state, pub/sub patterns

### Design data integration

- Recommend Azure Data Factory vs. Azure Synapse Pipelines for data movement
- Design for Azure Data Lake Storage Gen2 — hierarchical namespace, RBAC, ACLs
- Design for streaming data: Azure Event Hubs, Azure Stream Analytics

---

## Domain 3 — Design Business Continuity Solutions (~10–15%)

### Design for high availability

- Define RTO and RPO targets and map to Azure service SLAs
- Design for availability zones vs. availability sets — when each applies
- Design multi-region active-active vs. active-passive architectures
- Recommend Azure Traffic Manager routing methods for HA (priority, weighted, geographic)
- Recommend Azure Front Door for global load balancing + WAF + CDN integration

### Design for backup and disaster recovery

- Design Azure Backup policy — VM backup, Azure Files, SQL, retention tiers
- Design Recovery Services vault architecture — cross-region restore, soft delete
- Design Azure Site Recovery (ASR) for VM replication — RPO, test failover strategy
- Design backup for Azure PaaS services: App Service (deployment slots as partial DR)

---

## Domain 4 — Design Infrastructure Solutions (~25–30%)

### Design compute solutions

- Recommend VM SKU selection — compute/memory/storage optimized, spot vs. reserved
- Design for VM Scale Sets — autoscale policies, upgrade policies, health probes
- Recommend AKS vs. ACI vs. App Service vs. Azure Functions for containerized workloads
- Design AKS cluster — node pools, autoscaler, pod disruption budgets, network policy
- Recommend App Service plan tier — Shared, Basic, Standard, Premium, Isolated (ASE)

### Design network solutions

- Design VNet topology — hub-and-spoke vs. flat; peering vs. VPN Gateway
- Design for private connectivity: Private Endpoints, Private Link Service
- Design perimeter security: Azure Firewall, NSGs, Application Gateway WAF, DDoS Protection
- Recommend ExpressRoute vs. VPN Gateway — latency, bandwidth, SLA tradeoffs
- Design DNS strategy: Azure DNS private zones, custom resolvers, hybrid DNS

### Design application architecture

- Recommend messaging patterns: Service Bus (guaranteed delivery), Event Grid (event-driven), Event Hubs (streaming)
- Design for API Management — rate limiting, caching, transformation, backend routing
- Design caching strategy: Azure Cache for Redis, CDN, Application Gateway caching
- Recommend serverless patterns: Azure Functions triggers/bindings, Durable Functions
- Design for cost optimization: reserved instances, spot VMs, autoscale, right-sizing

### Design migration solutions

- Apply Azure Migrate for assessment and server migration
- Recommend lift-and-shift vs. re-platform vs. re-architect based on constraints
- Design for database migration: Azure Database Migration Service, DMS online vs. offline

---

> This repo activates AZ-305 track after AZ-104 (Phase 2–3).
> Domain 1 (governance/monitoring) and Domain 4 (infrastructure/cost) have the most
> direct overlap with gateway build work — Days 2–9 are already mapped there.
