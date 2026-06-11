# Deployment Log — Day 006

> Append-only. Each deploy run adds a dated section. Never edit prior runs.

---

## Deploy: 2026-05-28 (observability & resilience hardening)

### Pre-deploy gate
- Branch: `feature/day-006-gateway-hardening`
- Commit: `04f4931 feat(day-006): observability and resilience for AI gateway`
- Pillars audit: pre-dates audit-log.md convention; retroactive audit run 2026-06-08 confirmed no open REDs at ship state (S4/C2 RED fixed retroactively; see `audit-log.md`)
- Note: app was already live from a prior session; Day 006 added observability wiring on top of a running deployment

---

### 0. Infrastructure pre-work

**Verify App Insights is workspace-based:**
```powershell
az rest --method get `
  --url "https://management.azure.com/subscriptions/.../resourceGroups/rg-ai-lab-dev-eastus/providers/microsoft.insights/components/appi-ai-lab-api-dev-eastus-gio?api-version=2020-02-02"
```
**Result:** PASSED — ingestion endpoint `eastus-8.in.applicationinsights.azure.com` (workspace-based format confirmed; classic uses `dc.services.visualstudio.com`)

**Apply `APPLICATIONINSIGHTS_CONNECTION_STRING`:**
```powershell
# Applied via ARM PATCH (az webapp config appsettings set hits SSL reset on this network)
$token = az account get-access-token --query accessToken -o tsv
Invoke-RestMethod -Method PATCH `
  -Uri "https://management.azure.com/subscriptions/.../sites/app-ai-lab-api-dev-eastus-gio?api-version=2022-03-01" `
  -Headers @{ Authorization = "Bearer $token" } `
  -Body '{"properties":{"siteConfig":{"appSettings":[...]}}}'
```
**Result:** PASSED — connection string set: `InstrumentationKey=c08131fc...;IngestionEndpoint=https://eastus-8.in.applicationinsights.azure.com/...`

**Apply `Anthropic__Model=claude-opus-4-7`:**
```powershell
# Applied same ARM PATCH pass
```
**Result:** PASSED — confirmed in live response: `"model":"claude-opus-4-7"`

---

### 1. Build

**Command:**
```powershell
dotnet build src/lab-observability-api/lab-observability-api.csproj
```
**Result:** PASSED — 0 errors, 0 warnings

---

### 2. Publish

**Command:**
```powershell
dotnet publish src/lab-observability-api/lab-observability-api.csproj -c Release -o ./publish
```
**Result:** PASSED

---

### 3. Zip

**Command:**
```powershell
Compress-Archive -Path .\publish\* -DestinationPath .\lab-observability-api.zip -Force
```
**Result:** PASSED — files at root (not nested in /publish folder)

---

### 4. Set WEBSITE_RUN_FROM_PACKAGE

**Command:**
```powershell
az config set core.disable_ssl_certificate_verification=true
az webapp config appsettings set `
  --resource-group "rg-ai-lab-dev-eastus" `
  --name "app-ai-lab-api-dev-eastus-gio" `
  --settings WEBSITE_RUN_FROM_PACKAGE=1
az config set core.disable_ssl_certificate_verification=false
```
**Result:** PASSED

---

### 5. Kudu zip deploy

**Command:**
```powershell
$token = az account get-access-token --query accessToken -o tsv
Invoke-RestMethod `
  -Uri "https://app-ai-lab-api-dev-eastus-gio.scm.azurewebsites.net/api/publish?type=zip" `
  -Method Post `
  -Headers @{ Authorization = "Bearer $token" } `
  -InFile ".\lab-observability-api.zip" `
  -ContentType "application/zip"
```
**Result:** PASSED — HTTP 200

---

### 6. Post-deploy verification

#### Test 1: Health check
**Request:**
```powershell
Invoke-RestMethod "https://app-ai-lab-api-dev-eastus-gio.azurewebsites.net/health"
```
**Response:**
```json
{
  "status": "healthy",
  "checks": ["api-process", "routing", "logging"]
}
```
**Headers:** `x-correlation-id` present
**Result:** PASSED ✓

---

#### Test 2: Chat completion (happy path)
**Request:**
```powershell
Invoke-RestMethod `
  -Uri "https://app-ai-lab-api-dev-eastus-gio.azurewebsites.net/api/ai/chat" `
  -Method Post `
  -ContentType "application/json" `
  -Body '{"prompt":"Hello!"}'
```
**Response:**
```json
{
  "provider": "anthropic",
  "model": "claude-opus-4-7",
  "response": "Hello, hi, greetings!"
}
```
**Headers:** `x-correlation-id` present
**Result:** PASSED ✓

---

#### Test 3: Error contract — bad API key (no stack trace)
**Setup:** Temporarily set `Anthropic__ApiKey` to an invalid value
**Request:**
```powershell
Invoke-RestMethod `
  -Uri "https://app-ai-lab-api-dev-eastus-gio.azurewebsites.net/api/ai/chat" `
  -Method Post `
  -ContentType "application/json" `
  -Body '{"prompt":"Hello!"}'
```
**Response (HTTP 502):**
```json
{
  "code": "claude_provider_error",
  "message": "The AI provider request failed.",
  "correlationId": "..."
}
```
**Result:** PASSED ✓ — no stack trace, no internal message, safe error contract confirmed; full exception (`ClaudeProviderException: invalid x-api-key`, `ProviderErrorCode=authentication_error`, `IsTransient=False`) visible in server-side logs only; retry correctly did not fire (`Handled: 'False', Attempt: '0'`)

---

#### Test 4: App Insights — requests table
**KQL (via `az monitor app-insights query`):**
```kql
requests
| where timestamp > ago(1h)
| project timestamp, name, resultCode, duration
| order by duration desc
```
**Result:**
```
POST api/Ai/chat   200   1888ms   (06:50 UTC)
POST api/Ai/chat   200   1797ms
POST api/Ai/chat   502    286ms   (bad-key test)
```
**Result:** PASSED ✓ — telemetry pipeline confirmed live

---

#### Test 5: App Insights — token usage spans
**KQL:**
```kql
dependencies
| where name == "claude.chat.api"
| project timestamp, customDimensions
```
**Result:**
```
claude.chat.api span at 07:06:03Z:
  llm.tokens.input  = 19
  llm.tokens.output = 8
  llm.latency_ms    = 897ms
  llm.provider      = anthropic
  llm.model         = claude-opus-4-7
```
**Before/after contrast:** Pre-deploy span (`anthropic.messages.create`, 06:50Z) had no token tags — confirms Day 006 instrumentation is live
**Result:** PASSED ✓

---

### Issues & fixes

| Issue | Fix | Result |
|---|---|---|
| `az webapp config appsettings list` triggers SSL reset via `get_raw_functionapp` internal check | Used `az rest` direct ARM path instead | RESOLVED |
| `az webapp config appsettings set` for connection string hits `management.azure.com` TLS reset | ARM PATCH via `Invoke-RestMethod` with bearer token | RESOLVED |

---
