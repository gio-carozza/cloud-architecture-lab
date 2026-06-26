'use strict';
// name-check.js — naming consistency companion to symbol-drift-check.js
//
// Covers the gaps symbol-drift-check.js does not:
//   1. Broader C# type/method suffix list (Service, Manager, Builder, Factory,
//      Request, Response, Context, Pipeline, etc.)
//   2. Azure resource names — backtick spans vs docs/standards/azure-environment.md
//   3. Slash command references — /command spans vs .claude/commands/
//   4. ADR-NNN references — vs docs/adr/ (gap-sequence check belongs to /repo-audit)
//
// Run after symbol-drift-check.js; the two scripts check disjoint scopes.

const fs   = require('fs');
const path = require('path');

const EXCLUDE_DIRS = ['node_modules', '.git', 'bin', 'obj'];
const EXCLUDE_DOC_DIRS = ['docs/architecture', 'docs/adr'];
const EXCLUDE_DOC_FILES = [
  /-log\.md$/,
  /docs\/notes\/changelog\.md$/,
  /docs\/standards\/commit-convention\.md$/,
  /docs\/standards\/graveyard\.md$/,
  /docs\/standards\/agent-patterns\.md$/,
  /docs\/standards\/rag-patterns\.md$/,
  /docs\/standards\/responsible-ai\.md$/,
  /docs\/standards\/multi-turn-context\.md$/,
  /docs\/standards\/competitive-intelligence\.md$/, // third-party tech landscape — names are competitors/OSS tools, not ours
];

// Files where Azure-pattern names are convention examples, planned resources, or
// historical narration — not references to live provisioned resources.
const EXCLUDE_AZURE_CHECK_FILES = new Set([
  'docs/standards/naming-conventions.md',  // naming templates and a historical change-log entry
  'docs/standards/portfolio-strategy.md',  // planned future resources not yet provisioned
]);

// Suffixes NOT already owned by symbol-drift-check.js.
// symbol-drift-check.js owns: Controller, Provider, Client, Options, Middleware,
// Telemetry, Exception, Handler, Chunk, Usage, Job, Status, Result.
const EXTENDED_SUFFIX_RE = /^I?[A-Z][A-Za-z0-9]*(?:Service|Manager|Builder|Factory|Extension|Configuration|Request|Response|Context|Pipeline|Validator|Writer|Reader|Cache|Dispatcher|Store|Adapter|Decorator|Event|Message|Query)$/;

// ADR reference pattern: ADR-NNN or ADR-NNN-kebab-title
const ADR_REF_RE = /^ADR-(\d{3})(?:-[a-z0-9-]+)?$/i;

// Azure resource name: only spans that carry our lab-specific markers (-gio suffix
// or -dev-/-eastus- segment) to avoid flagging generic kebab-case tokens.
const AZURE_OUR_MARKER = /(-gio$)|(-dev-)|([-]eastus)/;

// Slash command span: /command-name pattern
const COMMAND_SPAN_RE = /^\/[a-z][a-z0-9-]+$/;
// Only check commands that carry one of our known prefixes — avoids flagging
// HTTP paths (/health, /swagger, /v1/...) that are not slash commands.
const KNOWN_COMMAND_PREFIXES = [
  '/cert-', '/repo-audit', '/new-day', '/sync-check', '/name-audit',
  '/deploy', '/adr', '/collab-lens', '/pitch', '/devil', '/10x',
];

// Framework / BCL types with the extended suffixes — never flag these.
const BCL_ALLOWLIST = new Set([
  'IConfiguration', 'IOptions', 'IServiceCollection', 'IServiceProvider',
  'ILogger', 'ILoggerFactory', 'IActionResult', 'IFormFile',
  'IEnumerable', 'IAsyncEnumerable', 'IDisposable', 'IAsyncDisposable',
  'IHttpClientFactory', 'IHostedService', 'IHostApplicationLifetime',
  'IMemoryCache', 'IDistributedCache',
  'HttpContext', 'HttpRequest', 'HttpResponse', 'ActionResult',
  'WebApplication', 'WebApplicationBuilder', 'BackgroundService',
  'JsonSerializer', 'JsonSerializerOptions', 'JsonDocument',
  'StringBuilder', 'StreamReader', 'StreamWriter', 'MemoryStream',
  'DbContext', 'DbSet', 'SqlConnection', 'SqlCommand',
  'HttpClient', 'HttpRequestMessage', 'HttpResponseMessage', 'HttpContent',
  'MemoryCache', 'CancellationToken', 'CancellationTokenSource',
]);

// ── Helpers ─────────────────────────────────────────────────────────────────
function walk(dir, ext, excludeDirs) {
  const out = [];
  if (!fs.existsSync(dir)) return out;
  for (const entry of fs.readdirSync(dir, { withFileTypes: true })) {
    const full = path.join(dir, entry.name);
    const rel  = path.relative('.', full).replace(/\\/g, '/');
    if (entry.isDirectory()) {
      if (excludeDirs.includes(entry.name)) continue;
      out.push(...walk(full, ext, excludeDirs));
    } else if (entry.name.endsWith(ext)) {
      out.push(rel);
    }
  }
  return out;
}

function isExcluded(rel) {
  if (EXCLUDE_DOC_DIRS.some(d => rel.startsWith(d + '/'))) return true;
  return EXCLUDE_DOC_FILES.some(re => re.test(rel));
}

