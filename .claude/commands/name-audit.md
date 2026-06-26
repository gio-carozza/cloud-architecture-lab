---
name: name-audit
description: Periodic AI-driven cross-doc naming consistency audit. Builds a canonical name registry from src/ and key docs, reads a curated file set, and identifies any entity referred to by inconsistent names across files — the check scripts cannot do. Run after significant documentation changes or class renames. STEP 12 optional.
allowed-tools: Bash, Read, Glob, Grep, Edit
---

# name-audit

## Usage

```text
/name-audit
```

No arguments. **Periodic — not required every day.** Run when:

- You've renamed a C# class, interface, or Azure resource and updated docs manually
- You've written significant new documentation (new standard, ADR, skill)
- `/sync-check` came back clean but cross-file naming feels inconsistent
- Closing a phase (end of Phase 1, Phase 2, etc.) — a good full sweep before the phase is frozen

---

## Procedure

### Step 1 — Run the scripts first

```bash
node scripts/symbol-drift-check.js
node scripts/name-check.js
```

Fix all findings before continuing. The reasoning pass below is highest signal when mechanical checks are already clean.

### Step 2 — Build the canonical name registry

Extract authoritative names from these sources:

**C# types** — from `src/lab-observability-api/**/*.cs`:
Grep for `public (abstract |sealed |partial |static )*(class|interface|record|enum|struct) [A-Z]` and extract the type name.

**C# method contracts on key seams** — read these files in full and extract all `public` method names:

- `src/lab-observability-api/Services/AI/IChatModelProvider.cs` (or wherever `IChatModelProvider` is defined)
- The `ChatRequest`, `ChatResponse`, `ChatChunk`, `ChatChunkUsage` model files

**Azure resources** — read `docs/standards/azure-environment.md` in full. Extract every named resource (App Service, App Insights workspace, Log Analytics workspace, Action Group, alert rules, resource group, subscription alias, budget name).

**Commands** — list `.claude/commands/` and extract every `/command-name`.

**ADRs** — list `docs/adr/` and extract every `ADR-NNN` number and title.

Build a flat registry table (in your working context — no file needed):

| Canonical name | Type | Source |
|---|---|---|
| `IChatModelProvider` | CSharpInterface | `src/.../IChatModelProvider.cs` |
| `ClaudeChatModelProvider` | CSharpClass | `src/.../ClaudeChatModelProvider.cs` |
| `app-ai-lab-api-dev-eastus-gio` | AzureResource | `docs/standards/azure-environment.md` |
| `/sync-check` | Command | `.claude/commands/sync-check.md` |
| `ADR-011` | ADR | `docs/adr/ADR-011-*.md` |

### Step 3 — Read the curated file set

Read these files in full:

- `CLAUDE.md`
- `docs/standards/azure-environment.md`
- `docs/standards/naming-conventions.md`
- The most recent 3 ADRs (highest `ADR-NNN` numbers in `docs/adr/`)
- Any `.claude/skills/*/SKILL.md` files that appear in the current day's changelog section
- If today has an `01-summary.md` and `03-architect-thinking.md`, read those too

### Step 4 — Identify naming inconsistencies

For each canonical name in the registry, scan the curated file set for:

**Variant spellings** — a token that is clearly trying to name a canonical entity but gets the name wrong. Examples of what to flag:

- `` `ChatProvider` `` when canonical is `IChatModelProvider`
- `` `ClaudeProvider` `` when canonical is `ClaudeChatModelProvider`
- `` `AnthropicChatProvider` `` when canonical is `ClaudeChatModelProvider`
- `` `app-ai-lab-api-eastus-gio` `` when canonical is `app-ai-lab-api-dev-eastus-gio` (missing `-dev-`)
- `` `/repo_audit` `` when canonical is `/repo-audit` (underscore instead of hyphen)
- `` `ADR-11` `` when canonical is `ADR-011` (missing zero-padding)

**Missing I-prefix on interfaces** — any backtick span ending in a known abstract-seam suffix (Provider, Service, Manager, Repository, Factory, Client) that matches a known interface name WITHOUT the I prefix. Example: `` `ChatModelProvider` `` when the interface is `IChatModelProvider`.

**Cross-file inconsistency** — the same entity is referred to by two different names across different files, where both names look plausible. This is the case the scripts cannot detect because neither name is "wrong" by suffix-matching alone — they just disagree across files. Surface these explicitly.

**Stale Azure resource names** — any Azure resource name in a doc that does not match the current live registry from `azure-environment.md`. Common after infra renames.

### Step 5 — Report and fix

For each finding, report the location and the correction:

```text
file:line: `span-used` → `canonical-name`
reason: [variant spelling | missing I-prefix | cross-file inconsistency | stale Azure resource]
```

Then apply fixes with targeted `Edit` calls. Confirm the canonical form against the registry before editing — do not guess. If unsure which name is canonical, read the source file for that entity before fixing.

If no findings: print `name-audit: clean — no cross-doc naming inconsistencies found.`

### Step 6 — Log changes

If any files were edited, upsert rows in the current day's section of `docs/notes/changelog.md`:

```markdown
| `path/to/file.md` | name-audit | corrected: `OldName` → `CanonicalName` |
```

Dedup key is file path within the day section — update in place if the row already exists.
