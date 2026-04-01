# ADR-004: First Workload on Azure App Service with Application Insights

## Status
Accepted

## Context
The lab has focused so far on Azure environment setup, CAF foundations, and Well-Architected analysis. The next step is to move from architecture theory into a deployed workload that can be operated and observed.

The first workload needs to be:
- simple
- fast to deploy
- low cost
- aligned with Azure-native services
- instrumented for telemetry from the beginning

## Decision
Deploy the first lab API as an ASP.NET Core Web API on Azure App Service and integrate it with Application Insights for observability.

## Why this decision was made
- App Service is a fast managed hosting option for APIs
- It avoids early infrastructure complexity
- It is appropriate for learning deployment, configuration, and operations
- Application Insights provides immediate visibility into requests, logs, failures, and performance signals
- This combination supports Operational Excellence as a practical discipline, not just a theory topic

## Consequences

### Positive
- Faster first deployment
- Lower operational overhead
- Native Azure observability path
- Good platform for learning release and monitoring patterns

### Negative
- Limited production realism compared to containerized or fully automated environments
- Manual deployment introduces drift risk
- Security and networking are still basic
- No infrastructure-as-code baseline yet

## Alternatives considered
1. Deploy to Azure Container Apps
   - Rejected for Day 4 because it adds more platform complexity than needed for the first workload

2. Deploy to AKS
   - Rejected because it is too heavy for the first deployment milestone

3. Stay local only
   - Rejected because the learning objective requires real Azure deployment and observability

## Follow-up decisions expected
- Add CI/CD pipeline
- Add IaC with Bicep
- Add alerting and dashboards
- Add identity and secrets management
- Add production-readiness controls