// ── Registry builders ────────────────────────────────────────────────────────
function buildSymbolRegistry() {
  const symbols = new Set();
  const identifierRe = /\b[A-Z][A-Za-z0-9]{2,}\b/g;

  for (const dir of ['src/lab-observability-api', 'src/lab-observability-api.Tests']) {
    for (const file of walk(dir, '.cs', EXCLUDE_DIRS)) {
      const text = fs.readFileSync(file, 'utf8');
      identifierRe.lastIndex = 0;
      let m;
      while ((m = identifierRe.exec(text)) !== null) symbols.add(m[0]);
    }
  }
  return symbols;
}

function buildAzureRegistry() {
  const resources = new Set();
  const file = 'docs/standards/azure-environment.md';
  if (!fs.existsSync(file)) return resources;

  const text = fs.readFileSync(file, 'utf8');
  // Backtick-wrapped resource names
  const btRe = /`([a-z][a-z0-9]*(?:-[a-z0-9][a-z0-9]*){2,})`/g;
  let m;
  while ((m = btRe.exec(text)) !== null) resources.add(m[1]);
  // Table cell resource names (bare, no backticks)
  const cellRe = /\|\s*([a-z][a-z0-9]*(?:-[a-z0-9][a-z0-9]*){2,})\s*\|/g;
  while ((m = cellRe.exec(text)) !== null) resources.add(m[1]);
  return resources;
}

function buildCommandRegistry() {
  const commands = new Set();
  const dir = '.claude/commands';
  if (!fs.existsSync(dir)) return commands;
  for (const f of fs.readdirSync(dir)) {
    if (f.endsWith('.md')) commands.add('/' + f.replace(/\.md$/, ''));
  }
  return commands;
}

function buildAdrRegistry() {
  const adrs = new Set();
  const dir = 'docs/adr';
  if (!fs.existsSync(dir)) return adrs;
  for (const f of fs.readdirSync(dir)) {
    const m = f.match(/^ADR-(\d{3})/i);
    if (m) adrs.add(m[1]);
  }
  return adrs;
}

// ── Scan docs ────────────────────────────────────────────────────────────────
function scanDocs(symbols, azureResources, commands, adrs) {
  const findings = [];

  for (const rel of walk('.', '.md', EXCLUDE_DIRS)) {
    if (isExcluded(rel)) continue;

    const lines = fs.readFileSync(rel, 'utf8').split(/\r?\n/);
    let inFence = false;

    lines.forEach((line, idx) => {
      if (/^`{3,}/.test(line)) { inFence = !inFence; return; }
      if (inFence) return;

      const re = /`([^`]+)`/g;
      let m;
      while ((m = re.exec(line)) !== null) {
        const span = m[1];

        // Check 1: broader C# type suffix (symbols not caught by symbol-drift-check.js)
        if (EXTENDED_SUFFIX_RE.test(span) && !BCL_ALLOWLIST.has(span) && !symbols.has(span)) {
          findings.push({ file: rel, line: idx + 1, kind: 'symbol', span,
            reason: 'symbol not found in src/ (extended suffix check)' });
          continue;
        }

        // Check 2: Azure resource names carrying our lab markers
        if (/^[a-z][a-z0-9]*(?:-[a-z0-9][a-z0-9]*){2,}$/.test(span)
            && AZURE_OUR_MARKER.test(span)
            && azureResources.size > 0
            && !azureResources.has(span)
            && !EXCLUDE_AZURE_CHECK_FILES.has(rel)) {
          findings.push({ file: rel, line: idx + 1, kind: 'azure', span,
            reason: 'Azure resource name not found in docs/standards/azure-environment.md' });
          continue;
        }

        // Check 3: slash command references
        if (COMMAND_SPAN_RE.test(span)
            && KNOWN_COMMAND_PREFIXES.some(p => span.startsWith(p))
            && commands.size > 0
            && !commands.has(span)) {
          findings.push({ file: rel, line: idx + 1, kind: 'command', span,
            reason: 'slash command not found in .claude/commands/' });
          continue;
        }

        // Check 4: ADR-NNN references
        const adrM = span.match(ADR_REF_RE);
        if (adrM && !adrs.has(adrM[1])) {
          findings.push({ file: rel, line: idx + 1, kind: 'adr-ref', span,
            reason: `docs/adr/ADR-${adrM[1]}-*.md does not exist` });
        }
      }
    });
  }
  return findings;
}

// ── Run ──────────────────────────────────────────────────────────────────────
const symbols       = buildSymbolRegistry();
const azureResources = buildAzureRegistry();
const commands      = buildCommandRegistry();
const adrs          = buildAdrRegistry();

const findings = scanDocs(symbols, azureResources, commands, adrs);

if (findings.length === 0) {
  console.log('name-check: clean — 0 findings.');
  process.exit(0);
}

console.log(`name-check: ${findings.length} finding(s):\n`);
const byFile = {};
for (const f of findings) (byFile[f.file] ||= []).push(f);
for (const [file, items] of Object.entries(byFile)) {
  console.log(file);
  for (const it of items) {
    console.log(`  line ${it.line}: \`${it.span}\` — ${it.reason}`);
  }
}
process.exit(1);
