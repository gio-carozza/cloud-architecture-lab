---
name: cert-scaffold
description: Scaffold certification study materials from official sources.
  Run once per exam. Fetches skills outline, creates domain folders,
  pulls resource links. Does NOT generate explanations yet — that happens
  via /cert-update as domains are touched during roadmap work.
  Usage: /cert-scaffold AZ-104
allowed-tools: Bash, Read, Write
---

# cert-scaffold

## What this does

1. Fetches the official skills outline for the given exam
2. Creates the domain folder structure under `docs/certifications/<EXAM>/`
3. Populates `README.md` per domain with name, weighting, and official
   MS Learn links
4. Creates empty concepts.md, practice-q.md, resources.md, day-mapping.md
   stubs so `/cert-update` knows what to fill
5. Updates `docs/certifications/domain-coverage.md` with the new exam row

## Source of truth (in priority order)

PRIMARY — the certification page (stable URLs, less likely to rotate):

- AZ-900: <https://learn.microsoft.com/en-us/credentials/certifications/azure-fundamentals/>
- AZ-104: <https://learn.microsoft.com/en-us/credentials/certifications/azure-administrator/>
- AZ-305: <https://learn.microsoft.com/en-us/credentials/certifications/azure-solutions-architect/>
- AI-103: <https://learn.microsoft.com/en-us/credentials/certifications/azure-ai-apps-and-agents-developer-associate/>

These pages link to the current "Study guide for Exam `<CODE>`" page, which
lists the measured skills (domains), sub-topics, and weightings. Follow that
link and parse the study guide — it is the authoritative, current domain list.

FALLBACK — skills outline PDF (binary IDs rotate; verify before trusting):

- AZ-900: <https://query.prod.cms.rt.microsoft.com/cms/api/am/binary/RE3VwUY>
- AZ-104: <https://query.prod.cms.rt.microsoft.com/cms/api/am/binary/RE4pCWy>
- AZ-305: <https://query.prod.cms.rt.microsoft.com/cms/api/am/binary/RWut8G>
- AI-103: <https://learn.microsoft.com/en-us/credentials/certifications/resources/study-guides/ai-103>
These IDs change without notice. If a PDF URL 404s or returns unexpected
content, do NOT use it — fall back to the study guide page above.

## Execution steps

1. Fetch the certification page for the given exam.
2. Find and follow the link to the current "Study guide for Exam `<CODE>`" page.
3. Extract the domain names, sub-topics, and percentage weightings from the
   study guide.
4. VERIFICATION GATE — before creating any folders, confirm:
   - At least 2 domains were extracted
   - Each domain has a name AND a weighting (or an explicit "weighting not
     stated" note)
   - The exam code on the page matches the requested exam
   If ANY of these fail, STOP. Report what was and was not found. Do NOT
   invent, guess, or backfill domain names from memory. An empty or wrong
   scaffold is worse than no scaffold.
5. Create `docs/certifications/<EXAM>/` if it does not exist.
6. For each confirmed domain, create:
   `docs/certifications/<EXAM>/domains/<NNN-kebab-name>/`
   with `README.md` (populated), and empty concepts.md, practice-q.md,
   resources.md, day-mapping.md.
7. Populate each `README.md` with:
   - Domain name and exam weighting
   - Official MS Learn module links for that domain
   - Sub-topics listed verbatim from the study guide
   - A line noting the source URL and the date fetched (domains change;
     record provenance)
8. Update `docs/certifications/domain-coverage.md`

## Idempotency

- If `docs/certifications/<EXAM>/` already exists, do NOT overwrite populated
  files. Report which domains already exist and only add missing ones.
- `README.md` may be refreshed if the study guide changed; note the change
  in the source/date line rather than silently overwriting.

## Token discipline

- Scaffold only. No explanations generated here.
- This command fetches and structures; `/cert-update` generates.
- One page fetch + one study-guide fetch per exam, not per domain.
- Do not fetch every linked MS Learn module — link to them, don't read them.
