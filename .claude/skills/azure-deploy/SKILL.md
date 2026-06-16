---
name: azure-deploy
description: Deploy Lab.Observability.Api to Azure App Service using the Kudu zip publish API. Use when deploying to app-ai-lab-api-dev-eastus-gio, when `az webapp deploy` fails with connection reset errors, or any time the user mentions deploying, publishing, or pushing the AI gateway to Azure.
allowed-tools: Bash, Read, Write
---

# Azure App Service Deploy — Kudu Zip Path

## When to use

- Deploying `lab-observability-api` to App Service
- `az webapp deploy` is failing with "remote host forcibly closed"
- Need a deterministic, repeatable deploy
- Day 5+ deployment work
- Before advising on App Service deployment specifics (settings, RUN_FROM_PACKAGE behavior, Kudu endpoints), verify current behavior against Microsoft Learn via the MCP. This is an Azure-surface check only — it does not cover Anthropic API behavior.

## Why this path

`az webapp deploy` wraps multiple deploy strategies and chooses one
heuristically. On constrained networks or larger payloads it can fail
silently mid-stream with TLS resets. The Kudu publish API is the
underlying primitive — explicit, observable, idempotent when paired
with `WEBSITE_RUN_FROM_PACKAGE=1`. Elite architects prefer primitives

**Forward-Deployed angle:** a repeatable, scriptable deploy means you can
push a fix with the customer watching. A flaky deploy process is a trust
problem, not just a technical one — customers notice when you can't ship
reliably.
they can reason about over heuristic wrappers, especially in CI/CD.

## Pre-flight

- Confirm logged-in subscription: `az account show`
- Confirm RG exists: `rg-ai-lab-dev-eastus`
- Confirm App Service exists: `app-ai-lab-api-dev-eastus-gio`
- Confirm Anthropic app settings exist (do not deploy if missing):
  - `Anthropic__ApiKey`
  - `Anthropic__Model`
  - `Anthropic__BaseUrl`
  - `Anthropic__MaxTokens`

## Steps (PowerShell)

```powershell
# 1. Publish (Release configuration, output to ./publish)
dotnet publish .\lab-observability-api.csproj -c Release -o .\publish

# 2. Zip with files AT ROOT (the * is essential — do not zip the folder itself)
Compress-Archive -Path .\publish\* -DestinationPath .\lab-observability-api.zip -Force

# 3. Set run-from-package mode (idempotent — safe to re-run)
az webapp config appsettings set `
  --resource-group "rg-ai-lab-dev-eastus" `
  --name "app-ai-lab-api-dev-eastus-gio" `
  --settings WEBSITE_RUN_FROM_PACKAGE=1

# 4. Get an ARM access token (short-lived, scoped to current az session)
$token = az account get-access-token --query accessToken -o tsv

# 5. Push via Kudu publish API
Invoke-RestMethod `
  -Uri "https://app-ai-lab-api-dev-eastus-gio.scm.azurewebsites.net/api/publish?type=zip" `
  -Method Post `
  -Headers @{ Authorization = "Bearer $token" } `
  -InFile ".\lab-observability-api.zip" `
  -ContentType "application/zip"
```

## Verification (must all pass)

- `GET /health` returns 200
- `GET /health/live` returns 200 (Day 6+)
- `GET /health/ready` returns 200 with config-presence check (Day 6+)
- `GET /swagger` loads UI
- `POST /api/ai/chat` returns a Claude completion within ~3s
- Response includes `X-Correlation-Id` header (Day 6+)
- Application Insights begins receiving requests/dependencies/traces (Day 6+)

## Rollback

- `az webapp deployment list-publishing-profiles` to inspect history
- Re-deploy a previous zip artifact via the same Kudu endpoint
- Or: toggle `WEBSITE_RUN_FROM_PACKAGE` off and use slot swap (future enhancement)

## Gotchas

- **Step 3 SSL reset (`az webapp config appsettings set`):** `management.azure.com`
  hits TLS reset on this network. Fix before running step 3:

  ```powershell
  az config set core.disable_ssl_certificate_verification=true
  # ... run step 3 ...
  az config set core.disable_ssl_certificate_verification=false
  ```

  If it still resets, use `Invoke-RestMethod` with an ARM bearer token instead
  (see `L` Gotchas for the full pattern).
- **502 Bad Gateway after deploy:** check log stream
  (`az webapp log tail -g rg-ai-lab-dev-eastus -n app-ai-lab-api-dev-eastus-gio`) —
  usually a startup binding issue or missing env var.
- **401 from Claude after deploy:** confirm `Anthropic__ApiKey` app setting
  uses DOUBLE underscore, not colon. App Service translates `__` → `:` for IConfiguration.
- **Zip layout wrong:** if you see "no entry point" errors, you likely zipped the
  `publish` folder instead of its contents. Always `publish\*`.
- **Kudu 401:** your `az` token expired. Re-run step 4.
- **Don't deploy from `bin/` — always `publish/`.** They are not the same.

## Architect-level extension (future)

- Move this to GitHub Actions with OIDC federated credentials (no secrets)
- Add deployment slots (staging/production swap) for zero-downtime
- Add a smoke test step between deploy and traffic swap
- Promote `WEBSITE_RUN_FROM_PACKAGE` to a Bicep-managed setting
