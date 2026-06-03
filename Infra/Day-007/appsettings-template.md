# Day 7 — App Service Settings Template

Settings to add to `app-ai-lab-api-dev-eastus-gio` for Day 7.

## New settings

```powershell
az webapp config appsettings set `
  -g rg-ai-lab-dev-eastus `
  -n app-ai-lab-api-dev-eastus-gio `
  --settings `
    Anthropic__EnablePromptCaching=true `
    Anthropic__SystemPrompt="<paste your ≥1100-token system prompt here>"
```

## Setting descriptions

| Setting | Type | Default | Notes |
|---------|------|---------|-------|
| `Anthropic__EnablePromptCaching` | bool | `true` | Enables `cache_control: {"type":"ephemeral","ttl":"1h"}` annotation on system prompt block. Set to `false` to revert to Day 6 payload shape without redeployment. **Note:** TTL is required for Claude 4 models — bare `{"type":"ephemeral"}` produces 0 cache tokens silently. |
| `Anthropic__SystemPrompt` | string | `""` | Operational system prompt prepended to every request. **Must be ≥1024 tokens** for Anthropic to honor the cache hint. Shorter prompts are annotated but not cached — the gateway behaves correctly; caching just produces no benefit. |

## Verification

After deploy, confirm both settings are present:

```powershell
az webapp config appsettings list `
  -g rg-ai-lab-dev-eastus `
  -n app-ai-lab-api-dev-eastus-gio `
  --query "[?name=='Anthropic__EnablePromptCaching' || name=='Anthropic__SystemPrompt'].{name:name,value:value}"
```

## Inherited from prior days

These settings must already exist; do not reset them:

| Setting | Day set | Notes |
|---------|---------|-------|
| `Anthropic__ApiKey` | Day 5 | Never echoed in logs or responses |
| `Anthropic__Model` | Day 6 | `claude-sonnet-4-6` (updated from `claude-opus-4-6` — former ID does not activate prompt caching) |
| `Anthropic__MaxTokens` | Day 5 | `512` |
| `APPLICATIONINSIGHTS_CONNECTION_STRING` | Day 6 | Workspace-based App Insights |
| `WEBSITE_RUN_FROM_PACKAGE` | Day 5 | Must be `1` for Kudu zip deploy |
