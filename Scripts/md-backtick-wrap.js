'use strict';
// md-backtick-wrap.js
// Wraps bare file paths, slash commands, and HTTP paths in backticks
// in markdown prose. Skips fenced code blocks, existing backtick spans,
// headings, and markdown link URLs.

const fs   = require('fs');
const path = require('path');

// ─── Code-span splitter ───────────────────────────────────────────────────────
function splitCodeSpans(line) {
  const spans = [];
  let i = 0;
  while (i < line.length) {
    if (line[i] !== '`') {
      const j = line.indexOf('`', i);
      if (j === -1) { spans.push({ code: false, text: line.slice(i) }); break; }
      spans.push({ code: false, text: line.slice(i, j) });
      i = j;
    } else {
      let j = i;
      while (j < line.length && line[j] === '`') j++;
      const ticks = j - i;
      let close = j, found = false;
      while (close < line.length) {
        if (line[close] === '`') {
          let k = close;
          while (k < line.length && line[k] === '`') k++;
          if (k - close === ticks) {
            spans.push({ code: true, text: line.slice(i, k) });
            i = k; found = true; break;
          }
          close = k;
        } else { close++; }
      }
      if (!found) { spans.push({ code: false, text: line.slice(i) }); break; }
    }
  }
  return spans;
}

// ─── Link skipper ─────────────────────────────────────────────────────────────
function skipLinks(text, fn) {
  let result = '', i = 0;
  const re = /!?\[[^\]]*\]\([^)]*\)/g;
  let m;
  while ((m = re.exec(text)) !== null) {
    result += fn(text.slice(i, m.index));
    result += m[0];
    i = m.index + m[0].length;
  }
  return result + fn(text.slice(i));
}

// ─── Argument patterns ────────────────────────────────────────────────────────
// Arg must be one of:
//   - digit sequence (day numbers):          \d+
//   - exam code:                             [A-Z]{2,3}-\d{3,4}
//   - kebab slug (requires a hyphen):        [a-z][a-z0-9]*-[a-z0-9-]+
//   - template blanks:                       _{3,}
//   - "all" keyword:                         all
// After a digit, optionally one more slug or "all".
const ARG =
  '(?:' +
    '\\d+(?:\\s+(?:all|[a-z][a-z0-9]*-[a-z0-9-]+))?' +
  '|' +
    '[A-Z]{2,3}-\\d{3,4}' +
  '|' +
    '[a-z][a-z0-9]*-[a-z0-9-]+' +
  '|' +
    '_{3,}' +
  '|' +
    'all' +
  ')';

const CMD_NAMES =
  'deploy|new-day|cert-update|cert-scaffold|adr|collab-lens|fast|code-review|ultrareview|loop|hooks|help|clear|config';

