<#
.SYNOPSIS
  Audits the cloud-architecture-lab repo for context-discipline review.

.DESCRIPTION
  Walks the repo, classifies files into categories (anchor / day-artifact /
  ADR / architecture / skill / source / infra / config / other), reports
  size and last-modified, and emits:
    - Console tree (visual scan)
    - audit-report.csv (sortable detail)
    - audit-summary.md (paste-ready for chat review)

.NOTES
  Run from the repo root. Read-only — does not modify any files.
#>

[CmdletBinding()]
param(
    [string]$RepoRoot = (Get-Location).Path,
    [string]$OutputDir = (Join-Path (Get-Location).Path "audit-output")
)

$ErrorActionPreference = "Stop"

# --- Setup ---
if (-not (Test-Path $OutputDir)) {
    New-Item -ItemType Directory -Path $OutputDir | Out-Null
}

$csvPath = Join-Path $OutputDir "audit-report.csv"
$mdPath  = Join-Path $OutputDir "audit-summary.md"

Write-Host ""
Write-Host "Auditing repo at: $RepoRoot" -ForegroundColor Cyan
Write-Host "Output directory: $OutputDir" -ForegroundColor Cyan
Write-Host ""

# --- Folders to exclude (noise) ---
$excludePatterns = @(
    '\\bin\\',
    '\\obj\\',
    '\\node_modules\\',
    '\\\.git\\',
    '\\\.vs\\',
    '\\publish\\',
    '\\audit-output\\',
    '\.zip$',
    '\.user$'
)

# --- Classification logic ---
function Get-FileCategory {
    param([string]$RelativePath)

    $p = $RelativePath -replace '\\', '/'

    # Anchors (must stay in project knowledge)
    if ($p -match '^CLAUDE\.md$')                                 { return 'anchor-root' }
    if ($p -match '^src/lab-observability-api/CLAUDE\.md$')        { return 'anchor-service' }
    if ($p -match '_principles\.md$')                             { return 'anchor-principles' }
    if ($p -match 'naming-conventions\.md$')                      { return 'anchor-conventions' }
    if ($p -match 'azure-environment\.md$')                       { return 'anchor-environment' }

    # Skills (auto-invoked, must stay)
    if ($p -match '\.claude/skills/.+/SKILL\.md$')                { return 'skill' }
    if ($p -match '\.claude/commands/')                           { return 'slash-command' }

    # ADRs
    if ($p -match 'docs/adr/ADR-\d{3}-')                          { return 'adr' }

    # Architecture diagrams / sequence flows
    if ($p -match 'docs/architecture/')                           { return 'architecture' }

    # Day-scoped notes
    if ($p -match 'docs/notes/Day-(\d{3})/')                      {
        $dayNum = [int]$matches[1]
        return "day-$($dayNum.ToString('000'))-notes"
    }

    # Certification prep
    if ($p -match 'docs/certifications/')                         { return 'cert-prep' }

    # Infra-as-code
    if ($p -match '^Infra/')                                      { return 'infra' }

    # Source code
    if ($p -match '\.cs$')                                        { return 'source-cs' }
    if ($p -match '\.csproj$')                                    { return 'source-csproj' }
    if ($p -match 'appsettings.*\.json$')                         { return 'source-config' }
    if ($p -match 'launchSettings\.json$')                        { return 'source-config' }
    if ($p -match 'Program\.cs$')                                 { return 'source-cs' }

    # Scripts
    if ($p -match '\.ps1$')                                       { return 'script' }
    if ($p -match '\.sh$')                                        { return 'script' }

    # Generic markdown not otherwise classified
    if ($p -match '\.md$')                                        { return 'doc-other' }

    return 'other'
}

# Recommendation per category — keep in project knowledge or not
function Get-Recommendation {
    param([string]$Category, [string]$RelativePath)

    switch -Wildcard ($Category) {
        'anchor-*'        { return 'KEEP — hot context' }
        'skill'           { return 'KEEP — auto-invoked' }
        'slash-command'   { return 'KEEP — workflow' }
        'adr' {
            # Keep most recent ADRs only
            if ($RelativePath -match 'ADR-(00[567])') { return 'KEEP — recent ADR' }
            return 'REMOVE — older ADR (Claude Code can read on demand)'
        }
        'day-006-notes'   { return 'KEEP — current day' }
        'day-*-notes'     { return 'REMOVE — completed day' }
        'architecture'    {
            if ($RelativePath -match 'day-006') { return 'KEEP — current day arch' }
            return 'REMOVE — older architecture doc'
        }
        'cert-prep'       { return 'REMOVE — load on demand' }
        'infra'           { return 'REMOVE — Claude Code reads on demand' }
        'source-*'        { return 'REMOVE — Claude Code reads on demand' }
        'script'          { return 'REMOVE — Claude Code reads on demand' }
        'doc-other'       { return 'REVIEW manually' }
        default           { return 'REVIEW manually' }
    }
}

