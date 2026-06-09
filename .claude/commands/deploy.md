# /deploy

Deploy `lab-observability-api` to Azure App Service using the proven Kudu zip path.

## Usage
`/deploy`

## What this does

Read `.claude/skills/azure-deploy/SKILL.md` and execute the steps:

0. **Pillars audit gate** — before any build or publish step:
   - Determine the current day number from the most recent `docs/notes/Day-NNN/` folder.
   - Check `docs/notes/Day-NNN/audit-log.md` for a STEP 8 pre-deploy run with no open
     RED items (all RED items → fixes entries show re-audit → GREEN).
   - If a passing STEP 8 run exists **and** no source files (`src/`) have been modified
     since that run was appended, proceed to step 1.
   - If no audit-log.md exists or no STEP 8 run has been recorded yet, run
     `.claude/skills/pillars-audit/SKILL.md` now and append the output before continuing.
   - If the audit contains **any open RED items**, HALT immediately. List the RED items and
     the minimum fix for each. Do not proceed to step 1 until all RED items are resolved,
     the fixes are recorded in audit-log.md, and the re-audit confirms GREEN.

1. Verify pre-flight (subscription, RG, app exists)

1b. Apply day app settings: check for `Infra/Day-NNN/appsettings-template.md`
   (where NNN is the current day, or the most recent `Infra/Day-NNN/` folder if
   the day is unspecified). If the file exists, read every setting it documents
   under "New settings" and apply them to the App Service via
   `az webapp config appsettings set`, using the SSL workaround from CLAUDE.md
   Gotchas. For each setting, first check whether it is already set to the same
   value — if so, skip it (idempotent). If the file does not exist, note that no
   new settings were applied and continue. **If the file exists but any setting
   fails to apply, halt immediately and report — do not proceed to publish.**

2. Run `dotnet publish -c Release -o ./publish` from the API project
3. Zip with `Compress-Archive -Path .\publish\*` (files at root)
4. Ensure `WEBSITE_RUN_FROM_PACKAGE=1` is set
   - This calls `az webapp config appsettings set`, which hits
     `management.azure.com` and WILL reset on this network's TLS inspection.
   - BEFORE step 4, apply the SSL workaround:
     ```powershell
     az config set core.disable_ssl_certificate_verification=true
     ```
     Run step 4, then restore:
     ```powershell
     az config set core.disable_ssl_certificate_verification=false
     ```
   - If it STILL resets, use the ARM bearer-token path instead (see
     CLAUDE.md Gotchas): acquire a token with
     `az account get-access-token` and PUT the setting via
     `Invoke-RestMethod` against the ARM REST endpoint.
5. Acquire ARM token: `az account get-access-token --query accessToken -o tsv`
6. POST to Kudu publish API (uses Windows native TLS — not affected by the reset)
7. Verify post-deploy, using `Invoke-RestMethod` (NOT `curl` — the curl alias
   triggers an IE-engine security prompt on this machine):
   - `GET /health` returns 200
   - `GET /swagger` loads
   - `POST /api/ai/chat` returns a completion
   - Response carries `X-Correlation-Id` header (Day 6+)

## DO NOT
- Deploy without a clean pillars audit — no RED items may be open at the time of step 4 (publish)
- Use `az webapp deploy` (known to fail on this network)
- Zip the `publish` folder itself (must be `publish\*`)
- Deploy without verifying app settings include `Anthropic__*` keys
- Use `curl` for verification (use `Invoke-RestMethod`)
- Deploy if `appsettings-template.md` lists a setting that failed to apply —
  halt and report; never deploy code that expects a setting that isn't there

## Output
Report each step's status. If any step fails, halt and surface the exact error.
If the failure is a TLS reset on `management.azure.com`, state that explicitly
and apply the SSL workaround before retrying.
