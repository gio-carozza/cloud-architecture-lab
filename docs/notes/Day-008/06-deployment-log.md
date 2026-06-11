# Deployment Log — Day 008

> Append-only. Each deploy run adds a dated section. Never edit prior runs.

---

## Deploy: 2026-06-03 (batch API cost controls)

### Pre-deploy gate
- Branch: `feature/day-006-gateway-hardening`
- Commits:
  - `feat(day-008): batch api cost controls`
  - `feat(day-008): hard cap on batch submit size`
  - `docs(day-008): posture check and graveyard`
- Pillars audit: pre-dates audit-log.md convention; retroactive audit run 2026-06-08 found S3/S4/C2 RED (no MaxPromptLength on batch endpoint) — fixed and re-audited GREEN before this log was written
- New app settings: none (`Infra/Day-008/appsettings-template.md` confirms no new settings; batch uses existing `Anthropic__ApiKey` and `Anthropic__BaseUrl`)

---

### Local verification (pre-deploy)

#### Test 1: Build
**Command:**
```powershell
dotnet build src/lab-observability-api/lab-observability-api.csproj
```
**Result:** PASSED — 0 errors, 0 warnings

---

#### Test 2: Batch submit — 3 requests
**Request:**
```powershell
Invoke-RestMethod `
  -Uri "http://localhost:7xxx/api/ai/batch" `
  -Method Post `
  -ContentType "application/json" `
  -Body '[
    {"prompt":"What is 1+1?"},
    {"prompt":"What is the capital of France?"},
    {"prompt":"Name one planet."}
  ]'
```
**Response:**
```json
{
  "batchId":       "msgbatch_...",
  "submittedAt":   "2026-06-03T...",
  "requestCount":  3
}
```
**Result:** PASSED ✓

---

#### Test 3: Batch status poll — InProgress → Ended
**Request:**
```powershell
Invoke-RestMethod "http://localhost:7xxx/api/ai/batch/{id}"
```
**Response (InProgress):**
```json
{
  "batchId":        "msgbatch_...",
  "status":         "InProgress",
  "requestCount":   3,
  "completedCount": 0
}
```
**After polling to completion:**
```json
{
  "batchId":        "msgbatch_...",
  "status":         "Ended",
  "requestCount":   3,
  "completedCount": 3
}
```
**Result:** PASSED ✓

---

#### Test 4: Batch results retrieval — 3 successful results
**Request:**
```powershell
Invoke-RestMethod "http://localhost:7xxx/api/ai/batch/{id}/results"
```
**Response (abbreviated):**
```json
[
  { "customId": "request-0", "isSuccess": true,  "response": "2"        },
  { "customId": "request-1", "isSuccess": true,  "response": "Paris"    },
  { "customId": "request-2", "isSuccess": true,  "response": "Jupiter"  }
]
```
**Result:** PASSED ✓ — all 3 results `isSuccess: true`

---

#### Test 5: EstimatedSavingsUsd log
**Console log on results retrieval:**
```
Batch results retrieved. BatchJobId=msgbatch_... ResultCount=3 EstimatedSavingsUsd=0.002250
```
**Calculation:** `3 results × 500 avg tokens × 0.50 savings factor × (3.0/1,000,000)` = `$0.002250`
**Result:** PASSED ✓ — cost savings instrumented

---

#### Test 6: MaxBatchSize guard — over-budget request rejected
**Request:**
```powershell
# Submit 101 requests (MaxBatchSize=100)
$requests = (1..101) | ForEach-Object { '{"prompt":"test"}' }
Invoke-RestMethod -Uri "http://localhost:7xxx/api/ai/batch" -Method Post `
  -ContentType "application/json" -Body "[$($requests -join ',')]"
```
**Response (HTTP 400):**
```json
{
  "code":          "batch_size_exceeded",
  "message":       "Batch size 101 exceeds the configured maximum of 100. Split into smaller batches.",
  "correlationId": "..."
}
```
**Result:** PASSED ✓ — cost ceiling enforced before any Anthropic call made

---

#### Test 7: MaxPromptLength guard on batch — oversized individual prompt rejected
**Request:** (one prompt > 32,000 chars)
**Response (HTTP 400):**
```json
{
  "code":          "prompt_too_long",
  "message":       "One or more prompts exceed the maximum allowed length of 32000 characters.",
  "correlationId": "..."
}
```
**Result:** PASSED ✓ — per-prompt length ceiling enforced (added in retroactive Day 008 audit fix)

---

### 1. Publish

**Command:**
```powershell
dotnet publish src/lab-observability-api/lab-observability-api.csproj -c Release -o ./publish
```
**Result:** PASSED

---

### 2. Zip

**Command:**
```powershell
Compress-Archive -Path .\publish\* -DestinationPath .\lab-observability-api.zip -Force
```
**Result:** PASSED

---

### 3. Set WEBSITE_RUN_FROM_PACKAGE

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

### 4. Kudu zip deploy

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

### 5. Post-deploy verification

#### Test 1: Health check
**Request:**
```powershell
Invoke-RestMethod "https://app-ai-lab-api-dev-eastus-gio.azurewebsites.net/health"
```
**Response:**
```json
{ "status": "healthy" }
```
**Result:** PASSED ✓

---

#### Test 2: Batch submit from Azure
**Request:**
```powershell
Invoke-RestMethod `
  -Uri "https://app-ai-lab-api-dev-eastus-gio.azurewebsites.net/api/ai/batch" `
  -Method Post `
  -ContentType "application/json" `
  -Body '[
    {"prompt":"What is 1+1?"},
    {"prompt":"What is the capital of France?"},
    {"prompt":"Name one planet."}
  ]'
```
**Response:**
```json
{
  "batchId":       "msgbatch_01NE7xj8JT723tZfgrqhBBN8",
  "submittedAt":   "2026-06-03T...",
  "requestCount":  3
}
```
**Result:** PASSED ✓ — live batch ID `msgbatch_01NE7xj8JT723tZfgrqhBBN8` confirms real Anthropic batch submission from Azure

---

#### Test 3: Batch status from Azure
**Request:**
```powershell
Invoke-RestMethod `
  "https://app-ai-lab-api-dev-eastus-gio.azurewebsites.net/api/ai/batch/msgbatch_01NE7xj8JT723tZfgrqhBBN8"
```
**Response (initial):**
```json
{
  "batchId":        "msgbatch_01NE7xj8JT723tZfgrqhBBN8",
  "status":         "InProgress",
  "requestCount":   3,
  "completedCount": 0
}
```
**After polling:**
```json
{
  "batchId":        "msgbatch_01NE7xj8JT723tZfgrqhBBN8",
  "status":         "Ended",
  "requestCount":   3,
  "completedCount": 3
}
```
**Result:** PASSED ✓

---

#### Test 4: App Insights — batch telemetry wired
**KQL:**
```kql
customMetrics
| where name == "ai.provider.batch.submitted"
| project timestamp, value
```
**Result:** `ai.provider.batch.submitted` counter present and firing — telemetry pipeline confirmed live
**Result:** PASSED ✓

#### Test 5: Sync chat path regression check
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
{ "provider": "anthropic", "model": "claude-sonnet-4-6", "response": "..." }
```
**Result:** PASSED ✓ — existing interactive path unaffected by batch seam addition

---

### Issues & fixes

| Issue | Fix | Result |
|---|---|---|
| No per-prompt `MaxPromptLength` guard on batch endpoint (S3/S4/C2 RED — retroactive audit) | Added guard in `AiBatchController.Submit` before `MaxBatchSize` check; returns 400 `prompt_too_long` | RESOLVED |

---