# --- Walk the tree ---
$allFiles = Get-ChildItem -Path $RepoRoot -Recurse -File | Where-Object {
    $fullPath = $_.FullName
    -not ($excludePatterns | Where-Object { $fullPath -match $_ })
}

Write-Host "Files found (after exclusions): $($allFiles.Count)" -ForegroundColor Green
Write-Host ""

# --- Build records ---
$records = foreach ($f in $allFiles) {
    $rel = $f.FullName.Substring($RepoRoot.Length).TrimStart('\','/')
    $cat = Get-FileCategory -RelativePath $rel
    $rec = Get-Recommendation -Category $cat -RelativePath $rel

    [PSCustomObject]@{
        RelativePath   = $rel
        Category       = $cat
        Recommendation = $rec
        SizeKB         = [math]::Round($f.Length / 1KB, 2)
        LastModified   = $f.LastWriteTime.ToString('yyyy-MM-dd')
        # Rough token estimate: ~4 chars/token for English text
        EstTokens      = if ($f.Extension -in '.md','.txt','.cs','.json','.csproj','.ps1') {
                            [math]::Round($f.Length / 4, 0)
                         } else { 0 }
    }
}

# --- Console tree (grouped) ---
Write-Host "=== CONSOLE TREE BY CATEGORY ===" -ForegroundColor Yellow
$records | Group-Object Category | Sort-Object Name | ForEach-Object {
    $catTotal = ($_.Group | Measure-Object EstTokens -Sum).Sum
    Write-Host ""
    Write-Host "[$($_.Name)] — $($_.Count) files, ~$catTotal tokens" -ForegroundColor Cyan
    foreach ($r in ($_.Group | Sort-Object RelativePath)) {
        $color = if ($r.Recommendation -like 'KEEP*') { 'Green' }
                 elseif ($r.Recommendation -like 'REMOVE*') { 'DarkGray' }
                 else { 'Yellow' }
        Write-Host ("  {0,-65} {1,8} KB  ~{2,6} tok  {3}" -f $r.RelativePath, $r.SizeKB, $r.EstTokens, $r.Recommendation) -ForegroundColor $color
    }
}

# --- CSV ---
$records | Sort-Object Category, RelativePath | Export-Csv -Path $csvPath -NoTypeInformation
Write-Host ""
Write-Host "CSV written: $csvPath" -ForegroundColor Green

# --- Markdown summary (paste-ready) ---
$keepFiles   = $records | Where-Object Recommendation -like 'KEEP*'
$removeFiles = $records | Where-Object Recommendation -like 'REMOVE*'
$reviewFiles = $records | Where-Object Recommendation -like 'REVIEW*'

$keepTokens   = ($keepFiles   | Measure-Object EstTokens -Sum).Sum
$removeTokens = ($removeFiles | Measure-Object EstTokens -Sum).Sum

$md = @()
$md += "# Repo Audit Summary"
$md += ""
$md += "Generated: $(Get-Date -Format 'yyyy-MM-dd HH:mm')"
$md += "Repo: $RepoRoot"
$md += ""
$md += "## Totals"
$md += "- Total files scanned: $($records.Count)"
$md += "- Estimated tokens (all text files): ~$([int](($records | Measure-Object EstTokens -Sum).Sum))"
$md += "- KEEP in project knowledge: $($keepFiles.Count) files (~$keepTokens tokens)"
$md += "- REMOVE from project knowledge: $($removeFiles.Count) files (~$removeTokens tokens)"
$md += "- REVIEW manually: $($reviewFiles.Count) files"
$md += ""
$md += "## KEEP (hot context)"
$md += ""
foreach ($r in ($keepFiles | Sort-Object RelativePath)) {
    $md += "- ``$($r.RelativePath)`` — $($r.Recommendation)"
}
$md += ""
$md += "## REMOVE (Claude Code reads on demand)"
$md += ""
foreach ($r in ($removeFiles | Sort-Object Category, RelativePath)) {
    $md += "- ``$($r.RelativePath)`` — $($r.Recommendation)"
}
$md += ""
$md += "## REVIEW manually"
$md += ""
foreach ($r in ($reviewFiles | Sort-Object RelativePath)) {
    $md += "- ``$($r.RelativePath)`` — $($r.Category)"
}

$md -join "`n" | Out-File -FilePath $mdPath -Encoding UTF8
Write-Host "Markdown summary written: $mdPath" -ForegroundColor Green
Write-Host ""
Write-Host "Done. Paste audit-summary.md back to me if you want a second-pass review." -ForegroundColor Cyan