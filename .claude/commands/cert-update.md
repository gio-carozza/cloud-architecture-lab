---
name: cert-update
description: Populate or update certification study content for domains
  touched during today's roadmap work. Run at end of each day session.
  Reads the day's summary.md to determine which cert domains were touched,
  then generates/updates concepts.md, practice-q.md, and resources.md
  for the affected domains. Processes ONE domain per run for token control.
  Usage: /cert-update 007        (process the next un-updated domain)
         /cert-update 007 all    (process all touched domains, one at a time)
allowed-tools: Bash, Read, Write
---

# cert-update

## What this does
1. Reads docs/notes/Day-NNN/summary.md for the given day number
2. Extracts the Certification Reinforcement section to identify which exams
   and domains were touched (Primary + Secondary)
3. Builds the list of touched domains, then processes them ONE AT A TIME:
   - Default (`/cert-update NNN`): update the FIRST touched domain that has
     not yet been updated for this day, then STOP and report which domains
     remain. Re-run the command to do the next one.
   - With `all` (`/cert-update NNN all`): loop through every touched domain,
     but generate each domain's content in a separate pass and report progress
     after each. Stop early if any single domain fails.
4. For each domain processed, generate or update:
   - concepts.md  — explanations at two levels
   - practice-q.md — 5 synthesized practice questions (see cap below)
   - resources.md  — curated links from MS Learn + reputable sources
   - day-mapping.md — append this day number
5. Update docs/certifications/domain-coverage.md to mark the domain

## Why one domain per run
A single day can touch 3-4 domains across two exams. Generating concepts +
questions + resources for all of them in one pass is a large, expensive
generation. One domain per run keeps each invocation cheap and reviewable.
The `all` flag exists for when you deliberately want the full sweep.

## Content generation rules

### concepts.md format
Each concept gets four explanations — one per audience level:

---
## <Concept Name>

### If you're 10 years old
[Analogy-first. Real-world object the reader already knows.
No jargon. One paragraph.]

### If you're a CEO
[Business value, ROI, or risk framing. What does this cost or save?
What breaks if ignored? What does a competitor gain by doing this well?
Two sentences maximum.]

### If you're an Engineer
[How to actually implement it. Exact API names, SDK methods, config keys,
code patterns. What are the common errors and how do you fix them?
Two to three paragraphs.]

### If you're an Architect
[System design, tradeoffs, enterprise patterns. Why this matters at scale.
What does the wrong choice cost in 18 months? Reference exact service names.
Include "why this matters in enterprise" and "common beginner mistake".
Two to four paragraphs.]

---

### practice-q.md format
5 questions per domain update (lowered from 10 for token control —
quality over quantity; re-run on a later day to add more). Scenario-based,
not definition recall. Format:

---
## Q<N>: <Scenario headline>

**Scenario:** [2-3 sentence real-world situation]

**Question:** [What should the architect do / what is true / what
would you configure?]

A) ...
B) ...
C) ...
D) ...

**Answer:** <letter>

**Why:** [2-3 sentences explaining why the correct answer is right
AND why each wrong answer is wrong. Name the specific Azure concept
or constraint that makes it so.]

**Exam domain:** <domain name>
**Cert:** <exam code>
**Roadmap day:** Day-NNN

---

### resources.md format
Curated links only. No scraped content. Format:

---
## Official Microsoft Resources
- [Title](URL) — one-line description of what's in it

## Diagrams and Visual References
- [Title](URL) — source (MS Learn / John Savill / Adam Marczak / etc.)

## Video (≤ 20 min)
- [Title](URL) — channel, length, what it covers

---

## What NOT to do
- Do not reproduce content from MeasureUp, Whizlabs, Udemy tests,
  or any paid exam bank — link to them at most
- Do not generate more than 5 questions per domain per run
- Do not regenerate concepts.md if it already exists and the domain
  content hasn't changed — append a "Day NNN additions" section instead
- Do not process more than one domain per run unless `all` is passed

## Token discipline
- Read ONLY the day's summary.md — not the full day folder
- Generate content only for domains marked Primary or Secondary
  in that day's cert reinforcement section
- One domain per run by default; the `all` flag is the explicit opt-in
- Fetch MS Learn resource links via web search; do not fetch and
  reproduce full page content
- One pass per domain, not iterative refinement
