---
name: cert-update
description: Populate or update certification study content for domains
  touched during today's roadmap work. Run at end of each day session.
  Reads the day's summary.md to determine which cert domains were touched,
  then generates/updates concepts.md, practice-q.md, and resources.md
  for each affected domain only.
  Usage: /cert-update 007
allowed-tools: Bash, Read, Write
---

# cert-update

## What this does
1. Reads docs/notes/Day-NNN/summary.md for the given day number
2. Extracts the Certification Reinforcement section to identify
   which exams and domains were touched (Primary + Secondary)
3. For each affected domain folder, generates or updates:
   - concepts.md  — explanations at two levels
   - practice-q.md — 10 synthesized practice questions
   - resources.md  — curated links from MS Learn + reputable sources
   - day-mapping.md — append this day number
4. Updates docs/certifications/domain-coverage.md to mark the domain

## Content generation rules

### concepts.md format
Each concept in the domain gets two explanations:

---
## <Concept Name>

### If you're 9 years old
[Analogy-first. Real-world object the reader already knows.
No jargon. One paragraph.]

### If you're an architect
[Precise technical definition. Name the tradeoffs. Reference the
Azure service or .NET API by exact name. Include "why this matters
in enterprise" and "common beginner mistake". Two to four paragraphs.]

---

### practice-q.md format
10 questions per domain update. Scenario-based (not definition recall).
Format:

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
- Do not generate more than 10 questions per domain per day run
  (token discipline — quality over quantity)
- Do not regenerate concepts.md if it already exists and the domain
  content hasn't changed — append a "Day NNN additions" section instead

## Token discipline
- Read only the day's summary.md — not the full day folder
- Generate content only for domains marked Primary or Secondary
  in that day's cert reinforcement section
- Fetch MS Learn resource links via web search; do not fetch and
  reproduce full page content
- One pass per domain, not iterative refinement