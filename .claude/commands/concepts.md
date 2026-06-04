## What is Azure Monitor?

### If you're 10 years old
Imagine your app is a fish tank. Azure Monitor is the person who checks
the tank every few seconds — is the water warm enough? Are the fish
swimming normally? Did anything spill? If something looks wrong, they
ring a bell so you can fix it before the fish die.

### If you're a CEO
Azure Monitor is the single pane of glass for everything running in Azure.
If your app is slow, crashing, or costing too much, Azure Monitor tells you
before your customers do — and before the cloud bill arrives. Without it,
you're flying blind. With it, you have the data to make decisions about
capacity, reliability, and cost.

### If you're an Engineer
Azure Monitor ingests three signal types: logs (via Log Analytics, queried
with KQL), metrics (time-series via the Metrics store, 93-day retention by
default), and traces (via Application Insights, OpenTelemetry-compatible
since 2023). In .NET, wire it via `Azure.Monitor.OpenTelemetry.AspNetCore`:
call `builder.Services.AddOpenTelemetry().UseAzureMonitor()`. Set the
connection string via `APPLICATIONINSIGHTS_CONNECTION_STRING`. Common error:
forgetting to add `WithTracing()` and `WithMetrics()` — the SDK doesn't add
them by default.

### If you're an Architect
Azure Monitor is the unified observability platform for Azure workloads.
Alert rules can target all three signal types. For AI gateways, the
critical metric is token cost per request — without it, cost anomalies are
invisible until the monthly bill arrives. Workspace-based Application
Insights (vs. classic) is required for Log Analytics integration and
is the only path forward as classic retires. Log Analytics query costs
accumulate with retention and ingestion volume — size retention policies
per signal type, not uniformly. The wrong choice at instrumentation time
means retrofitting observability under production load, which is the most
expensive possible time to do it.
