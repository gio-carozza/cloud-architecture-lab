# SLO and Performance Standard

**Phase:** 2 entry requirement (define targets now; enforce in Phase 2 alerting)
**Applies to:** all production endpoints on `app-ai-lab-api-dev-eastus-gio`

---

## Why define SLOs before Phase 2

Phase 1 builds the instrumentation (TTFT histogram, latency spans, 5xx alert). Phase 2 applies AI to real business problems — which means real users and real SLA conversations. Defining the targets in Phase 1 means the alert rules and KQL queries are already in place when Phase 2 begins.

---

## Latency SLOs

| Endpoint | p50 target | p95 target | p99 target | Measurement |
|---|---|---|---|---|
| `POST /api/ai/chat` (buffered) | < 2,000 ms | < 5,000 ms | < 10,000 ms | `ai.chat.complete` span duration |
| `POST /api/ai/chat/stream` (TTFT) | < 800 ms | < 2,000 ms | < 4,000 ms | `ai.provider.stream.ttft_ms` histogram |
| `POST /api/ai/batch` (submit) | < 500 ms | < 1,000 ms | < 2,000 ms | No tracing span exists for batch submit latency today — only `ai.provider.batch.submitted`/`completed` counters (count, not duration). This SLO is currently unmeasurable; would need a span added to `ClaudeBatchApiClient`. |
| `GET /api/ai/batch/{id}` (poll) | < 200 ms | < 500 ms | < 1,000 ms | `requests` table |
| `GET /health` | < 50 ms | < 100 ms | < 200 ms | `requests` table |

TTFT (time to first token) is the governing latency metric for the streaming path — total stream duration is secondary and not SLO-gated.

Latency targets assume Anthropic API is healthy. Track `claude.chat.api` vs `ai.chat.complete` span gap to separate gateway latency from provider latency.

---

## Availability SLO

| Target | Measurement window | Breach threshold |
|---|---|---|
| 99.5% uptime | Rolling 30 days | > 0.5% of requests return 5xx |

5xx rate alert already configured: `alert-ai-gateway-5xx-rate-dev-eastus-gio` fires at > 5% over 5 minutes (severity 2). That is an incident threshold, not the SLO threshold. The SLO is measured over 30 days in KQL, not by the alert rule.

---

## Throughput targets

| Endpoint | Max sustained RPS (Phase 1 lab) | Notes |
|---|---|---|
| `POST /api/ai/chat` | 5 RPS | Anthropic rate limits apply upstream |
| `POST /api/ai/chat/stream` | 3 RPS | SSE connections are long-lived; App Service connection limit applies |
| `POST /api/ai/batch` | 1 submit / 5 sec | Anthropic batch submission limits apply |

These are lab-environment targets, not enterprise scale. Phase 3 will require horizontal scaling analysis.

---

## Performance budget per feature

Every new endpoint or feature introduced in Day NNN must be assessed against the existing SLOs before the day closes. The `05-audit-log.md` P5 check covers this:

- Does the new path have a latency span?
- Is TTFT instrumented if it's a streaming path?
- Does the p95 latency of the new path fit within the SLO for its category?

If p95 is unknown (new feature, no traffic yet): use a synthetic load test with `k6` or `hey` against the local app. Record the result in `05-audit-log.md`.

---

## KQL queries for SLO measurement

Key queries are in `docs/standards/kql-cookbook.md`. For SLO reporting add:

- **Query 13** (to be added): p50/p95/p99 latency by endpoint over 30 days
- **Query 14** (to be added): 5xx rate over 30 days vs. 99.5% availability SLO
- **Query 15** (to be added): TTFT histogram percentiles for streaming path

Add these queries on the first day that SLO measurement becomes relevant (Phase 2 start, ~Day 21).

---

## Alert rules (Phase 2 additions)

Existing: 5xx rate > 5% over 5 min (severity 2).

Add in Phase 2:

| Alert | Threshold | Severity | When to add |
|---|---|---|---|
| p95 chat latency breach | > 5,000 ms over 10 min | 2 | Phase 2 start |
| TTFT p95 breach | > 2,000 ms over 5 min | 2 | Phase 2 start |
| Zero requests (silence) | No requests for 15 min during business hours | 3 | Phase 2 start |
| Cache hit rate drop | < 50% over 30 min | 3 | Phase 2 start |

All alerts route to `ag-ai-lab-dev-eastus-gio` (email `gio.carozza@outlook.com`).

---

## Performance review cadence

| Cadence | Action |
|---|---|
| End of each day | Check KQL Query 11 (TTFT percentiles) and Query 12 (stream duration) if streaming was touched |
| Phase 2 start | Define p95 baselines from 7 days of Phase 1 traffic |
| Monthly | Review 30-day availability and latency against SLO targets |
| Each provider addition | Run synthetic load test, compare p95 to SLO, document in ADR Consequences |
