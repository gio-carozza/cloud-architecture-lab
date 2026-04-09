
---

# 4. `docs/notes/Day-005/completion-checklist.md`

```md
# Day-005 — Completion Checklist

## Goal

Use this checklist to confirm Day-005 is complete both technically and architecturally.

---

## Code structure

- [x] `AnthropicOptions.cs` created
- [x] `ChatRequest.cs` created
- [x] `ChatResponse.cs` created
- [x] `IChatModelProvider.cs` created
- [x] `ClaudeChatModelProvider.cs` created
- [x] `AiController.cs` created
- [x] namespaces aligned with the real project namespace
- [x] dependency injection registration added in `Program.cs`

---

## Build and runtime

- [x] project builds successfully
- [x] application runs locally
- [x] `/health` endpoint still works
- [x] `/api/ai/chat` endpoint is reachable locally
- [x] controller validates prompt input
- [x] provider sends outbound request successfully
- [x] normalized response is returned to the caller

---

## Configuration

- [x] `Anthropic__ApiKey` configured locally through User Secrets
- [x] `Anthropic__Model` configured
- [x] `Anthropic__BaseUrl` configured
- [x] `Anthropic__MaxTokens` configured
- [x] no real secret committed to source control

---

## Azure deployment

- [x] Azure App Service environment variables added
- [x] Day-005 code deployed to existing Day-004 app
- [x] deployed `/health` endpoint still works
- [x] deployed `/api/ai/chat` can be tested on the site
- [x] application restart completed after config changes

---

## Architecture quality

- [x] provider abstraction introduced
- [x] controller does not contain vendor HTTP logic
- [x] configuration is externalized
- [x] response contract is application-owned
- [x] design can support future Azure OpenAI integration

---

## Documentation

- [x] `docs/architecture/day-005-ai-gateway-v1.md` created
- [x] `docs/adr/ADR-005-introduce-provider-abstraction-for-claude-integration.md` created
- [x] `docs/architecture/day-005-sequence-flow.md` created
- [x] `Infra/Day-005/appsettings-template.md` created
- [x] `docs/notes/Day-005/kql.md` created
- [x] `docs/notes/Day-005/deployment-guide.md` created
- [x] this checklist file committed to repo

---

## Key Day-005 answers captured

### Where are Anthropic values stored locally?

They are stored using `.NET User Secrets`.

### How is the Azure deployment performed?

The working deployment path is:

1. `dotnet publish`
2. ZIP the published output
3. set `WEBSITE_RUN_FROM_PACKAGE=1`
4. get Azure access token
5. push ZIP directly to the Kudu publish API

### What was the real cause of the `401`?

The local Anthropic API key was not yet configured correctly.

### What was the real cause of the later `400`?

The application had reached Anthropic successfully, but the Anthropic account credit balance was too low until credits were fixed.

---

## Portfolio readiness

- [x] Day-005 work is committed to GitHub
- [x] commit history is clean enough to explain
- [x] architecture decision can be discussed clearly in an interview
- [x] Day-005 shows progression from deployment to AI architecture

---

## Final Day-005 success condition

Day-005 is complete when the project is no longer just a deployed Web API.

Day-005 is complete when the project is a cloud-hosted AI gateway with a clear provider abstraction and a credible path toward multi-model enterprise architecture.