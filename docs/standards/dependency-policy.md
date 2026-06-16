# Dependency Policy

**Phase:** lower priority — active rules apply now; full enforcement Phase 2
**Applies to:** all NuGet packages in `src/lab-observability-api/lab-observability-api.csproj`

---

## When to add a dependency

A new NuGet package is justified when ALL of the following are true:

1. The functionality cannot be implemented cleanly in under ~50 lines of application code
2. The package is from a reputable source (Microsoft, a major OSS foundation, or a widely-adopted community package)
3. The package has active maintenance (last commit < 12 months, issues responded to)
4. The package does not introduce a known HIGH or CRITICAL vulnerability (`dotnet list package --vulnerable`)
5. The package's license is compatible with the repo's use (MIT, Apache 2.0, BSD are all acceptable)

Do NOT add a package to avoid writing one method. Do NOT add a package because it was used in a tutorial. The current dependency list is intentionally minimal.

---

## Evaluation checklist

Before adding any new package, answer:

- [ ] What does it do that justifies not writing it inline?
- [ ] Downloads per month on NuGet: > 100,000? (below this is a yellow flag)
- [ ] GitHub stars (if OSS): > 500?
- [ ] Last publish date: < 12 months ago?
- [ ] Is it used by Microsoft or a major cloud vendor in their own packages?
- [ ] Does `dotnet list package --vulnerable` show zero HIGH/CRITICAL after adding it?
- [ ] Is the license MIT, Apache 2.0, or BSD?

If any answer is "no" or "unknown": do not add it without documenting the exception in `07-files-changed.md` with explicit reasoning.

---

## Current approved packages (Phase 1 baseline)

These are the packages already in use. No re-evaluation required.

| Package | Purpose |
|---|---|
| `Anthropic.SDK` or direct `HttpClient` | Anthropic API transport |
| `Microsoft.Extensions.Http.Resilience` | Polly resilience pipeline |
| `OpenTelemetry.*` | Distributed tracing and metrics |
| `Azure.Monitor.OpenTelemetry.AspNetCore` | Application Insights export |
| `Serilog.AspNetCore` | Structured logging |
| `Microsoft.Extensions.Options` | Options pattern binding |

---

## Prohibited patterns

- **Do not use** `Newtonsoft.Json` — use `System.Text.Json` (built into .NET 8)
- **Do not use** individual Anthropic SDK types in provider-agnostic contracts — see `dotnet-api-conventions/SKILL.md`
- **Do not use** `log4net` or `NLog` — Serilog is the logging standard
- **Do not use** packages that require platform-specific native binaries unless Azure App Service (Linux) support is confirmed

---

## Update cadence

| Type | Cadence | Action |
|---|---|---|
| Patch versions (x.y.**Z**) | Weekly (Day NNN divisible by 5) | Update without ADR; run tests |
| Minor versions (x.**Y**.0) | Every 2 weeks | Update; run tests; check changelog for behavior changes |
| Major versions (**X**.0.0) | Review required | Do not update without reading migration guide; ADR if breaking changes affect the gateway |
| Security patches | Same day as disclosure | Update immediately; log in `07-files-changed.md` (step: `security-scan`) |

---

## Vulnerability scanning

Run at the end of every Day NNN divisible by 10:

```bash
dotnet list package --vulnerable --include-transitive
```

Results:

| Severity | Action |
|---|---|
| CRITICAL | Fix today — do not close the day without resolving |
| HIGH | Fix within 2 days |
| MEDIUM | Fix within 5 days; log as YELLOW in `05-audit-log.md` |
| LOW | Assess; log finding; fix at next dependency update cycle |

If a vulnerable package cannot be updated (pinned transitive dependency): document the CVE, the reason it cannot be updated, and the compensating control (e.g., "network-isolated, no external input reaches this code path") in `07-files-changed.md`.
