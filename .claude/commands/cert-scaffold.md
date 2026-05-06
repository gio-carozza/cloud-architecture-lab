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
2. Creates the domain folder structure under docs/certifications/<EXAM>/
3. Populates README.md per domain with name, weighting, and official
   MS Learn links
4. Creates empty concepts.md, practice-q.md, resources.md, day-mapping.md
   stubs so /cert-update knows what to fill
5. Updates docs/certifications/domain-coverage.md with the new exam row

## Source URLs by exam
- AZ-900: https://learn.microsoft.com/en-us/credentials/certifications/azure-fundamentals/
- AZ-104: https://learn.microsoft.com/en-us/credentials/certifications/azure-administrator/
- AZ-305: https://learn.microsoft.com/en-us/credentials/certifications/azure-solutions-architect/
- AI-102: https://learn.microsoft.com/en-us/credentials/certifications/azure-ai-engineer/

## Skills outline PDFs (parse domain names and weightings from these)
- AZ-900: https://query.prod.cms.rt.microsoft.com/cms/api/am/binary/RE3VwUY
- AZ-104: https://query.prod.cms.rt.microsoft.com/cms/api/am/binary/RE4pCWy
- AZ-305: https://query.prod.cms.rt.microsoft.com/cms/api/am/binary/RWut8G
- AI-102: https://query.prod.cms.rt.microsoft.com/cms/api/am/binary/RE4wbzt

## Execution steps
1. Fetch the skills outline page for the given exam
2. Extract domain names, sub-topics, and percentage weightings
3. Create docs/certifications/<EXAM>/ if it does not exist
4. For each domain, create:
   docs/certifications/<EXAM>/domains/<NNN-kebab-name>/
   with README.md stub, empty concepts.md, practice-q.md,
   resources.md, day-mapping.md
5. Populate each README.md with:
   - Domain name and exam weighting
   - Official MS Learn module links for that domain
   - Sub-topics listed verbatim from the skills outline
6. Update docs/certifications/domain-coverage.md

## Token discipline
- Scaffold only. No explanations generated here.
- This command fetches and structures; /cert-update generates.
- One HTTP fetch per exam, not per domain.