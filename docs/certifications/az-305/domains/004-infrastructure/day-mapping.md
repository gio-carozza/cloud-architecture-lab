# Day Mapping — Design Infrastructure Solutions (AZ-305 Domain 4)

| Day | Topics Covered |
|-----|----------------|
| Day-008 | Async/job-shaped workload pattern (submit → poll → retrieve); recommending compute for batch AI workloads (provider-native batch API vs. Functions vs. Container Apps vs. AKS); WAF Cost Optimization pillar — deferrable workload routing for 50% token cost reduction; latency SLA as architectural constraint driving processing path selection |
| Day-009 | Streaming vs. buffered response pattern (SSE for interactive AI); TTFT (time-to-first-token) as the governing latency SLO for streaming workloads; nginx proxy buffering on App Service (`X-Accel-Buffering: no`); client disconnect → upstream CancellationToken propagation for cost governance; Liskov Substitution test as the deciding criterion for interface extension vs. new seam (streaming extends IChatModelProvider; batch earns IBatchChatModelProvider) |
