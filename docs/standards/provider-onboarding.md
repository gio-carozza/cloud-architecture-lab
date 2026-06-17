# Provider Onboarding Playbook

**Phase:** 1 (active now — abstraction exists; second provider expected Phase 2)
**Applies to:** adding any new LLM provider to the AI Gateway

---

## When to use this playbook

Any time a new LLM provider is added: Azure OpenAI, Amazon Bedrock, Azure AI Foundry, or any other model endpoint. The abstraction (`IChatModelProvider`) already exists — this playbook defines exactly what to build and verify.

---

## Pre-work: write the ADR first

Before writing any code, create an ADR using `/adr adopt-<provider-name>-provider`. The ADR must answer:

- What is the provider's API shape? (REST, SDK, gRPC)
- Does it support streaming natively? If not, how does `StreamAsync` degrade?
- Does it support prompt caching? If so, what is the mechanism?
- Does it support batch processing? If so — new `IBatchChatModelProvider` implementation or existing?
- What are the Liskov substitutability implications? (run the ADR-010 four-column test)
- What config keys does it require?

Do not proceed to implementation until the ADR is Accepted.

---

## Files to create

### Required

```text
src/lab-observability-api/
  Options/<Provider>Options.cs          — config binding (model, key, base URL, etc.)
  Services/<Provider>/<Provider>ApiClient.cs  — HTTP transport for this provider
  Services/AI/<Provider>ChatModelProvider.cs  — IChatModelProvider implementation
```

### Required if provider supports batch

```text
  Services/<Provider>/<Provider>BatchApiClient.cs
  Services/AI/<Provider>BatchChatModelProvider.cs   — IBatchChatModelProvider implementation
```

### Required: tests

```text
src/lab-observability-api.Tests/
  Controllers/AiControllerTests.cs / AiBatchControllerTests.cs — extend with a
    new <Provider> case via GatewayWebApplicationFactory + a Fake<Provider>ChatModelProvider
    at the IChatModelProvider/IBatchChatModelProvider seam (the actual pattern used
    today — see FakeChatModelProvider.cs / FakeBatchChatModelProvider.cs).
    No HttpClient-level fake exists in this repo yet — there is no <Provider>ApiClientTests.cs
    precedent; the first one to add HTTP-transport-layer unit tests sets the pattern.
```

### Required: infrastructure

```text
Infra/Day-NNN/appsettings-template.md  — document all new config keys
```

### Required: documentation

```text
docs/adr/ADR-NNN-adopt-<provider>-provider.md
docs/notes/changelog.md   — row for every file above, under the current ## Day NNN section
```

---

## Implementation checklist

### Options class

- [ ] `const string SectionName` defined
- [ ] All required properties use `required` or have a safe default
- [ ] Registered in `Program.cs` with `.ValidateDataAnnotations().ValidateOnStart()`

### API client

- [ ] Named `HttpClient` registered in `Program.cs` (separate from other providers)
- [ ] Resilience pipeline configured (timeout, circuit breaker) — do NOT reuse interactive pipeline settings without reviewing if they apply
- [ ] `CancellationToken` accepted and propagated to every HTTP call
- [ ] All provider-specific error codes mapped to safe application exceptions before they leave the client
- [ ] No Anthropic types referenced

### Provider implementation (`IChatModelProvider`)

- [ ] `SendAsync` implemented
- [ ] `StreamAsync` implemented — either natively or via default degrade (call `SendAsync`, yield one terminal `ChatChunk`)
- [ ] No provider-specific types in `ChatRequest`, `ChatResponse`, or `ChatChunk` parameters or return values
- [ ] Telemetry: `ai.chat.<provider>` outer span and `<provider>.chat.api` inner span, consistent with Day 6 two-span pattern
- [ ] Cache telemetry: `llm.cache.read_tokens` and `llm.cache.creation_tokens` tags if provider supports caching
- [ ] DI registered as keyed scoped: `services.AddKeyedScoped<IChatModelProvider, <Provider>ChatModelProvider>("<provider-key>")`

### Contract enforcement

Run these checks before closing the day:

```bash
# No provider types in agnostic contracts
grep -r "<Provider>\|<provider>" src/lab-observability-api/Models/AI/ --include="*.cs"
# Should return zero matches in ChatRequest.cs, ChatResponse.cs, ChatChunk.cs
```

---

## Routing (when multiple providers are registered)

The gateway does not yet have a routing strategy (Phase 2 scope). For now, the default `IChatModelProvider` binding in DI points to the active provider. When routing is added, the keyed registration (`AddKeyedScoped`) is already in place — the router reads the key.

Do not hard-code a routing decision in the controller. Routing belongs in a future `IProviderRouter` service.

---

## Verification checklist (run before committing)

- [ ] `dotnet build` — zero warnings, zero errors
- [ ] `dotnet test` — all tests pass
- [ ] `/health` endpoint returns 200 with new provider registered
- [ ] `POST /api/ai/chat` returns a valid `ChatResponse` using the new provider
- [ ] `POST /api/ai/chat/stream` streams tokens correctly (or degrades gracefully)
- [ ] Telemetry visible in App Insights `dependencies` table within 5 min
- [ ] `docs/notes/changelog.md` has a row for every new file, under the current day's section
- [ ] ADR status updated to `Accepted`
- [ ] `CLAUDE.md` Provider Abstraction Contract section updated if interface changed
