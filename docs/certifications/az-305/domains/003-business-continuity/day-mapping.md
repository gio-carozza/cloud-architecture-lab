# Day Mapping — Design Business Continuity Solutions (AZ-305 Domain 3)

| Day | Topics Covered |
|-----|----------------|
| Day-006 | Retry with exponential backoff + jitter (thundering herd prevention); circuit breaker pattern (Closed/Open/Half-Open states, 50% failure ratio, 30s sampling window); attempt timeout vs. overall timeout; `Microsoft.Extensions.Http.Resilience` AddStandardResilienceHandler; non-retriable error classification (401/403 must not be retried); per-provider circuit breaker scope |