// ─── Patterns (applied in order — most specific first) ────────────────────────
// Each entry: [regex, replacer(match)]
const PATTERNS = [
  // 1. HTTP method + path
  [
    /(?<![`\w])((?:GET|POST|PUT|PATCH|DELETE) \/[A-Za-z0-9/_{}.*-]+)(?![`])/g,
    (match, g1) => `\`${g1}\``,
  ],
  // 2. Slash commands with optional technical args
  [
    new RegExp(
      `(?<![\\w\`/])(/(${CMD_NAMES})(?:\\s+${ARG})?)(?=[^a-zA-Z0-9_/-]|$)`,
      'g'
    ),
    (match, g1) => `\`${g1}\``,
  ],
  // 3. Standalone /health /swagger /api paths
  [
    /(?<![`\w/])(\/(?:health|swagger|api(?:\/[A-Za-z0-9/_{}.*-]*)?))\b/g,
    (match, g1) => `\`${g1}\``,
  ],
  // 4. Repo paths (known prefixes)
  [
    /(?<![`[\(/\w])((?:\.claude|docs|src|Infra|audit-output)\/[^\s`[\]<>()"',;|!?:\\]*)/g,
    (match, g1) => {
      let t = g1.replace(/[.,;:!?)]+$/, ''); // trim trailing sentence punct
      return `\`${t}\`` + g1.slice(t.length);
    },
  ],
  // 5. Root known files referenced in prose
  [
    /(?<![`[\(/\w])(CLAUDE\.md|README\.md|\.gitattributes|\.markdownlint\.json|\.gitignore)(?![`\w])/g,
    (match, g1) => `\`${g1}\``,
  ],
  // 6. dotnet CLI commands in prose
  [
    /(?<![`])(dotnet\s+(?:build|run|test|publish|restore|user-secrets|add|new|ef|watch)(?:\s+[^\s`.,;:!?)\]|]+)*)(?![`])/g,
    (match, g1) => `\`${g1}\``,
  ],
];

// ─── Apply all patterns (multi-pass to avoid double-wrapping) ─────────────────
function applyPatterns(text) {
  for (const [pat, replacer] of PATTERNS) {
    // Re-split code spans after each pattern to prevent touching already-wrapped content
    const spans = splitCodeSpans(text);
    text = spans.map(s => {
      if (s.code) return s.text;
      pat.lastIndex = 0;
      return s.text.replace(pat, replacer);
    }).join('');
  }
  return text;
}

// ─── Process one line ─────────────────────────────────────────────────────────
function processLine(line, inFence) {
  if (inFence) return line;
  if (/^#{1,6}\s/.test(line)) return line;          // skip headings
  if (/^-{3,}\s*$/.test(line)) return line;          // skip HR
  if (/^={3,}\s*$/.test(line)) return line;          // skip setext HR

  // Table row: process each cell independently (| splits columns)
  if (/^\s*\|/.test(line)) {
    const cells = line.split('|');
    return cells.map((cell, idx) => {
      if (idx === 0 || idx === cells.length - 1) return cell;
      const spans = splitCodeSpans(cell);
      return spans.map(s => s.code ? s.text : skipLinks(s.text, applyPatterns)).join('');
    }).join('|');
  }

  const spans = splitCodeSpans(line);
  return spans.map(s => s.code ? s.text : skipLinks(s.text, applyPatterns)).join('');
}

// ─── Process one file ─────────────────────────────────────────────────────────
function processFile(filePath) {
  const raw = fs.readFileSync(filePath);
  const hasBom  = raw[0] === 0xef && raw[1] === 0xbb && raw[2] === 0xbf;
  const hasCrlf = raw.indexOf(0x0d) !== -1;
  const str  = (hasBom ? raw.slice(3) : raw).toString('utf8');
  const lines = str.split(/\r?\n/);

  let inFence = false, inFrontmatter = false, frontmatterDone = false;
  let changed = false;

  const out = lines.map((line, idx) => {
    // Handle YAML frontmatter (--- at start of file)
    if (idx === 0 && line.trim() === '---') { inFrontmatter = true; return line; }
    if (inFrontmatter && !frontmatterDone) {
      if (line.trim() === '---') { inFrontmatter = false; frontmatterDone = true; }
      return line;
    }

    // Fence toggle
    if (/^`{3,}/.test(line)) {
      if (!inFence) { inFence = true; }
      else if (/^`{3,}\s*$/.test(line)) { inFence = false; }
      return line;
    }

    const newLine = processLine(line, inFence);
    if (newLine !== line) changed = true;
    return newLine;
  });

  if (!changed) return false;

  const sep = hasCrlf ? '\r\n' : '\n';
  const content = (hasBom ? '﻿' : '') + out.join(sep);
  fs.writeFileSync(filePath, content, 'utf8');
  return true;
}

// ─── Walk (exclude docs/architecture and node_modules) ───────────────────────
const EXCLUDE = ['node_modules', '.git'];
const changed = [];

function walk(dir) {
  for (const entry of fs.readdirSync(dir, { withFileTypes: true })) {
    const full = path.join(dir, entry.name);
    const rel  = path.relative('.', full).replace(/\\/g, '/');
    if (entry.isDirectory()) {
      if (EXCLUDE.includes(entry.name)) continue;
      if (rel === 'docs/architecture') continue;
      walk(full);
    } else if (entry.name.endsWith('.md')) {
      if (processFile(full)) changed.push(rel);
    }
  }
}

walk('.');

console.log(`\nChanged ${changed.length} files:`);
changed.forEach(f => console.log(`  ${f}`));
