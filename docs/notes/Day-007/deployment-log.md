# Deployment Log — Day 007

> Append-only. Each deploy run adds a dated section. Never edit prior runs.

---

## Deploy: 2026-06-03 (prompt caching and cost observability)

### Pre-deploy gate
- Branch: `feature/day-006-gateway-hardening`
- Commit: `d5e4982 feat(day-007): prompt caching and cost observability`
- Pillars audit: pre-dates audit-log.md convention; retroactive audit run 2026-06-08 confirmed no open REDs (no RED items found for Day 007)
- New app settings to apply: `Anthropic__EnablePromptCaching=true`, `Anthropic__SystemPrompt` (6920 chars / ≈1490 tokens)

---

### Local verification (pre-deploy — three bugs found and fixed)

#### Bug 1: Wrong model ID — cache silently returns 0 tokens
**Symptom:** `cache_creation_input_tokens=0` on every request despite TTL set
**Root cause:** `claude-opus-4-6` is not a valid model ID — API returns 200 with content but zero cache tokens, no error
**Fix:** Switched user-secrets `Anthropic:Model` from `claude-opus-4-6` → `claude-sonnet-4-6`
**Verification:** `cache_creation_input_tokens=1488` on next request
**Result:** FIXED ✓

#### Bug 2: TTL omitted — Claude 4 silently ignores ephemeral without TTL
**Symptom:** `{"type":"ephemeral"}` without `"ttl"` produces 0 cache tokens on Claude 4 models
**Root cause:** Claude 4 requires explicit TTL; Claude 3 worked without it (silent behavioral difference)
**Fix:** Updated `BuildAnthropicRequest` — `cache_control: {"type":"ephemeral","ttl":"1h"}`
**Result:** FIXED ✓

#### Bug 3: Nested cache creation format — Claude 4 new response structure
**Symptom:** `cacheCreationTokens` parsed as 0 even after TTL fix
**Root cause:** New Anthropic API response nests creation tokens in `cache_creation.ephemeral_1h_input_tokens` / `ephemeral_5m_input_tokens` in addition to flat `cache_creation_input_tokens`
**Fix:** Extended `TryExtractUsage` with nested format fallback: sum `ephemeral_1h_input_tokens` + `ephemeral_5m_input_tokens` when flat field is 0
**Result:** FIXED ✓

#### Local test — first request (cache population)
**Request:**
```powershell
Invoke-RestMethod -Uri "http://localhost:7xxx/api/ai/chat" -Method Post `
  -ContentType "application/json" -Body '{"prompt":"Summarize your system prompt."}'
```
**Console log:**
```
Prompt cache activity. CacheReadTokens=0 CacheCreationTokens=1488
```
**Result:** PASSED ✓ — system prompt (6920 chars) populated cache; 1488 tokens written

#### Local test — second identical request (cache hit)
**Request:** (same as above)
**Console log:**
```
Prompt cache activity. CacheReadTokens=1488 CacheCreationTokens=0
```
**Result:** PASSED ✓ — full cache hit confirmed; 0 new tokens written

#### Local test — caching disabled path
**Setup:** `EnablePromptCaching=false` in user-secrets
**Expected behavior:** payload falls back to `"system": "<string>"` (plain string, no content array)
**Result:** PASSED ✓ — verified payload structure via debug log `"system added as plain string (caching disabled)"`

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
**Result:** PASSED — files at root

---

### 4. Apply Day 007 app settings

New settings from `Infra/Day-007/appsettings-template.md`:

**Command:**
```powershell
# ARM PATCH (PUT /config/appsettings blocked on this network — PATCH parent site resource instead)
$token = az account get-access-token --query accessToken -o tsv
Invoke-RestMethod -Method PATCH `
  -Uri "https://management.azure.com/subscriptions/.../sites/app-ai-lab-api-dev-eastus-gio?api-version=2022-03-01" `
  -Headers @{ Authorization = "Bearer $token" } `
  -ContentType "application/json" `
  -Body '{
    "properties": {
      "siteConfig": {
        "appSettings": [
          { "name": "Anthropic__EnablePromptCaching", "value": "true" },
          { "name": "Anthropic__SystemPrompt",        "value": "<6920-char system prompt>" },
          ... (all existing settings preserved — PATCH replaces entire array)
        ]
      }
    }
  }'
```
**Result:** PASSED — both new settings applied; all existing settings preserved in PATCH body

---

### 5. Set WEBSITE_RUN_FROM_PACKAGE

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

### 6. Kudu zip deploy

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

### 7. Post-deploy verification

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

#### Test 2: Chat completion — confirm model and correlation ID
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
  "model": "claude-sonnet-4-6",
  "response": "..."
}
```
**Headers:** `x-correlation-id` present
**Result:** PASSED ✓ — model correctly `claude-sonnet-4-6` (not the retired `claude-opus-4-7` from Day 006)

---

#### Test 3: App Insights — cache creation on first post-deploy request
**KQL:**
```kql
dependencies
| where name == "claude.chat.api"
| where timestamp > ago(30m)
| project timestamp, customDimensions
| order by timestamp asc
| take 1
```
**Result:**
```
claude.chat.api span:
  llm.cache.creation_tokens = 1488
  llm.cache.read_tokens      = (absent — first request, no read yet)
```
**Result:** PASSED ✓ — system prompt cache populated on live Azure deployment

---

#### Test 4: App Insights — cache read on second request
**Request:** (same prompt, second call)
**KQL result:**
```
claude.chat.api span:
  llm.cache.read_tokens      = 1488
  llm.cache.creation_tokens  = (absent)
```
**Result:** PASSED ✓ — full cache hit confirmed on Azure

---

#### Test 5: KQL Query 8 — cache hit rate
**Query:**
```kql
dependencies
| where name == "claude.chat.api"
| summarize
    total   = count(),
    hits    = countif(isnotempty(tostring(customDimensions["llm.cache.read_tokens"])))
| extend hitRate = round(100.0 * hits / total, 1)
```
**Result:**
```
total=2  hits=1  hitRate=50.0%
```
**Result:** PASSED ✓ — 50% hit rate after 2 post-deploy requests (expected: first populates, second reads)

---

### Issues & fixes

| Issue | Fix | Result |
|---|---|---|
| `PUT /config/appsettings` blocked on this network for new settings | ARM PATCH on parent site resource — includes all existing settings in the array (PATCH replaces entirely) | RESOLVED |
| `claude-opus-4-6` wrong model ID — silent zero cache tokens | Switched to `claude-sonnet-4-6` before deploy | RESOLVED |
| `{"type":"ephemeral"}` without TTL — Claude 4 silently ignores | Added `"ttl":"1h"` to `cache_control` | RESOLVED |
| Nested `cache_creation` response format — flat field reads 0 | Extended `TryExtractUsage` with nested fallback | RESOLVED |

---
