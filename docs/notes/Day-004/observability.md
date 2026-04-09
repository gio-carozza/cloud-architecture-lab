
---

## File: `C:\dev\cloud-architecture-lab\docs\notes\day-4\observability.md`

```md
# Day 4 Observability Notes

## Objective
Instrument the first workload so I can observe requests, logs, and failures after deployment.

## What was implemented
- Application Insights SDK added to the API
- Structured logging with `ILogger`
- Root, health, and test endpoints
- Intentional error endpoint for exception validation

## Telemetry expected
- Requests
- Response times
- Failures
- Exceptions
- Application traces/logs

## Validation steps
1. Open the deployed root endpoint
2. Open `/health`
3. Open `/api/test/ping`
4. Trigger `/api/test/error`
5. Open Application Insights in Azure Portal
6. Review:
   - Requests
   - Failures
   - Traces
   - Live Metrics

## Why this matters
Observability turns a deployed app into an operable system. Without telemetry, production support becomes guesswork.

## What I should look for
- Which endpoints are called most often
- Whether failures are isolated or repeated
- Response duration patterns
- Whether warnings appear before failures
- Whether telemetry is sufficient for troubleshooting

## Architect interpretation
A mature architecture is not judged only by whether it runs. It is judged by whether the team can detect issues, diagnose them quickly, and improve the system with evidence.