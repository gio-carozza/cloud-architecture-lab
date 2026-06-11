# ADR-007: Standardize `-gio` Suffix on Globally-Unique Azure Resources

## Status
Accepted

## Date
2026-04-29

## Context

While provisioning Day 6 monitoring infrastructure, the resource name
`appi-ai-lab-dev-eastus` was found to be already taken in Azure's global
Application Insights namespace. This is the second observed collision in
this lab (the first was during Day 4 App Service provisioning, mitigated
ad-hoc by appending `-gio`).

The current naming convention (`docs/standards/naming-conventions.md`)
did not explicitly require an ownership suffix, leading to inconsistent
application across resources.

Azure resource names fall into three uniqueness scopes:
- Global (DNS-backed): App Service, Storage, Key Vault, App Insights, SQL Server
- Subscription-scoped: Resource Groups, Budgets
- Resource-group-scoped: App Service Plans, Log Analytics Workspaces, Action Groups

In an enterprise context, this collision-avoidance role is played by a
3-letter organization code (e.g., `acme-`, `con-`). For this personal lab,
`gio` serves the same purpose.

## Decision

Adopt `-gio` as a mandatory suffix on all globally-unique Azure resource
types in this lab. Apply it consistently to RG-scoped resources where it
adds value (cross-resource queryability, future-proofing). Exempt resources
in namespaces the lab fully controls (subscription, resource group).

Update `docs/standards/naming-conventions.md` to encode the rule, and
update all prior-day documentation where resource names drifted.

## Alternatives Considered

### Alternative 1 — Per-resource ad-hoc suffixing
Continue resolving collisions individually as they occur.

Rejected: Already produced inconsistency between Day 4 prose and Day 4
commands. Does not scale and creates a documentation drift problem.

### Alternative 2 — UUID/random suffix
Append a short random hash to every resource (e.g., `appi-ai-lab-dev-a3f9`).

Rejected: Random suffixes are unmemorable, harm operability, and make
documentation harder to keep current. They optimize for a problem this
lab does not have.

### Alternative 3 — Region/AZ suffix only
Rely on region/AZ suffix for uniqueness (`appi-ai-lab-dev-eastus2`).

Rejected: Doesn't actually solve global uniqueness — many people pick the
same region. Conflates two different concepts (location vs. ownership).

## Consequences

### Positive
- Eliminates entire class of provisioning failures (global name collision)
- Convention is documentable, teachable, and enforceable
- Mirrors the enterprise pattern of org-code prefixes/suffixes
- Aligns prose, code, and live resources

### Negative
- One-time documentation update across multiple prior-day notes
- Marginally longer resource names

### Neutral / Tradeoffs
- `-gio` is personal-lab-specific. In a real enterprise, this becomes
  the org code. The pattern (ownership suffix) is what transfers, not
  the literal string.

## Implementation Notes

Files updated:
- `docs/standards/naming-conventions.md` (rules + change log)
- `docs/notes/Day-004/deployment.md` (drift fix)
- `docs/notes/Day-006/01-summary.md` (Phase A variable names)
- `docs/notes/Day-006/02-completion-checklist.md` (resource names)
- `CLAUDE.md` (Azure Environment block + convention summary)

Live Azure resources created/updated this day:
- `law-ai-lab-dev-eastus-gio` (Log Analytics workspace, new)
- `appi-ai-lab-api-dev-eastus-gio` (existing, reused; verified workspace-based)

No live resource renames performed (renames are destructive in Azure for
most types). Existing resources already conform to the new convention.

## References

- ADR-001 (Azure subscription adoption)
- `docs/standards/naming-conventions.md` (the convention itself)
- Azure naming rules: https://learn.microsoft.com/en-us/azure/azure-resource-manager/management/resource-name-rules

## Errata
- 2026-04-30: `naming-conventions.md` and `azure-environment.md` relocated
  from `docs/notes/Day-001/` to `docs/standards/` for lifecycle clarity.
  No content change.