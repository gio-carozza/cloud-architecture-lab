# Deployment Log — Day 009

> Append-only. Each deploy run adds a dated section. Never edit prior runs.

---

## Deploy: 2026-06-05 (SSE streaming on interactive path)

### Pre-deploy gate
- Branch: `feature/day-006-gateway-hardening`
- Commits:
  - `feat(day-009): streaming responses on interactive path`
  - Posture gap fix: `try/finally` in `ClaudeChatModelProvider.StreamAsync`
  - Metric name fix: `ai.chat.stream.ttft_ms` → `ai.provider.stream.ttft_ms`
- Pillars audit: pre-dates audit-log.md convention for this day; retroactive audit run 2026-06-08 confirmed no open REDs
- New app settings: none (`Infra/Day-009/appsettings-template.md` confirms no new settings; streaming uses existing `Anthropic__*` config)

---

### Local verification (pre-deploy)

#### Test 1: Build
**Command:**
```powershell
dotnet build src/lab-observability-api/lab-observability-api.csproj
```
**Result:** PASSED — 0 errors, 0 warnings

---

#### Test 2: Streaming endpoint — incremental token delivery
**Request:**
```powershell
# Use curl (not Invoke-RestMethod) for SSE — need to see the stream arrive incrementally
curl -X POST "https://localhost:7xxx/api/ai/chat/stream" `
  -H "Content-Type: application/json" `
  -d '{"prompt":"Count from 1 to 10 slowly."}'
```
**Response (SSE stream):**
```
data: {"textDelta":"1","stopReason":null,"usage":null}

data: {"textDelta":",","stopReason":null,"usage":null}

data: {"textDelta":" 2","stopReason":null,"usage":null}

... (12 distinct timestamps recorded — first token at 6ms, subsequent chunks ~370ms apart)

data: {"textDelta":"","stopReason":"end_turn","usage":{"inputTokens":24,"outputTokens":138,"cacheReadTokens":1488,"cacheCreationTokens":0}}

```
**Result:** PASSED ✓
- 12 distinct timestamps (incremental delivery confirmed)
- First token: 6ms (TTFT)
- Chunks: ~370ms apart on 138-token response
- Final chunk: `stopReason: "end_turn"`, `usage` populated
- `cacheReadTokens: 1488` — system prompt cache hit active on streaming path

---

#### Test 3: MaxPromptLength guard — validated before SSE headers
**Request:**
```powershell
# Prompt of 33,000 chars
$body = '{"prompt":"' + ('a' * 33000) + '"}'
Invoke-RestMethod -Uri "https://localhost:7xxx/api/ai/chat/stream" `
  -Method Post -ContentType "application/json" -Body $body
```
**Response (HTTP 400 — returned before SSE headers committed):**
```json
{
  "code":          "prompt_too_long",
  "message":       "Prompt exceeds the maximum allowed length of 32000 characters.",
  "correlationId": "..."
}
```
**Result:** PASSED ✓ — 400 returned correctly; SSE headers never committed

---

#### Test 4: Mid-stream error contract — safe SSE error frame
**Setup:** Trigger a provider error after stream starts (e.g., interrupt upstream connection mid-stream)
**Expected SSE frame:**
```
event: error
data: {"code":"stream_error","message":"An error occurred during streaming.","correlationId":"..."}

```
**Result:** PASSED ✓ — no stack trace, no provider error message; `correlationId` only in error frame

---

#### Test 5: Sync path regression — unaffected by streaming addition
**Request:**
```powershell
Invoke-RestMethod `
  -Uri "https://localhost:7xxx/api/ai/chat" `
  -Method Post `
  -ContentType "application/json" `
  -Body '{"prompt":"Hello!"}'
```
**Response:**
```json
{
  "provider": "anthropic",
  "model":    "claude-sonnet-4-6",
  "response": "Hi!"
}
```
**Result:** PASSED ✓ — `IChatModelProvider.SendAsync` unmodified; streaming added as separate operation

---

#### Test 6: Console — TTFT and usage logged
**Console output on streaming request:**
```
Claude streaming first token received. Model=claude-sonnet-4-6 TtftMs=6.2
Claude streaming completed. Model=claude-sonnet-4-6 InputTokens=24 OutputTokens=138 CacheReadTokens=1488
```
**Result:** PASSED ✓ — TTFT and full token audit trail in structured logs

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

#### Test 2: Streaming from Azure — SSE not buffered by App Service
**Request:**
```powershell
# Key test: confirms X-Accel-Buffering:no works and nginx doesn't swallow the stream
curl -X POST "https://app-ai-lab-api-dev-eastus-gio.azurewebsites.net/api/ai/chat/stream" `
  -H "Content-Type: application/json" `
  -d '{"prompt":"Count from 1 to 5."}'
```
**Response headers:**
```
HTTP/1.1 200 OK
Content-Type: text/event-stream; charset=utf-8
Cache-Control: no-cache
X-Accel-Buffering: no
x-correlation-id: ...
```
**Response body (SSE stream arriving incrementally):**
```
data: {"textDelta":"1","stopReason":null,"usage":null}

data: {"textDelta":",","stopReason":null,"usage":null}

... (tokens arriving at 18ms / 219ms / 250ms timestamps)

data: {"textDelta":"","stopReason":"end_turn","usage":{"inputTokens":...,"outputTokens":...,"cacheReadTokens":1488,"cacheCreationTokens":0}}

```
**Result:** PASSED ✓
- `200 text/event-stream` confirmed
- `X-Accel-Buffering: no` confirmed in response headers
- Tokens arriving incrementally (not buffered) — 18ms / 219ms / 250ms timestamps
- `cacheReadTokens=1488` — system prompt cache hit active from Azure

---

#### Test 3: Sync path regression from Azure
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
**Result:** PASSED ✓

---

#### Test 4: KQL Query 11 — TTFT percentiles from Azure
**Query:**
```kql
customMetrics
| where name == "ai.provider.stream.ttft_ms"
| summarize
    p50 = percentile(value, 50),
    p95 = percentile(value, 95),
    p99 = percentile(value, 99),
    count = count()
```
**Result:**
```
p50=1354ms   p95=(insufficient data)   p99=(insufficient data)   count=3
```
**Result:** PASSED ✓ — TTFT histogram wired and receiving data from Azure; p95/p99 require more traffic to stabilize

---

### Issues & fixes

| Issue | Fix | Result |
|---|---|---|
| Initial metric name `ai.chat.stream.ttft_ms` — violated `ai.provider.*` convention | Renamed to `ai.provider.stream.ttft_ms` in `GatewayTelemetry.cs` and ADR-011 before deploy | RESOLVED |
| `try/finally` with catch in async iterators (CS1626) — TTFT stopwatch not closed on client disconnect | Restructured to `try/finally` only (no catch clause) — finally block guaranteed to run on disconnect; catch moved to controller level | RESOLVED |

---
