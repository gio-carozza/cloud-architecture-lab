# Resources — Design Business Continuity Solutions (AZ-305 Domain 3)

---

## Official Microsoft Resources

- [AZ-305 Study Guide](https://learn.microsoft.com/en-us/credentials/certifications/resources/study-guides/az-305) — Authoritative domain breakdown; confirms which business continuity and reliability objectives appear in Domain 3
- [Reliability patterns — Azure Architecture Center](https://learn.microsoft.com/en-us/azure/architecture/framework/resiliency/reliability-patterns) — Canonical list of cloud reliability patterns including retry, circuit breaker, bulkhead, and timeout with Azure implementation guidance
- [Circuit Breaker pattern](https://learn.microsoft.com/en-us/azure/architecture/patterns/circuit-breaker) — Detailed pattern description with state diagram, implementation considerations, and when to use vs. retry alone
- [Retry pattern](https://learn.microsoft.com/en-us/azure/architecture/patterns/retry) — Canonical retry pattern reference; includes when NOT to retry, jitter guidance, and idempotency requirements
- [Microsoft.Extensions.Http.Resilience documentation](https://learn.microsoft.com/en-us/dotnet/core/resilience/http-resilience) — Official .NET documentation for `AddStandardResilienceHandler` and configuring retry, circuit breaker, and timeout policies

## Learning Paths

- [Design a solution for backup and disaster recovery (AZ-305)](https://learn.microsoft.com/en-us/training/modules/design-solution-for-backup-disaster-recovery/) — Covers business continuity patterns; includes reliability and resilience design for cloud workloads
- [Build resilient services with Microsoft.Extensions.Resilience](https://learn.microsoft.com/en-us/dotnet/core/resilience/) — .NET resilience patterns reference, including Polly integration and `AddStandardResilienceHandler` configuration

## Exam Readiness

- [Preparing for AZ-305: Design Business Continuity Solutions (Part 3 of 4)](https://learn.microsoft.com/en-us/shows/exam-readiness-zone/preparing-for-az-305-03-fy25) — Microsoft exam readiness video for Domain 3 objectives including backup, BCDR design, and reliability patterns
