# Day 005 — AI Gateway v1

## Purpose

Day 05 introduces the first real AI integration into the existing Azure-hosted application.

The goal is not to build a toy chatbot. The goal is to evolve the Day 04 deployed API into the first version of a production-oriented **AI Gateway** with a clean provider boundary.

This marks the shift from:

- deploying applications to Azure

to:

- designing cloud-hosted AI application architecture

---

## What was already completed before Day 05

From Days 01–04, the following foundation was established:

- Azure subscription and lab environment created
- Resource group and governance baseline created
- Azure App Service deployed in East US
- .NET 8 Web API successfully hosted
- Real deployment and troubleshooting issues encountered and resolved
- Azure Well-Architected concepts reviewed and applied
- Existing application is running and reachable

This means Day 05 should build on a real deployed system, not start from scratch.

---

## Day 05 Objective

Transform the existing Day 04 API into **AI Gateway v1** by adding:

- a provider abstraction layer
- an Anthropic Claude integration
- a new API endpoint for AI chat requests
- environment-variable based configuration
- a structure that can later support Azure OpenAI and additional providers

---

## Why this matters

Enterprise AI architecture should not be tightly coupled to one model vendor.

A real architect plans for:

- provider changes
- model evolution
- configuration changes by environment
- observability
- secret handling
- future routing and fallback patterns

If AI integration is placed directly inside controller logic, the application becomes harder to maintain, harder to test, and harder to evolve.

Day 005 introduces the first proper architectural seam for model integration.

---

## What was built

The following components were added:

### Options

- `AnthropicOptions`

### Models

- `ChatRequest`
- `ChatResponse`

### Services

- `IChatModelProvider`
- `ClaudeChatModelProvider`

### Controller

- `AiController`

---

## New architectural pattern

The API now uses this flow:

1. Client sends request to `/api/ai/chat`
2. Controller receives the request
3. Controller calls `IChatModelProvider`
4. `ClaudeChatModelProvider` sends the request to Anthropic
5. Claude response is normalized into `ChatResponse`
6. API returns a provider-neutral response shape

This is intentionally designed so future providers can be added without rewriting controller logic.

---

## Configuration strategy

Anthropic configuration is externalized from code.

The application reads:

- `Anthropic__ApiKey`
- `Anthropic__Model`
- `Anthropic__BaseUrl`
- `Anthropic__MaxTokens`

These values are expected to be supplied through:

- local development configuration or user secrets
- Azure App Service environment variables in the deployed environment

No real secret should be committed to source control.

---

## Azure deployment alignment

This Day 005 work builds directly on the existing Day 04 Azure App Service deployment.

The deployed application remains:

- .NET 8 Web API
- Azure App Service (Linux)
- hosted in East US
- managed inside the Gio architecture lab subscription

The only change in Day 005 is that the hosted application now becomes capable of acting as an AI integration gateway.

---

## Risks addressed by Day 05

Before Day 005, the application had no AI abstraction and no AI provider integration.

This introduced these architectural gaps:

- no provider boundary
- no pattern for multi-model support
- no path for external model integration
- high risk of future controller-level coupling
- no reusable AI service design

Day 005 reduces those risks by introducing a clean service contract and provider-specific implementation.

---

## What beginners usually do wrong

Common beginner mistakes include:

- putting raw HTTP calls in controllers
- hard-coding API keys
- returning vendor-specific JSON directly to clients
- hard-coding model names in multiple places
- mixing transport details with business logic
- designing no path for future provider expansion

These patterns may work for demos but do not scale well in real enterprise systems.

---

## What this design does better

This design introduces:

- separation of concerns
- provider abstraction
- environment-based configuration
- cleaner testing boundaries
- future support for additional providers
- a more reviewable architecture for senior-level discussion

---

## Validation performed

Day 005 is considered successful when:

- project builds successfully
- `/health` still works
- `/api/ai/chat` works locally
- Anthropic configuration is provided through environment variables
- deployed Azure App Service can successfully process an AI request
- logs confirm provider invocation
- no secret exists in source control

---

## Portfolio significance

This is the first day where the project begins to resemble a real cloud AI architecture asset.

Day 005 demonstrates:

- Azure-hosted AI integration
- clean application design
- external provider integration
- environment-based secure configuration
- architecture evolution from basic deployment to AI service boundary

This is stronger portfolio material than simply calling an LLM from a local script.

---

## Next recommended step

Day 006 should focus on **operational maturity** for the AI gateway, including:

- structured telemetry
- latency measurement
- failure classification
- timeout handling
- retry policy
- safer error contracts
- request correlation

Day 005 establishes the boundary.
Day 006 should harden it.

---

## Summary

Day 005 transforms the existing Azure-hosted Web API into the first version of a production-oriented AI integration service.

The key outcome is not just that Claude can be called.

The real outcome is that the application now has a proper provider abstraction and an architectural path forward for multi-model enterprise AI design.
