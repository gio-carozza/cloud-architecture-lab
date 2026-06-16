---
name: adr
description: Create a new Architecture Decision Record. Reads the adr-writer skill, finds the next ADR number, creates the file with standard template, sets Status to Proposed.
allowed-tools: Read, Write, Glob
---

# /adr

Create a new Architecture Decision Record.

## Usage

`/adr <kebab-case-title>`
Example: `/adr adopt-serilog-with-application-insights-sink`

## What this does

1. Read `.claude/skills/adr-writer/SKILL.md` for the template & rules
2. Find the next ADR number by scanning `docs/adr/ADR-*.md`
3. Create `docs/adr/ADR-NNN-<title>.md` with the standard template
4. Set Status: Proposed and Date: today
5. Open the file for editing

## Reminder

- Title must be verb-led
- Document at least 2 alternatives with rejection reasons
- Include negative consequences, not just upsides
