# Monitoring and Incident Runbook

**Phase:** 1 (active now — alerts are already configured)
**Applies to:** `app-ai-lab-api-dev-eastus-gio` and all dependent Azure resources

---

## Alert inventory

| Alert | Condition | Severity | Notification |
|---|---|---|---|
| `alert-ai-gateway-5xx-rate-dev-eastus-gio` | 5xx rate > 5% over 5 min | 2 | Email `gio.carozza@outlook.com` |
| Budget alert (`lab-monthly-limit`) | Spend > threshold | N/A | Email |

Phase 2 additions (defined in `slo-performance.md`): p95 latency, TTFT breach, zero-request silence, cache hit rate drop.

---

## Daily monitoring check (end of each day)

Run these KQL queries from `docs/standards/kql-cookbook.md` before closing any day that deployed to Azure:

| Query | What to verify |
|---|---|
| Query 1: recent requests | 2xx rate > 95% for past hour |
| Query 4: error rate | Zero 5xx in past 30 min post-deploy |
| Query 8: cache hit rate | ≥ 80% if prompt caching enabled |
| Query 11: TTFT percentiles | p95 < 2,000 ms if streaming deployed |

If any check fails: do not commit the day as closed. Investigate before the STEP 12 commit.

---

## Incident response procedure

### Severity classification

| Severity | Definition | Response time |
|---|---|---|
| P1 | Gateway 100% unavailable or all requests failing | Immediate — same session |
| P2 | Error rate > 5% or p95 latency > 3× SLO target | Within 1 hour |
| P3 | Degraded quality, cache miss spike, single endpoint failure | Within 4 hours |
| P4 | YELLOW item in audit log, non-blocking warning | Next day session |

### P1 / P2 response steps

1. **Identify the scope.** Run KQL Query 1 (recent requests) and Query 4 (error rate). Is it all endpoints or one? Is it App Service, the Anthropic API, or network?

2. **Check App Service status.**
   - Portal → `app-ai-lab-api-dev-eastus-gio` → Overview: is it running?
   - Check Application Event Log (Kudu → LogFiles) for startup exceptions.

3. **Check Anthropic API status.** Visit `status.anthropic.com`. If Anthropic is down: the gateway is healthy; set a maintenance message or disable the endpoint temporarily.

4. **Roll back if a deploy caused it.**
   - Identify the last good deployment hash: `git log --oneline`
   - Rebuild and redeploy the previous commit using `/deploy`
   - Do NOT use `az webapp deploy` — use the Kudu zip path (see `.claude/skills/azure-deploy/SKILL.md`)

5. **Disable the failing endpoint if needed.** Set a feature flag or return `503` from the controller. Never leave a broken endpoint returning `500` with stack traces.

6. **Document the incident.** Create or update `docs/notes/Day-NNN/05-audit-log.md` with:
   - Timestamp of detection
   - Correlation ID from the first failing request
   - Root cause (once identified)
   - Fix applied
   - KQL query used to confirm recovery

---

## Common failure modes and fixes

### 5xx spike immediately after deploy

**Cause:** startup exception, missing app setting, or port binding failure.
**Check:** Kudu → LogFiles → `default_docker.log` (last 200 lines).
**Fix:** identify the missing config key, add it via Azure Portal App Settings, restart.

### Anthropic 401 errors

**Cause:** `Anthropic__ApiKey` app setting missing, expired, or incorrectly named.
**Check:** Portal → App Settings → confirm `Anthropic__ApiKey` exists with a valid value.
**Fix:** update the key. Restart the app. Confirm with a test request.

### Anthropic 400 errors (vague)

**Cause 1:** Account credit balance exhausted.
**Check:** Log in to `console.anthropic.com` → check billing.
**Cause 2:** Model ID invalid.
**Check:** Confirm `Anthropic__Model` is a valid current model ID (see `CLAUDE.md` Gotchas).

### Cache hit rate drop to 0%

**Cause 1:** `EnablePromptCaching` set to false.
**Cause 2:** System prompt shorter than 1,024 tokens (Anthropic minimum).
**Cause 3:** TTL missing from `cache_control` (Claude 4 models require `"ttl":"1h"`).
**Check:** Run KQL Query 8. Compare with `Anthropic__EnablePromptCaching` app setting.

### TTFT spike on streaming endpoint

**Cause 1:** Anthropic API cold start or overload.
**Cause 2:** `HttpClient` timeout too short for streaming (must be `InfiniteTimeSpan`).
**Check:** Compare `claude.chat.stream.api` span duration vs. `ai.chat.stream` span. If they match, the gateway is not adding latency — it's the provider.

### App Service restart loop

**Cause:** exception in `Program.cs` startup (usually `ValidateOnStart()` failing because a required config key is missing).
**Check:** Kudu → Process Explorer → check if `dotnet` process is running. Check `default_docker.log`.
**Fix:** add the missing app setting, wait for restart.

---

## Post-incident checklist

After a P1 or P2 incident is resolved:

- [ ] Root cause documented in `05-audit-log.md`
- [ ] The KQL query used to detect/confirm the issue added to `kql-cookbook.md` if it isn't already there
- [ ] A test case added to cover the failure path (see `testing-standard.md`)
- [ ] `CLAUDE.md` Gotchas section updated if the failure revealed a new environmental constraint
- [ ] `07-files-changed.md` updated with any files touched during the fix
