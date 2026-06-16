# Commit Message Convention

**Phase:** 1 (active now)
**Applies to:** all commits to this repository

---

## Format

```text
<type>(<scope>): <subject>

[optional body]

[optional footer]
Co-Authored-By: Claude Sonnet 4.6 <noreply@anthropic.com>
```

Subject line max: 72 characters. Body lines max: 100 characters. No period at end of subject.

---

## Type

| Type | When to use |
|---|---|
| `feat` | New feature, new endpoint, new provider, new ADR |
| `fix` | Bug fix |
| `docs` | Documentation only — notes, ADRs, standards, cert content |
| `chore` | Tooling, scripts, config — no production code change |
| `refactor` | Code restructured without behavior change |
| `test` | Tests added or updated, no production code change |
| `perf` | Performance improvement (caching, batching, connection reuse) |

## Scope

Always include a scope. Use one of:

| Scope | What it covers |
|---|---|
| `day-NNN` | Work produced during Day NNN (most common) |
| `gateway` | Core gateway behavior not tied to a specific day |
| `provider` | Provider abstraction or a specific provider |
| `telemetry` | Observability, spans, metrics, KQL |
| `infra` | App settings, Bicep, deploy scripts |
| `cert` | Certification study content |
| `deps` | Dependency updates |

---

## Examples

```text
feat(day-010): add multi-turn context window management

fix(gateway): propagate CancellationToken through streaming write loop

docs(day-010): posture check and graveyard

chore(day-010): repo-audit fixes

test(provider): add regression test for empty prompt validation

perf(day-009): enable prompt caching on streaming path

refactor(telemetry): rename StreamFirstTokenMs to StreamTtftMs

docs(cert): populate AZ-900 domain 003 concepts and practice questions
```

---

## Rules

- **One logical change per commit.** If you fixed a bug and updated a doc, that is two commits.
- **Never commit with failing tests.** `dotnet test` must pass before `git commit`.
- **Never commit secrets.** `appsettings.json`, `.env`, `*.pfx`, user-secrets files are all prohibited.
- **Always include the `Co-Authored-By` trailer** when Claude Code wrote or substantially edited the code.
- **Do not amend published commits.** Create a new commit to fix a previous one.
- **Squashing:** squash only within a single day's uncommitted work. Never squash across days.

---

## Day-close commit sequence

End of every day, STEP 12 produces three commits in this order:

```text
feat(day-NNN): <what was built>          ← STEP 9/10 — code and docs
docs(day-NNN): posture check and graveyard  ← STEP 12 — posture artifacts
chore(day-NNN): repo-audit fixes         ← STEP 12 — only if /repo-audit made changes
```

If only docs changed (no code): use `docs(day-NNN):` for the first commit.
