## What is Azure Monitor?

### If you're 10 years old
Imagine your app is a fish tank. Azure Monitor is the person who checks
the tank every few seconds — is the water warm enough? Are the fish
swimming normally? Did anything spill? If something looks wrong, they
ring a bell so you can fix it before the fish die.

### If you're an architect
Azure Monitor is the unified observability platform for Azure workloads.
It ingests logs via Log Analytics (KQL queryable), metrics via the
Metrics store (time-series, 93-day retention by default), and traces
via Application Insights (distributed tracing, OTel-compatible since
2023). Alert rules can target all three signal types...