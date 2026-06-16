# Cost Governance Standard

**Phase:** 1 (active now)
**Applies to:** all LLM API usage, Azure resource spend, and cost-control decisions

---

## Cost targets (per request)

| Path | Input token budget | Output token budget | Notes |
|---|---|---|---|
| `POST /api/ai/chat` (buffered) | 4,000 tokens | 1,024 tokens | Default `MaxTokens` in `AnthropicOptions` |
| `POST /api/ai/chat/stream` (streaming) | 4,000 tokens | 1,024 tokens | Same budget; TTFT not a cost driver |
| `POST /api/ai/batch` (batch submit) | 100,000 tokens per job | 4,096 per item | 50% cost reduction vs. real-time |

Budgets are enforced via `MaxTokens` in `AnthropicOptions`. Never hard-code token limits in controllers.

---

## Prompt caching requirement

Prompt caching (`EnablePromptCaching = true`) is the **default and required** production state. Disabling it must be documented in the day's `07-files-changed.md` with a reason.

Expected cache economics for a stable system prompt:

| Metric | Target |
|---|---|
| Cache hit rate | ≥ 80% after warm-up (first 5 requests) |
| Cached input cost | ~10% of uncached rate |
| `ai.provider.cache.hits` counter | Visible in App Insights within 5 min of traffic |

If cache hit rate drops below 80% for more than 10 consecutive requests, investigate before shipping more features.

---

## Batch vs. real-time decision rule

Use `IBatchChatModelProvider` (batch) when ALL of the following are true:

- The caller does not need a response in the same HTTP request
- The workload is ≥ 10 requests that can be grouped
- Latency tolerance is ≥ 24 hours (Anthropic batch SLA)

Use `IChatModelProvider` (real-time) when ANY of the following is true:

- A human is waiting for the response
- The result is needed within the same session
- The operation is interactive (streaming, TTFT matters)

Never route interactive chat through the batch path to save cost — the UX cost exceeds the token savings.

---

## Azure spend controls

| Control | Setting | Where |
|---|---|---|
| Monthly budget | `lab-monthly-limit` budget alert | Azure subscription |
| 5xx error rate alert | > 5% over 5 min, severity 2 | `alert-ai-gateway-5xx-rate-dev-eastus-gio` |
| Action group | Email to `gio.carozza@outlook.com` | `ag-ai-lab-dev-eastus-gio` |

If a budget alert fires: stop all non-essential API calls, investigate which path is the source, and reduce `MaxTokens` or switch to batch before resuming.

---

## Cost observability (KQL)

Cost signals live in the `dependencies` table in App Insights. Key queries are in `docs/standards/kql-cookbook.md`:

- Query 8: cache hit rate
- Query 9: estimated daily token savings from caching
- Query 10: token spend by path (chat vs. stream vs. batch)

Run Query 9 at the end of every day that touched the LLM path. If savings are not showing, check `EnablePromptCaching` and verify the system prompt exceeds 1,024 tokens (Anthropic's minimum cacheable size).

---

## Cost review cadence

| Cadence | Action |
|---|---|
| End of each day | Run KQL Query 9; log result in `05-audit-log.md` |
| End of each week | Check Azure Cost Management for actual spend vs. budget |
| Phase transition (Day 20, Day 50) | Review token budgets; adjust `MaxTokens` if workload changed |

---

## Adding a new LLM path

Whenever a new endpoint or provider path is added, cost governance requires:

- [ ] `MaxTokens` set via `AnthropicOptions` — never hard-coded
- [ ] Prompt caching decision documented (on or off, and why)
- [ ] `ai.provider.cache.hits` / `ai.provider.cache.misses` counters fire on the new path
- [ ] A KQL query added to `kql-cookbook.md` for the new path's token spend
- [ ] `Infra/Day-NNN/appsettings-template.md` updated if new config keys control cost behavior
