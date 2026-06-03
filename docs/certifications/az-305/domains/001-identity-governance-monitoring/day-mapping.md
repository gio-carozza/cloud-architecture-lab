# Day Mapping — Design identity, governance, and monitoring solutions

| Day | Topics exercised | Build artifact |
|---|---|---|
| Day-007 | WAF Cost Optimization pillar as a design constraint (not post-audit); metrics vs. logs distinction for cost observability; layered cost governance model (budget controls + operational toggles + token telemetry); YAGNI applied to monitoring abstraction — Anthropic-specific today, abstract when second provider exists | `ai.provider.cache.hits` / `ai.provider.cache.misses` counters; `llm.cache.read_tokens` / `llm.cache.creation_tokens` custom dimensions; KQL Query 8 (cache hit rate) and Query 9 (estimated savings) as real-time cost observability |
