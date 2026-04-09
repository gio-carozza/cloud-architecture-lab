# ADR-005 — Introduce Provider Abstraction for Claude Integration

## Status

Accepted

---

## Context

The application already exists as a deployed .NET 8 Web API hosted on Azure App Service.

The next phase of the roadmap introduces external LLM integration, starting with Anthropic Claude.

A design decision is required:

- Should the application call the model provider directly from controllers?
- Or should the application introduce a provider abstraction layer?

This decision is important because the roadmap is no longer Azure-only. It is now Azure-first and multi-model.

Future roadmap phases are expected to include:

- Azure OpenAI integration
- multi-model routing
- model comparison and evaluation
- provider-specific configuration by environment
- resilience and observability requirements

If provider-specific logic is embedded directly into controllers, the application will become tightly coupled to one vendor and harder to evolve.

---

## Decision

Introduce a provider abstraction using an interface:

- `IChatModelProvider`

Create a provider-specific implementation:

- `ClaudeChatModelProvider`

Controllers will depend on the abstraction rather than the vendor implementation.

Configuration will be externalized using options binding through:

- `AnthropicOptions`

The first supported provider will be Anthropic Claude.

The application response contract will be normalized into internal models instead of returning vendor-native payloads directly.

---

## Rationale

This decision supports several architectural goals.

### 1. Separation of concerns

Controllers should orchestrate requests and responses, not build provider-specific HTTP payloads.

### 2. Provider flexibility

The application should be able to support additional providers later without rewriting API endpoints.

### 3. Configuration discipline

Model name, API key, base URL, and token settings should live in configuration rather than being hard-coded in business logic.

### 4. Reviewability

A provider abstraction is easier to explain in architecture reviews and easier to evolve over time.

### 5. Future routing capability

Future routing strategies will require the application to choose among providers or models. That is much easier if providers already implement a shared contract.

---

## Alternatives considered

### Alternative 1 — Call Anthropic directly inside the controller

This approach was rejected.

Reasons:

- tightly couples controller logic to one provider
- mixes transport details with application concerns
- makes future Azure OpenAI integration harder
- reduces testability
- increases refactoring cost later

### Alternative 2 — Use static helper methods without an interface

This approach was rejected.

Reasons:

- does not establish a proper application boundary
- still limits flexibility
- makes dependency injection less useful
- creates weaker architecture for future multi-provider expansion

### Alternative 3 — Wait until multiple providers are required

This approach was rejected.

Reasons:

- defers architecture until after coupling already exists
- increases rework later
- encourages demo-first rather than system-first design

---

## Consequences

### Positive consequences

- cleaner controller design
- clearer service boundaries
- easier future integration of Azure OpenAI
- easier testing and mocking
- cleaner documentation and architecture review posture
- better portfolio value

### Negative consequences

- adds extra files and structure early
- introduces more design overhead than direct integration
- slightly slower initial implementation

These tradeoffs are acceptable because the roadmap is intentionally architect-level rather than demo-level.

---

## Implementation notes

The Day-005 implementation includes:

- `AnthropicOptions`
- `ChatRequest`
- `ChatResponse`
- `IChatModelProvider`
- `ClaudeChatModelProvider`
- `AiController`

The controller consumes `IChatModelProvider`.

The provider implementation uses configuration and HTTP integration details internally.

---

## Decision outcome

The application will use a provider abstraction for all external LLM integration going forward.

This establishes the first reusable architectural boundary for the multi-model path of the roadmap.