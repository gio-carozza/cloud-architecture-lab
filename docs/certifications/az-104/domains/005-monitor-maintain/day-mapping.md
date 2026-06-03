# Day Mapping — Monitor and maintain Azure resources

| Day | Topics exercised | Build artifact |
|---|---|---|
| Day-006 | App Insights setup (workspace-based), correlation IDs, structured log enrichment, KQL queries 1–7, alert rule for 5xx rate | `appi-ai-lab-api-dev-eastus-gio`; `alert-ai-gateway-5xx-rate-dev-eastus-gio`; `kql-cookbook.md` queries 1–7 |
| Day-007 | KQL queries 8 (cache hit rate) and 9 (estimated savings) against `dependencies` table; `customDimensions["llm.cache.read_tokens"]` and `["llm.cache.creation_tokens"]` as queryable dimensions; `toint()` cast pattern; `union` for cross-table correlation | `kql-cookbook.md` queries 8–9 verified against live App Insights data; 50% cache hit rate confirmed via Query 8 |
