'use strict';
// symbol-drift-check.js
// Flags backtick-wrapped C# symbol names, error/telemetry code strings, and
// src/ file paths in markdown docs that no longer exist in src/. Catches both
// renamed symbols AND symbols that were documented but never actually built
// (e.g. a fictional middleware class described in a skill).
//
// Scope (precision over recall — see docs/standards or ask before widening):
//   - Only inline single-backtick spans in prose/tables are checked.
//     Fenced ```csharp code examples are NOT parsed — most are deliberately
//     abbreviated and would produce too much noise. Review those by hand.
//   - Only symbol names ending in a known "type-ish" suffix are checked
//     (Controller, Provider, Client, Options, Middleware, Telemetry,
//     Exception, Handler, Chunk, Usage, Job, Status, Result). This mirrors
//     the manual grep pattern used in the 2026-06-16 repo audit and avoids
//     flagging generic prose words.
//   - Only lowercase dotted/underscored strings (telemetry names, error
//     codes) are checked against string literals actually present in src/.
//   - This does NOT catch signature/behavior drift (same name, different
//     meaning) or prose/factual drift (stale dates, timelines).

const fs   = require('fs');
const path = require('path');

const EXCLUDE_DIRS = ['node_modules', '.git', 'bin', 'obj'];
const EXCLUDE_DOC_DIRS = ['docs/architecture', 'docs/adr'];
const EXCLUDE_DOC_FILES = [
  /-log\.md$/,                       // deployment-log.md, audit-log.md — point-in-time records
  /docs\/notes\/changelog\.md$/,     // running changelog of historical edits, may narrate old names
  /docs\/standards\/commit-convention\.md$/, // example commit messages narrate renames on purpose
  /docs\/standards\/graveyard\.md$/, // postmortem log — narrates rejected/discarded names on purpose
  /docs\/standards\/agent-patterns\.md$/,      // explicit Phase 2 stub — designed, not built
  /docs\/standards\/rag-patterns\.md$/,        // explicit Phase 2 stub — designed, not built
  /docs\/standards\/responsible-ai\.md$/,      // explicit Phase 2 stub — designed, not built
  /docs\/standards\/multi-turn-context\.md$/,  // explicit "Day 10 scope" — designed, not built yet
];

// docs/adr is excluded entirely: "Alternatives Considered" sections intentionally
// name rejected designs that were never built, and accepted ADRs can't be edited
// to "fix" this anyway (only factual corrections are allowed).

// Catches the 2026-06-16 corruption class: a buggy backtick-wrap.js replace
// callback collapsed full paths/commands down to a single character inside
// the backticks (`/health` -> `` `h` ``, `/cert-update` -> `` `c` ``, etc.),
// silently destroying real content across 50 files / 80 occurrences. A
// single-letter backtick span in prose is essentially never intentional in
// this repo (no generic-type-parameter docs use bare `T`/`K`/`V` today) —
// treat any as a near-certain truncation/corruption signal.
const SINGLE_LETTER_RE = /^[A-Za-z]$/;

const SUFFIX_RE = /^[A-Z][A-Za-z0-9]*(?:Controller|Provider|Client|Options|Middleware|Telemetry|Exception|Handler|Chunk|Usage|Job|Status|Result)$/;
// Only check OUR telemetry namespace — broader snake_case/dotted matching also
// catches Anthropic's own wire-format field names (prompt_tokens, usage.*, etc.)
// which are correct to document and aren't ours to keep in sync.
const CODE_STRING_RE = /^ai\.[a-z0-9_.]+$/;

// Well-known BCL/framework types — never flag these; we don't grep the BCL.
// Plus designs explicitly rejected in an ADR (ADR-009, ADR-011) — legitimate to
// name when explaining why they weren't chosen, in architect-thinking notes or
// cert practice questions that mirror an ADR's "Alternatives Considered" reasoning.
const BCL_ALLOWLIST = new Set([
  'NotSupportedException', 'NotImplementedException', 'InvalidOperationException',
  'ArgumentException', 'ArgumentNullException', 'ArgumentOutOfRangeException',
  'HttpRequestException', 'TaskCanceledException', 'OperationCanceledException',
  'JsonException', 'IOException', 'ILoggerProvider', 'ILoggerFactory',
  'IServiceProvider', 'IHttpClientFactory', 'IDisposable', 'IAsyncDisposable',
  'TelemetryClient',
  // rejected-by-ADR designs:
  'CachingChatModelProvider', 'IStreamingChatModelProvider',
]);

// ─── Walk helper ───────────────────────────────────────────────────────────
function walk(dir, filterExt, excludeDirs) {
  const out = [];
  for (const entry of fs.readdirSync(dir, { withFileTypes: true })) {
    const full = path.join(dir, entry.name);
    const rel  = path.relative('.', full).replace(/\\/g, '/');
    if (entry.isDirectory()) {
      if (excludeDirs.includes(entry.name)) continue;
      out.push(...walk(full, filterExt, excludeDirs));
    } else if (entry.name.endsWith(filterExt)) {
      out.push(rel);
    }
  }
  return out;
}

