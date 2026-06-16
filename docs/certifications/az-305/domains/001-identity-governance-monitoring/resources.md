# Resources — Design identity, governance, and monitoring solutions

## Day 007 additions (2026-06-03)

*Focus: WAF Cost Optimization pillar, observable cost controls, AI workload governance*

---

## Official Microsoft Resources

- [Azure Well-Architected Framework — Cost Optimization pillar](https://learn.microsoft.com/en-us/azure/well-architected/cost-optimization/) — the authoritative WAF cost guidance; design principles, trade-offs, and Azure service patterns
- [Cost optimization design principles](https://learn.microsoft.com/en-us/azure/well-architected/cost-optimization/principles) — the 5 principles AZ-305 exam questions are built around
- [Azure Monitor overview](https://learn.microsoft.com/en-us/azure/azure-monitor/overview) — the platform that unifies metrics, logs, and alerting; foundational for AZ-305 monitoring design questions
- [Metrics vs. logs in Azure Monitor](https://learn.microsoft.com/en-us/azure/azure-monitor/fundamentals/data-platform) — canonical reference distinguishing the two signal types, their latency, and their use cases
- [Azure Cost Management + Billing](https://learn.microsoft.com/en-us/azure/cost-management-billing/cost-management-billing-overview) — budget controls, cost alerts, and cost analysis — the financial governance layer
- [Azure Well-Architected Review](https://learn.microsoft.com/en-us/assessments/azure-architecture-review/) — the official WAF assessment tool; useful for understanding how Microsoft frames cost optimization questions

## Diagrams and Visual References

- [WAF Cost Optimization pillar diagram](https://learn.microsoft.com/en-us/azure/well-architected/cost-optimization/) — MS Learn; the pillar's five design principles as a visual
- [Azure Monitor data platform diagram](https://learn.microsoft.com/en-us/azure/azure-monitor/fundamentals/data-platform#metrics-and-logs) — MS Learn; shows metrics vs. logs flow, retention, and query paths

## Video (≤ 20 min)

- [AZ-305 Exam Readiness: Design identity, governance, and monitoring solutions](https://learn.microsoft.com/en-us/shows/exam-readiness-zone/preparing-for-az-305-design-identity-governance-and-monitoring-solutions-1-of-4) — Exam Readiness Zone, ~15 min, covers AZ-305 Domain 1 directly
- [Well-Architected Framework overview](https://www.youtube.com/watch?v=mNFw4zrVGug) — Azure Fridays, ~18 min, covers all five pillars including Cost Optimization with Azure service examples

## Hands-on

- [WAF architectural review for AI gateways](https://learn.microsoft.com/en-us/azure/architecture/ai-ml/architecture/baseline-openai-e2e-chat) — reference architecture that applies WAF cost optimization to an Azure OpenAI gateway — close analog to this roadmap's AI gateway
