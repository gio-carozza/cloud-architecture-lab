# AZ-104 — Exam Objectives

> Verify against the official Skills Measured PDF before exam prep:
> <https://learn.microsoft.com/en-us/credentials/certifications/azure-administrator/>
> Weights shift with exam updates. Last verified against training data: ~early 2025.

## Domain 1 — Manage Azure Identities and Governance (~20–25%)

### Manage Microsoft Entra users and groups

- Create, configure, and manage user accounts (bulk operations, guest accounts, external identities)
- Create and manage groups (security groups, Microsoft 365 groups, dynamic membership rules)
- Manage licenses and guest access (B2B collaboration, access reviews)

### Manage access to Azure resources

- Configure Azure role-based access control (RBAC) — built-in and custom roles
- Assign roles at management group, subscription, resource group, and resource scope
- Interpret access denied errors; use effective permissions view

### Manage Azure subscriptions and governance

- Configure Azure policies — definitions, assignments, remediation tasks
- Apply and manage resource locks (ReadOnly, CanNotDelete)
- Configure and manage resource tags — inheritance, enforcement via policy
- Manage cost with Azure Cost Management (budgets, alerts, cost analysis)
- Configure management groups for hierarchy governance

### Manage Microsoft Entra authentication

- Configure multi-factor authentication (MFA) per-user and Conditional Access
- Configure self-service password reset (SSPR)
- Implement Conditional Access policies (location, device, risk-based)

---

## Domain 2 — Implement and Manage Storage (~15–20%)

### Configure Azure Storage accounts

- Create and configure storage accounts (LRS, ZRS, GRS, GZRS, RA-GRS redundancy)
- Configure access tiers (Hot, Cool, Cold, Archive) and lifecycle management policies
- Configure large file shares, soft delete, and versioning

### Configure Azure Blob Storage

- Create containers and configure access levels (Private, Blob, Container)
- Configure blob versioning, soft delete, and change feed
- Configure object replication and immutability policies

### Configure Azure Storage security

- Generate and manage shared access signatures (SAS) — service, account, user delegation
- Configure stored access policies
- Configure Azure Storage encryption (Microsoft-managed, customer-managed keys)
- Configure infrastructure encryption and secure transfer requirement

### Configure Azure Files

- Create and configure Azure file shares (SMB, NFS)
- Configure Azure File Sync — sync groups, server endpoints, cloud tiering
- Configure file share snapshots and soft delete

### Manage Azure Storage

- Use AzCopy for data transfer (import/export, cross-account copy)
- Use Azure Storage Explorer for management
- Configure Azure Import/Export service for large-scale offline transfer

---

## Domain 3 — Deploy and Manage Azure Compute Resources (~20–25%)

### Configure VMs for high availability and scalability

- Create and configure availability sets (fault domains, update domains)
- Create and configure Azure Virtual Machine Scale Sets (VMSS) — manual and autoscale
- Configure Azure dedicated hosts for compliance/licensing isolation

### Provision and manage VMs

- Create VMs (portal, CLI, ARM templates, Bicep)
- Configure VM size, OS disks, data disks, disk caching
- Configure VM extensions (Custom Script, Azure Monitor Agent, diagnostics)
- Manage VM backups, snapshots, and restore points
- Move VMs between resource groups and subscriptions

### Create and configure containers

- Create and configure Azure Container Instances (ACI) — single container, container groups
- Create and configure Azure Kubernetes Service (AKS) — basics: nodes, pods, deployments, services
- Configure container registries (ACR) — image push/pull, geo-replication

### Create and configure Azure App Service

- Create App Service plans (tiers, scaling)
- Create and configure web apps (runtime stacks, TLS/SSL, custom domains)
- Configure deployment slots and slot swaps (staging → production)
- Configure auto-scaling rules for App Service plans
- Configure deployment methods (GitHub Actions, zip deploy, container)

---

## Domain 4 — Implement and Manage Virtual Networking (~15–20%)

### Configure virtual networks

- Create and configure virtual networks (VNets) and subnets
- Configure private endpoints and service endpoints
- Configure VNet peering (same region, global) — connectivity and routing implications

### Configure secure access to virtual networks

- Create and configure network security groups (NSGs) — inbound/outbound rules, ASGs
- Configure Azure Bastion for secure RDP/SSH without public IPs
- Configure VPN Gateway (point-to-site, site-to-site) — SKUs, BGP, active-active
- Configure Azure ExpressRoute — circuits, peering types, FastPath

### Configure name resolution and load balancing

- Configure Azure DNS — public and private zones, A, CNAME, alias records, delegation
- Configure Azure Load Balancer — basic vs standard, health probes, load-balancing rules, NAT rules
- Configure Azure Application Gateway — URL-based routing, WAF, SSL termination
- Configure Azure Traffic Manager — routing methods (priority, weighted, performance, geographic)

### Monitor and troubleshoot virtual networking

- Use Network Watcher — connection monitor, packet capture, IP flow verify, NSG flow logs
- Diagnose and resolve VPN connectivity issues
- Interpret NSG flow logs and connection troubleshooting results

---

## Domain 5 — Monitor and Maintain Azure Resources (~10–15%)

### Monitor resources using Azure Monitor

- Configure and interpret Azure Monitor metrics — namespaces, dimensions, aggregations
- Create metric alerts, log alerts, and activity log alerts
- Configure action groups — email, SMS, webhook, Logic App, Automation Runbook
- Create and configure Azure Monitor workbooks and dashboards
- Configure diagnostic settings — route logs to Log Analytics, Storage, Event Hubs
- Query logs with KQL (Kusto Query Language) in Log Analytics

### Implement backup and recovery

- Create and configure Recovery Services vaults
- Configure Azure Backup for VMs, Azure Files, SQL Server in Azure VMs
- Configure backup policies (schedule, retention)
- Perform and validate restores (full VM, file-level, item-level)
- Configure soft delete for backup data

### Implement disaster recovery

- Configure Azure Site Recovery (ASR) for VM replication
- Create replication policies and configure replication for VMs
- Perform test failovers and planned failovers
- Configure recovery plans (runbook automation, manual steps)

---

> Weights above are approximate — verify against the current PDF before exam prep.
> This repo activates AZ-104 track at ~Day 035 (target: Day 070). Domain 5 (Monitor) has the most
> direct overlap with gateway build work — Days 6, 7, 9 are already mapped there.