// ─── Step 1: build the "current truth" set from src/ ──────────────────────
function buildSourceTruth() {
  const symbolSet = new Set();
  const codeStringSet = new Set();

  const csFiles = [
    ...walk('src/lab-observability-api', '.cs', EXCLUDE_DIRS),
    ...walk('src/lab-observability-api.Tests', '.cs', EXCLUDE_DIRS),
  ];

  const identifierRe = /\b[A-Z][A-Za-z0-9]{2,}\b/g;
  const stringLiteralRe = /"([a-z][a-z0-9_.]*)"/g;

  for (const file of csFiles) {
    const text = fs.readFileSync(file, 'utf8');

    let m;
    identifierRe.lastIndex = 0;
    while ((m = identifierRe.exec(text)) !== null) symbolSet.add(m[0]);

    stringLiteralRe.lastIndex = 0;
    while ((m = stringLiteralRe.exec(text)) !== null) {
      if (m[1].includes('.') || m[1].includes('_')) codeStringSet.add(m[1]);
    }
  }

  return { symbolSet, codeStringSet };
}

// ─── Step 2: scan docs for backtick spans ──────────────────────────────────
function isExcludedDocFile(rel) {
  if (EXCLUDE_DOC_DIRS.some(d => rel.startsWith(d + '/'))) return true;
  return EXCLUDE_DOC_FILES.some(re => re.test(rel));
}

function checkPathSpan(span) {
  if (!span.startsWith('src/') && !/^(Controllers|Services|Models|Telemetry|Middleware|Options|Contracts)\//.test(span)) {
    return null; // not path-shaped
  }
  if (span.includes('*') || span.includes('...')) return null; // glob or ellipsis-abbreviated, not a literal path
  const candidates = [span, path.join('src/lab-observability-api', span)];
  const exists = candidates.some(c => fs.existsSync(c));
  return exists ? null : { kind: 'path', span };
}

function scanDocs(symbolSet, codeStringSet) {
  const mdFiles = walk('.', '.md', EXCLUDE_DIRS);
  const findings = [];

  for (const rel of mdFiles) {
    const excluded = isExcludedDocFile(rel);

    const lines = fs.readFileSync(rel, 'utf8').split(/\r?\n/);
    let inFence = false;

    lines.forEach((line, idx) => {
      if (/^`{3,}/.test(line)) { inFence = !inFence; return; }
      if (inFence) return;

      const re = /`([^`]+)`/g;
      let m;
      while ((m = re.exec(line)) !== null) {
        const span = m[1];

        // Runs on every file, including ones excluded from the checks below —
        // corruption can land anywhere, and a single-letter span is never the
        // kind of legitimate "narrates an old name on purpose" content those
        // exclusions exist for.
        if (SINGLE_LETTER_RE.test(span)) {
          findings.push({ file: rel, line: idx + 1, kind: 'truncated', span });
          continue;
        }

        if (excluded) continue;

        const pathFinding = checkPathSpan(span);
        if (pathFinding) {
          findings.push({ file: rel, line: idx + 1, ...pathFinding });
          continue;
        }

        if (SUFFIX_RE.test(span) && !symbolSet.has(span) && !BCL_ALLOWLIST.has(span)) {
          findings.push({ file: rel, line: idx + 1, kind: 'symbol', span });
          continue;
        }

        if (CODE_STRING_RE.test(span) && !codeStringSet.has(span)) {
          findings.push({ file: rel, line: idx + 1, kind: 'code', span });
        }
      }
    });
  }

  return findings;
}

// ─── Run ────────────────────────────────────────────────────────────────────
const { symbolSet, codeStringSet } = buildSourceTruth();
const findings = scanDocs(symbolSet, codeStringSet);

if (findings.length === 0) {
  console.log('symbol-drift-check: clean — 0 findings.');
  process.exit(0);
}

console.log(`symbol-drift-check: ${findings.length} finding(s):\n`);
const byFile = {};
for (const f of findings) (byFile[f.file] ||= []).push(f);

for (const [file, items] of Object.entries(byFile)) {
  console.log(file);
  for (const it of items) {
    const reason = it.kind === 'path'
      ? `path does not exist on disk`
      : it.kind === 'symbol'
        ? `symbol not found anywhere in src/`
        : it.kind === 'truncated'
          ? `single-letter backtick span — likely backtick-wrap.js corruption truncating real content (see 2026-06-16 incident); check git history to recover the original`
          : `code/telemetry string not found in any src/ string literal`;
    console.log(`  line ${it.line}: \`${it.span}\` — ${reason}`);
  }
}

process.exit(1);
