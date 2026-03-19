# Azure Cloud Adoption Framework (CAF)

The Cloud Adoption Framework provides guidance for organizations adopting cloud technologies.

CAF includes several stages:

Strategy  
Plan  
Ready  
Adopt  
Govern  
Manage

## Ready Phase

The Ready phase focuses on preparing the cloud environment before deploying workloads.

Preparation includes:

• subscription structure  
• identity configuration  
• network planning  
• governance policies  

The purpose is to create a stable environment where workloads can be deployed safely.

## Topics to Learn
- Landing zones
- Governance
- Resource organization
- Subscription strategy
- Management groups

Each section learned is broken down into 5 steps:

1. Meaning (Plain-English)
2. What an architect actually thinks about
3. What it looks like in real Azure
4. What it means for your lab today
5. One takeaway you should remember

# Landing Zone

1. Plain English: 
    A landing zone is a pre-built cloud environment where applications can safely live.

2. What an architect thinks about:
    When designing a landing zone, you think:
    a. Where will workloads live?
    b. How do we separate environments (dev/test/prod)?
    c. How do we enforce security and governance?
    d. How do we scale this later without breaking things?

3. What it looks like in Azure:
    A real landing zone includes:
    a. multiple subscriptions
    b. management groups
    c. policies
    d. network design (VNets, subnets)
    e. identity and access (RBAC)
    f. monitoring/logging

4. Lab Result Day 2:
    a. 1 subscription
    b. 1 resource group
    That is NOT a landing zone yet — it’s a learning sandbox.

5. Key takeaway
    A landing zone is the foundation for everything, not just where you deploy resources.

# Governance

1. Plain English
    Governance = rules and controls for how Azure is used.

2. What an architect thinks about
    a. Who can create resources?
    b. What regions are allowed?
    c. What naming standards must be followed?
    d. How do we control cost?
    e. How do we enforce security?
    
3. What it looks like in Azure
    a. Azure Policy - enforce rules (e.g., only East US allowed)
    b. RBAC (Role-Based Access Control) - who can do what
    c. Budgets - cost control
    d. Tags - cost tracking and organization

4. Lab Result Day 2:
    Governance started:
        ✅ budget (lab-monthly-limit)
        ✅ naming convention (rg-ai-lab-dev-eastus)
    Pending:
        policies
        RBAC design

5. Key takeaway
    Governance makes sure the cloud doesn’t turn into chaos.

# Resource Organization

1. Plain English
    How you group and structure resources.

2. What an architect thinks about
    a. What resources belong together?
    b. Should this app have its own resource group?
    c. Should environments be separated?
    d. What happens when we delete something?

3. What it looks like in Azure
    a. Resource Groups
        logical containers
    b. Tagging strategy
        environment=dev
        app=ai-lab
    c. Naming standards

    Examples
        rg-ai-api-dev-eastus
        rg-ai-data-dev-eastus
        rg-ai-api-prod-eastus

4. Lab Result Day 2:
    The following resource exists:
        rg-ai-lab-dev-eastus
    Pending:
        API
        data
        AI services

5. Key takeaway
    Resource organization is about clarity, lifecycle, and control.

# Subscription Strategy

1. Plain English
    How you divide Azure into separate billing and management boundaries.

2. What an architect thinks about
    a. Should dev and prod be separated?
    b. Who owns which subscription?
    c. How do we isolate risk?
    d. How do we control costs per team?

3. What it looks like in real Azure
    a. Common structure:
        Company Root
        │
        ├─ Dev Subscription
        ├─ Test Subscription
        ├─ Prod Subscription
    b. Why?
        isolate environments
        reduce risk
        separate billing
        enforce governance differently

4. Lab Result Day 2:
    a. Current Subscription:
        gio-architecture-lab
    b. Pending
        gio-arch-dev
        gio-arch-prod

5. Key takeaway
    Subscriptions are boundaries for cost, access, and risk.   

# Management Groups

1. Plain English
    Management groups are a way to organize multiple subscriptions.

2. What an architect thinks about
    a. How do I apply policies to MANY subscriptions at once?
    b. How do I group environments logically?
    c. How do I scale governance?

3. What it looks like in Azure
    a. Hierarchy:
        Tenant Root
        │
        ├─ Platform
        │   ├─ Shared Services
        │   └─ Identity
        │
        └─ Landing Zones
            ├─ Dev
            ├─ Test
            └─ Prod    
    b. Policies applied at higher levels flow down.
    c. Why this matters
        Instead of configuring 10 subscriptions manually:
        👉 You apply rules once at the management group level

4. Lab Result Day 2:
    No Management Goups yet

5. Key takeaway
    Management groups = governance at scale

# How these ALL connect (this is the big picture)

This is the mental model you want:
    Management Groups
        ↓
    Subscriptions
        ↓
    Landing Zone (platform structure)
        ↓
    Resource Groups
        ↓
    Resources (apps, databases, AI services)

## Notes
This file will store my notes on Azure Cloud Adoption Framework.
